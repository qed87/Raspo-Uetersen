import { describe, test } from "node:test";
import assert from "node:assert/strict";
import { ConcurrentModificationError } from "../src/stamp-card-errors.js";
import { TeamRepository } from "../src/stamp-card-repository.js";
import { coach, expectErr, expectOk, ISSUER, max, STREAM_NAME, TEAM_ID } from "./helpers/fixtures.js";
import { InMemoryEventStore } from "./helpers/in-memory-event-store.js";

const team2017 = { club: "raspo", name: "2017-1", coaches: [coach.email] };
const issued = { issuer: ISSUER, issuedOn: "2026-10-01T10:00:00.000Z" };
const teamAdded = {
    type: "TeamAdded",
    data: { id: "11111111-1111-4111-8111-111111111111", club: "raspo", name: "2017-1", ...issued }
};

describe("loading a team", () => {
    test("a missing stream yields a team that does not exist yet", async () => {
        const repository = new TeamRepository(new InMemoryEventStore().asClient());

        const team = expectOk(await repository.load(STREAM_NAME));

        assert.equal(team.revision, undefined);
    });

    test("a saved team is rebuilt from its events", async () => {
        const repository = new TeamRepository(new InMemoryEventStore().asClient());
        const team = expectOk(await repository.load(STREAM_NAME));
        team.create(TEAM_ID, team2017, ISSUER);
        const playerId = team.addPlayer(max, ISSUER);
        expectOk(await repository.save(team));

        const loaded = expectOk(await repository.load(STREAM_NAME));

        assert.equal(loaded.findPlayer(playerId).firstName, "Max");
    });

    test("unknown event types in the stream are ignored", async () => {
        const store = new InMemoryEventStore();
        store.streams.set(STREAM_NAME, [teamAdded, { type: "SomethingElse", data: {} }]);
        const repository = new TeamRepository(store.asClient());

        const team = expectOk(await repository.load(STREAM_NAME));

        assert.equal(team.revision, 1n);
    });

    test("an event with invalid data is reported with its revision", async () => {
        const store = new InMemoryEventStore();
        store.streams.set(STREAM_NAME, [teamAdded, { type: "PlayerAdded", data: { firstName: "Max" } }]);
        const repository = new TeamRepository(store.asClient());

        const error = expectErr(await repository.load(STREAM_NAME));

        assert.deepEqual(error.problems.map(problem => [problem.revision, problem.eventType]), [[1n, "PlayerAdded"]]);
    });

    test("all problems of a stream are reported, not only the first one", async () => {
        const store = new InMemoryEventStore();
        store.streams.set(STREAM_NAME, [
            teamAdded,
            { type: "PlayerRemoved", data: { id: "22222222-2222-4222-8222-222222222222", ...issued } },
            { type: "StampAdded", data: { reason: "Training", ...issued } },
            { type: "StampCardRemoved", data: { id: "33333333-3333-4333-8333-333333333333", ...issued } }
        ]);
        const repository = new TeamRepository(store.asClient());

        const error = expectErr(await repository.load(STREAM_NAME));

        assert.deepEqual(error.problems.map(problem => problem.revision), [1n, 2n, 3n]);
    });
});

describe("saving a team", () => {
    test("appends exactly the recorded events", async () => {
        const store = new InMemoryEventStore();
        const repository = new TeamRepository(store.asClient());
        const team = expectOk(await repository.load(STREAM_NAME));
        team.create(TEAM_ID, team2017, ISSUER);

        expectOk(await repository.save(team));

        assert.deepEqual(store.eventTypesOf(STREAM_NAME), ["TeamAdded", "CoachAdded"]);
    });

    test("a team that was changed concurrently is not saved", async () => {
        const repository = new TeamRepository(new InMemoryEventStore().asClient());
        const first = expectOk(await repository.load(STREAM_NAME));
        const second = expectOk(await repository.load(STREAM_NAME));
        first.create(TEAM_ID, team2017, ISSUER);
        second.create(TEAM_ID, team2017, ISSUER);
        expectOk(await repository.save(first));

        const error = expectErr(await repository.save(second));

        assert.ok(error instanceof ConcurrentModificationError);
    });

    test("a team without changes appends nothing", async () => {
        const store = new InMemoryEventStore();
        const repository = new TeamRepository(store.asClient());

        expectOk(await repository.save(expectOk(await repository.load(STREAM_NAME))));

        assert.equal(store.streams.has(STREAM_NAME), false);
    });
});
