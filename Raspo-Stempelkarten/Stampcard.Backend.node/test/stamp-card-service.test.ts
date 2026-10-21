import { beforeEach, describe, test } from "node:test";
import assert from "node:assert/strict";
import { TeamNotFoundError } from "../src/stamp-card-errors.js";
import { StampCardService } from "../src/stamp-card-service.js";
import { admin, coach, max, otherCoach } from "./helpers/fixtures.js";
import { InMemoryEventStore } from "./helpers/in-memory-event-store.js";

const CLUB_NAME = "rasensport-uetersen-1926";
const TEAM_NAME = "2017-1";
const TEAM_STREAM = "team-rasensport-uetersen-1926-2017-1";

let store: InMemoryEventStore;
let service: StampCardService;

async function createTeamWithStampCard() {
    await service.createTeam({ name: TEAM_NAME, coaches: [coach.email] }, admin);
    const playerId = await service.addPlayer(TEAM_NAME, max, coach);
    const stampCardId = await service.addStampCard(TEAM_NAME, { playerId, accountingYear: 2026 }, coach);
    return { teamName: TEAM_NAME, stampCardId };
}

beforeEach(() => {
    store = new InMemoryEventStore();
    service = new StampCardService(store.asClient(), CLUB_NAME);
});

describe("StampCardService", () => {
    test("a team is stored in the stream team-{club}-{team name}", async () => {
        await service.createTeam({ name: TEAM_NAME, coaches: [] }, admin);

        assert.deepEqual([...store.streams.keys()], [TEAM_STREAM]);
    });

    test("a team belongs to the configured club", async () => {
        await service.createTeam({ name: TEAM_NAME, coaches: [] }, admin);

        const team = await service.getTeam(TEAM_NAME, admin);

        assert.equal(team.club, CLUB_NAME);
    });

    test("the configured club name is normalized", async () => {
        const otherService = new StampCardService(store.asClient(), "Rasensport Uetersen 1926");

        await otherService.createTeam({ name: TEAM_NAME, coaches: [] }, admin);

        assert.deepEqual([...store.streams.keys()], [TEAM_STREAM]);
    });

    test("a team is deleted by its team name", async () => {
        await service.createTeam({ name: TEAM_NAME, coaches: [] }, admin);

        await service.deleteTeam(TEAM_NAME, admin);

        assert.deepEqual(store.eventTypesOf(TEAM_STREAM), ["TeamAdded", "TeamDeleted"]);
    });

    test("teams of other coaches are reported as not found", async () => {
        await createTeamWithStampCard();

        await assert.rejects(service.listPlayers(TEAM_NAME, otherCoach), TeamNotFoundError);
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
        const eventCount = store.eventTypesOf(TEAM_STREAM).length;

        await assert.rejects(service.approveStamps(TEAM_NAME,
            { stampIds: [stampId, "00000000-0000-4000-8000-000000000000"] }, coach));

        assert.equal(store.eventTypesOf(TEAM_STREAM).length, eventCount);
    });

    test("a stamp submitted by an external user is listed as pending", async () => {
        const reference = await createTeamWithStampCard();
        const sharedCode = await service.shareStampCard(reference, coach);
        const stampId = await service.submitStamp(reference, sharedCode.code, { reason: "Lauf", issuerName: "Oma" });

        const pending = await service.listPendingStamps(TEAM_NAME, coach);

        assert.deepEqual(pending.map(entry => entry.stamp.id), [stampId]);
    });

    test("coaches only see the teams they coach", async () => {
        store.projectedTeams = [
            { id: "id-a", streamId: "team-raspo-a", club: "raspo", name: "a", coaches: ["Coach@Raspo.de"] },
            { id: "id-b", streamId: "team-raspo-b", club: "raspo", name: "b", coaches: [otherCoach.email] }
        ];

        const teams = await service.listTeams(coach);

        assert.deepEqual(teams.map(team => team.name), ["a"]);
    });

    test("admins see all teams", async () => {
        store.projectedTeams = [
            { id: "id-a", streamId: "team-raspo-a", club: "raspo", name: "a", coaches: [coach.email] },
            { id: "id-b", streamId: "team-raspo-b", club: "raspo", name: "b", coaches: [otherCoach.email] }
        ];

        const teams = await service.listTeams(admin);

        assert.equal(teams.length, 2);
    });

    test("incomplete stamp cards are collected across all teams", async () => {
        await createTeamWithStampCard();
        store.projectedTeams = [{ id: "team-id", streamId: TEAM_STREAM, club: CLUB_NAME, name: TEAM_NAME, coaches: [coach.email] }];

        const incomplete = await service.listIncompleteStampCards({ accountingYear: 2026, requiredStamps: 1 });

        assert.deepEqual(incomplete.map(card => card.player?.firstName), ["Max"]);
    });
});
