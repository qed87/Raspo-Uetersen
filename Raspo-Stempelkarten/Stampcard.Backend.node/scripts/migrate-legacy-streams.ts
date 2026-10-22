/**
 * Migrates the team streams of the former .NET backend into the event format of this backend.
 *
 * The legacy streams are named team-{guid without dashes} and contain PascalCase events,
 * e.g. { "Club": "raspo1926", "Name": "2017", "Issuer": ..., "IssuedOn": ... }.
 * Every legacy stream is written under the same name to the target database, the guid becomes the team id.
 * Before anything is written, each migrated stream is replayed by the TeamAggregate, so that it is known to load.
 *
 * Usage (dry run, nothing is written):
 *   npx tsx scripts/migrate-legacy-streams.ts --source kurrentdb://localhost:2113?tls=false
 * Write the migrated streams:
 *   npx tsx scripts/migrate-legacy-streams.ts --source <url> --target <url> --write
 *
 * The target must be a different database: the legacy streams belong to the category "team" as well
 * and would show up as broken teams in the all-club-teams projection.
 * Streams that already exist in the target are skipped, so the migration can be run again.
 */
import {
    jsonEvent,
    KurrentDBClient,
    NO_STREAM,
    START,
    WrongExpectedVersionError,
    type JSONType
} from "@kurrent/kurrentdb-client";
import { randomUUID } from "node:crypto";
import { parseArgs } from "node:util";
import z from "zod";
import { loadConfig } from "../src/config-loader-plugin/index.js";
import { isTeamEventType, parseTeamEvent, type TeamEvent } from "../src/stamp-card-events.js";
import { TeamAggregate } from "../src/stamp-card-model.js";

const LEGACY_STREAM_PATTERN = /^team-([0-9a-f]{32})$/;

const legacyIssuedSchema = z.object({
    Issuer: z.string(),
    IssuedOn: z.string()
});

const legacyEventSchemas = {
    TeamAdded: legacyIssuedSchema.extend({ Club: z.string(), Name: z.string() }),
    TeamUpdated: legacyIssuedSchema.extend({ Name: z.string() }),
    TeamDeleted: legacyIssuedSchema,
    CoachAdded: legacyIssuedSchema.extend({ Email: z.string() }),
    CoachRemoved: legacyIssuedSchema.extend({ Email: z.string() }),
    PlayerAdded: legacyIssuedSchema.extend({
        Id: z.uuid(),
        FirstName: z.string(),
        LastName: z.string(),
        Birthdate: z.iso.date(),
        Birthplace: z.string()
    }),
    PlayerUpdated: legacyIssuedSchema.extend({
        Id: z.uuid(),
        FirstName: z.string(),
        LastName: z.string(),
        Birthdate: z.iso.date(),
        Birthplace: z.string(),
        Active: z.boolean()
    }),
    PlayerRemoved: legacyIssuedSchema.extend({ Id: z.uuid() }),
    StampCardAdded: legacyIssuedSchema.extend({ Id: z.uuid(), PlayerId: z.uuid(), AccountingYear: z.int() }),
    StampCardRemoved: legacyIssuedSchema.extend({ Id: z.uuid() }),
    StampAdded: legacyIssuedSchema.extend({ Id: z.uuid(), StampCardId: z.uuid(), Reason: z.string() }),
    StampErased: legacyIssuedSchema.extend({ Id: z.uuid(), StampCardId: z.uuid() })
};

type LegacyEventType = keyof typeof legacyEventSchemas;

type LegacyEvent = {
    [Type in LegacyEventType]: { type: Type, data: z.infer<(typeof legacyEventSchemas)[Type]> }
}[LegacyEventType];

type StoredLegacyEvent = { id: string, revision: bigint, type: string, data: unknown, created: Date };

/**
 * A migrated event, `data` is written as is.
 */
type MigratedEvent = { id: string, type: string, data: Record<string, unknown>, source: StoredLegacyEvent };

type MigratedStream = { legacyStreamName: string, streamName: string, teamName: string, events: MigratedEvent[] };

const { values: options } = parseArgs({
    options: {
        source: { type: "string", default: "kurrentdb://localhost:2113?tls=false" },
        target: { type: "string" },
        club: { type: "string" },
        write: { type: "boolean", default: false }
    }
});

const club = options.club ?? loadConfig("res/appsettings.json").club.name;

