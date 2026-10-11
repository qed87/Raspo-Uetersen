import {
    jsonEvent,
    NO_STREAM,
    StreamNotFoundError,
    WrongExpectedVersionError,
    type KurrentDBClient
} from "@kurrent/kurrentdb-client";
import { randomUUID } from "node:crypto";
import { isTeamEventType, parseTeamEvent, type TeamEvent } from "./stamp-card-events.js";
import { TeamAggregate } from "./stamp-card-model.js";
import { ConcurrentModificationError } from "./stamp-card-errors.js";

/**
 * Loads teams by replaying their event streams and saves the changes they recorded.
 */
export class TeamRepository {
    constructor(private readonly eventStoreDb: KurrentDBClient) {
    }

    async load(streamName: string): Promise<TeamAggregate> {
        const history: TeamEvent[] = [];
        let revision: bigint | undefined;
        try {
            for await (const { event } of this.eventStoreDb.readStream(streamName)) {
                if (event === undefined) continue;
                revision = event.revision;
                if (isTeamEventType(event.type)) {
                    history.push(parseTeamEvent(event.type, event.data));
                }
            }
        } catch (error) {
            if (!(error instanceof StreamNotFoundError)) throw error;
        }
        return new TeamAggregate(streamName, revision, history);
    }

    async save(team: TeamAggregate): Promise<void> {
        if (team.changes.length === 0) return;
        try {
            await this.eventStoreDb.appendToStream(
                team.streamName,
                team.changes.map(change => jsonEvent({ id: randomUUID(), type: change.type, data: change.data })),
                { streamState: team.revision ?? NO_STREAM });
        } catch (error) {
            if (error instanceof WrongExpectedVersionError) {
                throw new ConcurrentModificationError(team.streamName);
            }
            throw error;
        }
    }
}
