import {
    NO_STREAM,
    StreamNotFoundError,
    WrongExpectedVersionError,
    type KurrentDBClient
} from "@kurrent/kurrentdb-client";
import type { TeamReadDto } from "../../src/stamp-card-dtos.js";

type StoredEvent = { type: string, data: unknown };

// The client's error classes have no public constructors, the code under test only checks instanceof.
function errorOfType<T extends Error>(errorClass: { prototype: T }): T {
    return Object.create(errorClass.prototype);
}

/**
 * Replaces the KurrentDB client in tests. Events are serialized to JSON like in the real store,
 * so that replaying them exercises the same parsing as in production.
 */
export class InMemoryEventStore {
    readonly streams = new Map<string, StoredEvent[]>();
    projectedTeams: TeamReadDto[] = [];

    asClient(): KurrentDBClient {
        return this as unknown as KurrentDBClient;
    }

    eventTypesOf(streamName: string): string[] {
        return (this.streams.get(streamName) ?? []).map(event => event.type);
    }

    readStream(streamName: string) {
        const events = this.streams.get(streamName);
        return (async function* () {
            if (events === undefined) throw errorOfType(StreamNotFoundError);
            for (const [revision, event] of events.entries()) {
                yield { event: { type: event.type, data: JSON.parse(JSON.stringify(event.data)), revision: BigInt(revision) } };
            }
        })();
    }

    async appendToStream(
        streamName: string,
        events: { type: string, data: unknown }[],
        options: { streamState: bigint | typeof NO_STREAM }): Promise<void> {
        const stored = this.streams.get(streamName);
        const currentRevision = stored === undefined ? NO_STREAM : BigInt(stored.length - 1);
        if (currentRevision !== options.streamState) throw errorOfType(WrongExpectedVersionError);
        this.streams.set(streamName, [...stored ?? [], ...events.map(({ type, data }) => ({ type, data }))]);
    }

    async getProjectionState() {
        return { teams: this.projectedTeams };
    }
}