if (options.write && options.target === undefined) {
    fail("--write requires --target.");
}
if (options.target !== undefined && options.target === options.source) {
    fail("--target must be a different database than --source.");
}

const source = KurrentDBClient.connectionString(options.source);
const target = options.target === undefined ? undefined : KurrentDBClient.connectionString(options.target);

try {
    const legacyStreams = await readLegacyStreams(source);
    console.log(`Found ${legacyStreams.size} legacy team streams in ${options.source}, club '${club}'.\n`);

    const migratedStreams = [...legacyStreams].map(([streamName, events]) => migrateStream(streamName, events));
    const invalidStreams = migratedStreams.filter(stream => !verify(stream));
    if (invalidStreams.length > 0) {
        fail(`${invalidStreams.length} streams cannot be replayed after the migration, nothing is written.`);
    }

    if (!options.write || target === undefined) {
        console.log("\nDry run, nothing is written. Pass --target <url> --write to write the streams.");
    } else {
        for (const stream of migratedStreams) {
            await write(target, stream);
        }
    }
} finally {
    await source.dispose();
    await target?.dispose();
}

/**
 * Reads $all once and groups the events of the legacy team streams, in stream order.
 */
async function readLegacyStreams(client: KurrentDBClient): Promise<Map<string, StoredLegacyEvent[]>> {
    const streams = new Map<string, StoredLegacyEvent[]>();
    for await (const { event } of client.readAll({ fromPosition: START })) {
        if (event === undefined || !LEGACY_STREAM_PATTERN.test(event.streamId)) continue;
        const events = streams.get(event.streamId) ?? [];
        events.push({ id: event.id, revision: event.revision, type: event.type, data: event.data, created: event.created });
        streams.set(event.streamId, events);
    }
    return streams;
}

function migrateStream(legacyStreamName: string, storedEvents: StoredLegacyEvent[]): MigratedStream {
    const teamId = LEGACY_STREAM_PATTERN.exec(legacyStreamName)![1]!;
    const activePlayers = new Map<string, boolean>();
    let teamName = "";
    const events = storedEvents.flatMap(storedEvent => {
        const legacyEvent = parseLegacyEvent(legacyStreamName, storedEvent);
        if (legacyEvent.type === "TeamAdded" || legacyEvent.type === "TeamUpdated") {
            teamName = legacyEvent.data.Name;
        }
        return migrateEvent(teamId, legacyEvent, activePlayers)
            .map(({ type, data }, index) => ({
                // The mapped event keeps the legacy event id, an added PlayerEnabled / PlayerDisabled gets a new one.
                id: index === 0 ? storedEvent.id : randomUUID(),
                type,
                data,
                source: storedEvent
            }));
    });
    return { legacyStreamName, streamName: `team-${teamId}`, teamName, events };
}

/**
 * Maps a legacy event to the events of this backend. The active flag of PlayerUpdated
 * becomes PlayerEnabled / PlayerDisabled, if it changes.
 */
function migrateEvent(
    teamId: string,
    event: LegacyEvent,
    activePlayers: Map<string, boolean>
): { type: string, data: Record<string, unknown> }[] {
    const issued = { issuer: event.data.Issuer, issuedOn: event.data.IssuedOn };
    switch (event.type) {
        case "TeamAdded":
            return [{ type: "TeamAdded", data: { id: teamId, club, name: event.data.Name, ...issued } }];
        case "TeamUpdated":
            return [{ type: "TeamUpdated", data: { name: event.data.Name, ...issued } }];
        case "TeamDeleted":
            return [{ type: "TeamDeleted", data: issued }];
        case "CoachAdded":
        case "CoachRemoved":
            return [{ type: event.type, data: { email: event.data.Email.toLowerCase(), ...issued } }];
        case "PlayerAdded": {
            const { Id, FirstName, LastName, Birthdate, Birthplace } = event.data;
            activePlayers.set(Id, true);
            return [{
                type: "PlayerAdded",
                data: { id: Id, firstName: FirstName, lastName: LastName, birthdate: Birthdate, birthplace: Birthplace, ...issued }
            }];
        }
        case "PlayerUpdated": {
            const { Id, FirstName, LastName, Birthdate, Birthplace, Active } = event.data;
            const events: { type: string, data: Record<string, unknown> }[] = [{
                type: "PlayerUpdated",
                data: { id: Id, firstName: FirstName, lastName: LastName, birthdate: Birthdate, birthplace: Birthplace, ...issued }
            }];
            if (activePlayers.get(Id) !== Active) {
                activePlayers.set(Id, Active);
                events.push({ type: Active ? "PlayerEnabled" : "PlayerDisabled", data: { id: Id, ...issued } });
            }
            return events;
        }
        case "PlayerRemoved":
            activePlayers.delete(event.data.Id);
            return [{ type: "PlayerRemoved", data: { id: event.data.Id, ...issued } }];
        case "StampCardAdded": {
            const { Id, PlayerId, AccountingYear } = event.data;
            return [{ type: "StampCardAdded", data: { id: Id, playerId: PlayerId, accountingYear: AccountingYear, ...issued } }];
        }
        case "StampCardRemoved":
            return [{ type: "StampCardRemoved", data: { id: event.data.Id, ...issued } }];
        case "StampAdded": {
            const { Id, StampCardId, Reason } = event.data;
            return [{ type: "StampAdded", data: { id: Id, stampCardId: StampCardId, reason: Reason, ...issued } }];
        }
        case "StampErased":
            return [{ type: "StampRemoved", data: { id: event.data.Id, stampCardId: event.data.StampCardId, ...issued } }];
    }
}

