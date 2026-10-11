import {
    jsonEvent,
    NO_STREAM,
    StreamNotFoundError,
    WrongExpectedVersionError,
    type KurrentDBClient
} from "@kurrent/kurrentdb-client";
import { randomUUID } from "node:crypto";
import { err, ok, type Result } from "neverthrow";
import { isTeamEventType, parseTeamEvent } from "./stamp-card-events.js";
import { TeamAggregate } from "./stamp-card-model.js";
import { ConcurrentModificationError, StreamReplayError, type ReplayProblem } from "./stamp-card-errors.js";

type StoredEvent = { revision: bigint, type: string, data: unknown };

/**
 * Loads teams by replaying their event streams and saves the changes they recorded.
 * Invalid stream contents and concurrent changes are returned as results,
 * failures of the database itself are thrown.
 */
export class TeamRepository {
    constructor(private readonly eventStoreDb: KurrentDBClient) {
    }

    /**
     * Replays every stored event and collects all problems instead of stopping at the first one.
     */
    async load(streamName: string): Promise<Result<TeamAggregate, StreamReplayError>> {
        const storedEvents = await this.readStoredEvents(streamName);
        const team = new TeamAggregate(streamName, storedEvents.at(-1)?.revision);
        const problems = storedEvents.flatMap(storedEvent => replay(team, storedEvent));
        return problems.length === 0 ? ok(team) : err(new StreamReplayError(streamName, problems));
    }

    async save(team: TeamAggregate): Promise<Result<void, ConcurrentModificationError>> {
        if (team.changes.length === 0) return ok(undefined);
        try {
            await this.eventStoreDb.appendToStream(
                team.streamName,
                team.changes.map(change => jsonEvent({ id: randomUUID(), type: change.type, data: change.data })),
                { streamState: team.revision ?? NO_STREAM });
            return ok(undefined);
        } catch (error) {
            if (error instanceof WrongExpectedVersionError) {
                return err(new ConcurrentModificationError(team.streamName));
            }
            throw error;
        }
    }

    private async readStoredEvents(streamName: string): Promise<StoredEvent[]> {
        const storedEvents: StoredEvent[] = [];
        try {
            for await (const { event } of this.eventStoreDb.readStream(streamName)) {
                if (event !== undefined) {
                    storedEvents.push({ revision: event.revision, type: event.type, data: event.data });
                }
            }
        } catch (error) {
            if (!(error instanceof StreamNotFoundError)) throw error;
        }
        return storedEvents;
    }
}

function replay(team: TeamAggregate, storedEvent: StoredEvent): ReplayProblem[] {
    if (!isTeamEventType(storedEvent.type)) return [];
    return parseTeamEvent(storedEvent.type, storedEvent.data)
        .andThen(event => team.replay(event))
        .match(
            () => [],
            reason => [{ revision: storedEvent.revision, eventType: storedEvent.type, reason }]);
}
