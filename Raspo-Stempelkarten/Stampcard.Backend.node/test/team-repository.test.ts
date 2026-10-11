import { describe, test } from "node:test";
import assert from "node:assert/strict";
import { ConcurrentModificationError } from "../src/stamp-card-errors.js";
import { TeamRepository } from "../src/stamp-card-repository.js";
import { coach, ISSUER, max, STREAM_NAME } from "./helpers/fixtures.js";
import { InMemoryEventStore } from "./helpers/in-memory-event-store.js";

const team2017 = { club: "raspo", name: "2017-1", coaches: [coach.email] };

describe("TeamRepository", () => {
    test("loading a missing stream returns a team that does not exist yet", async () => {
        const repository = new TeamRepository(new InMemoryEventStore().asClient());

        const team = await repository.load(STREAM_NAME);

        assert.equal(team.revision, undefined);
    });

    test("saving appends exactly the recorded events", async () => {
        const store = new InMemoryEventStore();
        const repository = new TeamRepository(store.asClient());
        const team = await repository.load(STREAM_NAME);
        team.create(team2017, ISSUER);

        await repository.save(team);

        assert.deepEqual(store.eventTypesOf(STREAM_NAME), ["TeamAdded", "CoachAdded"]);
    });

    test("a saved team is rebuilt when loaded again", async () => {
        const repository = new TeamRepository(new InMemoryEventStore().asClient());
        const team = await repository.load(STREAM_NAME);
        team.create(team2017, ISSUER);
        const playerId = team.addPlayer(max, ISSUER);
        await repository.save(team);

        const loaded = await repository.load(STREAM_NAME);

        assert.equal(loaded.findPlayer(playerId).firstName, "Max");
    });

    test("saving a team that was changed concurrently is rejected", async () => {
        const repository = new TeamRepository(new InMemoryEventStore().asClient());
        const first = await repository.load(STREAM_NAME);
        const second = await repository.load(STREAM_NAME);
        first.create(team2017, ISSUER);
        second.create(team2017, ISSUER);
        await repository.save(first);

        await assert.rejects(repository.save(second), ConcurrentModificationError);
    });

    test("saving a team without changes appends nothing", async () => {
        const store = new InMemoryEventStore();
        const repository = new TeamRepository(store.asClient());

        await repository.save(await repository.load(STREAM_NAME));

        assert.equal(store.streams.has(STREAM_NAME), false);
    });

    test("unknown event types in the stream are ignored", async () => {
        const store = new InMemoryEventStore();
        store.streams.set(STREAM_NAME, [{ type: "SomethingElse", data: {} }]);
        const repository = new TeamRepository(store.asClient());

        const team = await repository.load(STREAM_NAME);

        assert.equal(team.revision, 0n);
    });
});