function parseLegacyEvent(streamName: string, storedEvent: StoredLegacyEvent): LegacyEvent {
    const type = storedEvent.type;
    if (!Object.hasOwn(legacyEventSchemas, type)) {
        fail(`${streamName}@${storedEvent.revision}: unknown legacy event type '${type}'.`);
    }
    const parsed = legacyEventSchemas[type as LegacyEventType].safeParse(storedEvent.data);
    if (!parsed.success) {
        fail(`${streamName}@${storedEvent.revision}: invalid ${type} (${z.prettifyError(parsed.error).replaceAll("\n", " ")}).`);
    }
    return { type, data: parsed.data } as LegacyEvent;
}

/**
 * Replays the migrated events the same way the TeamRepository loads a stream and prints the result.
 */
function verify(stream: MigratedStream): boolean {
    const team = new TeamAggregate(stream.streamName, undefined);
    const problems: string[] = [];
    const ignored = new Set<string>();
    stream.events.forEach((event, revision) => {
        if (!isTeamEventType(event.type)) {
            ignored.add(event.type);
            return;
        }
        parseTeamEvent(event.type, event.data)
            .andThen((teamEvent: TeamEvent) => team.replay(teamEvent))
            .mapErr(reason => problems.push(`  #${revision} ${event.type}: ${reason}`));
    });

    const legacyEvents = new Set(stream.events.map(event => event.source)).size;
    console.log(`${stream.legacyStreamName} -> ${stream.streamName} '${stream.teamName}': `
        + `${legacyEvents} legacy events, ${stream.events.length} migrated events`);
    if (problems.length === 0) {
        const players = team.listPlayers();
        const stampCards = team.listStampCards();
        console.log(`  ${players.length} players (${players.filter(player => player.active).length} active), `
            + `${stampCards.length} stamp cards, ${stampCards.reduce((sum, card) => sum + card.countAcceptedStamps(), 0)} stamps, `
            + `coaches: ${team.describe().coaches.join(", ")}`);
    }
    if (ignored.size > 0) {
        console.log(`  not replayed by this backend, kept for the projection: ${[...ignored].join(", ")}`);
    }
    problems.forEach(problem => console.log(problem));
    return problems.length === 0;
}

async function write(client: KurrentDBClient, stream: MigratedStream): Promise<void> {
    const events = stream.events.map(event => jsonEvent({
        id: event.id,
        type: event.type,
        data: event.data as JSONType,
        metadata: {
            migratedFrom: stream.legacyStreamName,
            legacyRevision: event.source.revision.toString(),
            legacyEventType: event.source.type,
            legacyCreated: event.source.created.toISOString()
        }
    }));
    try {
        await client.appendToStream(stream.streamName, events, { streamState: NO_STREAM });
        console.log(`Written ${stream.streamName} (${events.length} events).`);
    } catch (error) {
        if (!(error instanceof WrongExpectedVersionError)) throw error;
        console.log(`Skipped ${stream.streamName}, it already exists in the target.`);
    }
}

function fail(message: string): never {
    console.error(message);
    process.exit(1);
}
