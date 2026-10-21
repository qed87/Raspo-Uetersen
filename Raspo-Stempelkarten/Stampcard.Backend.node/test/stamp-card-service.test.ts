import { beforeEach, describe, test } from "node:test";
import assert from "node:assert/strict";
import { TeamAlreadyExistsError, TeamNotFoundError } from "../src/stamp-card-errors.js";
import { StampCardService } from "../src/stamp-card-service.js";
import { admin, coach, max, otherCoach } from "./helpers/fixtures.js";
import { InMemoryEventStore } from "./helpers/in-memory-event-store.js";

const CLUB_NAME = "rasensport-uetersen-1926";
const TEAM_NAME = "2017-1";

let store: InMemoryEventStore;
let service: StampCardService;

function teamStream(teamId: string): string {
    return `team-${teamId}`;
}

async function createTeamWithStampCard() {
    const teamId = await service.createTeam({ name: TEAM_NAME, coaches: [coach.email] }, admin);
    const playerId = await service.addPlayer(teamId, max, coach);
    const stampCardId = await service.addStampCard(teamId, { playerId, accountingYear: 2026 }, coach);
    return { teamId, stampCardId };
}

beforeEach(() => {
    store = new InMemoryEventStore();
    service = new StampCardService(store.asClient(), CLUB_NAME);
});

describe("StampCardService", () => {
    test("a team is stored in the stream team-{team id}", async () => {
        const teamId = await service.createTeam({ name: TEAM_NAME, coaches: [] }, admin);

        assert.match(teamId, /^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/);
        assert.deepEqual([...store.streams.keys()], [teamStream(teamId)]);
    });

    test("a team belongs to the configured club", async () => {
        const teamId = await service.createTeam({ name: TEAM_NAME, coaches: [] }, admin);

        const team = await service.getTeam(teamId, admin);

        assert.equal(team.club, CLUB_NAME);
    });

    test("the configured club name is normalized", async () => {
        const otherService = new StampCardService(store.asClient(), "Rasensport Uetersen 1926");

        const teamId = await otherService.createTeam({ name: TEAM_NAME, coaches: [] }, admin);

        assert.equal((await otherService.getTeam(teamId, admin)).club, CLUB_NAME);
    });

    test("a team name that is taken in the club is rejected", async () => {
        store.projectedTeams = [{ id: "id-a", streamId: "team-id-a", club: CLUB_NAME, name: TEAM_NAME, coaches: [] }];

        await assert.rejects(service.createTeam({ name: TEAM_NAME, coaches: [] }, admin), TeamAlreadyExistsError);
        assert.equal(store.streams.size, 0);
    });

    test("a team name that is taken in another club is accepted", async () => {
        store.projectedTeams = [{ id: "id-a", streamId: "team-id-a", club: "other-club", name: TEAM_NAME, coaches: [] }];

        await service.createTeam({ name: TEAM_NAME, coaches: [] }, admin);

        assert.equal(store.streams.size, 1);
    });

    test("a team is deleted by its team id", async () => {
        const teamId = await service.createTeam({ name: TEAM_NAME, coaches: [] }, admin);

        await service.deleteTeam(teamId, admin);

        assert.deepEqual(store.eventTypesOf(teamStream(teamId)), ["TeamAdded", "TeamDeleted"]);
    });

    test("teams of other coaches are reported as not found", async () => {
        const { teamId } = await createTeamWithStampCard();

        await assert.rejects(service.listPlayers(teamId, otherCoach), TeamNotFoundError);
    });

    test("the email of the user is recorded as issuer", async () => {
        const reference = await createTeamWithStampCard();

        const stampId = await service.addStamp(reference, { reason: "Training" }, coach);

        const card = await service.getStampCard(reference, coach);
        assert.equal(card.findStamp(stampId).issuer, coach.email);
    });

    test("approving several stamps saves nothing if one of them is unknown", async () => {
        const reference = await createTeamWithStampCard();
        const sharedCode = await service.shareStampCard(reference, coach);
        const stampId = await service.submitStamp(reference, sharedCode.code, { reason: "Lauf", issuerName: "Oma" });
        const eventCount = store.eventTypesOf(teamStream(reference.teamId)).length;

        await assert.rejects(service.approveStamps(reference.teamId,
            { stampIds: [stampId, "00000000-0000-4000-8000-000000000000"] }, coach));

        assert.equal(store.eventTypesOf(teamStream(reference.teamId)).length, eventCount);
    });

    test("a stamp submitted by an external user is listed as pending", async () => {
        const reference = await createTeamWithStampCard();
        const sharedCode = await service.shareStampCard(reference, coach);
        const stampId = await service.submitStamp(reference, sharedCode.code, { reason: "Lauf", issuerName: "Oma" });

        const pending = await service.listPendingStamps(reference.teamId, coach);

        assert.deepEqual(pending.map(entry => entry.stamp.id), [stampId]);
    });

    test("coaches only see the teams they coach", async () => {
        store.projectedTeams = [
            { id: "id-a", streamId: "team-id-a", club: "raspo", name: "a", coaches: ["Coach@Raspo.de"] },
            { id: "id-b", streamId: "team-id-b", club: "raspo", name: "b", coaches: [otherCoach.email] }
        ];

        const teams = await service.listTeams(coach);

        assert.deepEqual(teams.map(team => team.name), ["a"]);
    });

    test("admins see all teams", async () => {
        store.projectedTeams = [
            { id: "id-a", streamId: "team-id-a", club: "raspo", name: "a", coaches: [coach.email] },
            { id: "id-b", streamId: "team-id-b", club: "raspo", name: "b", coaches: [otherCoach.email] }
        ];

        const teams = await service.listTeams(admin);

        assert.equal(teams.length, 2);
    });

    test("incomplete stamp cards are collected across all teams", async () => {
        const { teamId } = await createTeamWithStampCard();
        store.projectedTeams = [{ id: teamId, streamId: teamStream(teamId), club: CLUB_NAME, name: TEAM_NAME, coaches: [coach.email] }];

        const incomplete = await service.listIncompleteStampCards({ accountingYear: 2026, requiredStamps: 1 });

        assert.deepEqual(incomplete.map(card => card.player?.firstName), ["Max"]);
    });
});
