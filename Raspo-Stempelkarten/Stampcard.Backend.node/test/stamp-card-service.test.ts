import { beforeEach, describe, test } from "node:test";
import assert from "node:assert/strict";
import { ConcurrentModificationError, TeamAlreadyExistsError, TeamNotFoundError } from "../src/stamp-card-errors.js";
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

async function createTeam(coaches: string[] = []): Promise<string> {
    return (await service.createTeam({ name: TEAM_NAME, coaches }, admin)).data;
}

async function createTeamWithStampCard() {
    const teamId = await createTeam([coach.email]);
    const { data: playerId } = await service.addPlayer(teamId, max, coach);
    const { data: stampCardId } = await service.addStampCard(teamId, { playerId, accountingYear: 2026 }, coach);
    return { teamId, playerId, stampCardId };
}

beforeEach(() => {
    store = new InMemoryEventStore();
    service = new StampCardService(store.asClient(), CLUB_NAME);
});

describe("StampCardService", () => {
    test("a team is stored in the stream team-{team id}", async () => {
        const teamId = await createTeam();

        assert.match(teamId, /^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/);
        assert.deepEqual([...store.streams.keys()], [teamStream(teamId)]);
    });

    test("a team belongs to the configured club", async () => {
        const teamId = await createTeam();

        const { data: team } = await service.getTeam(teamId, admin);

        assert.equal(team.club, CLUB_NAME);
    });

    test("the configured club name is normalized", async () => {
        const otherService = new StampCardService(store.asClient(), "Rasensport Uetersen 1926");

        const { data: teamId } = await otherService.createTeam({ name: TEAM_NAME, coaches: [] }, admin);

        assert.equal((await otherService.getTeam(teamId, admin)).data.club, CLUB_NAME);
    });

    test("a team name that is taken in the club is rejected", async () => {
        store.projectedTeams = [{ id: "id-a", streamId: "team-id-a", club: CLUB_NAME, name: TEAM_NAME, coaches: [], concurrencyToken: 0 }];

        await assert.rejects(service.createTeam({ name: TEAM_NAME, coaches: [] }, admin), TeamAlreadyExistsError);
        assert.equal(store.streams.size, 0);
    });

    test("a team name that is taken in another club is accepted", async () => {
        store.projectedTeams = [{ id: "id-a", streamId: "team-id-a", club: "other-club", name: TEAM_NAME, coaches: [], concurrencyToken: 0 }];

        await createTeam();

        assert.equal(store.streams.size, 1);
    });

    test("a team is deleted by its team id", async () => {
        const teamId = await createTeam();

        await service.deleteTeam(teamId, admin);

        assert.deepEqual(store.eventTypesOf(teamStream(teamId)), ["TeamAdded", "TeamDeleted"]);
    });

    test("teams of other coaches are reported as not found", async () => {
        const { teamId } = await createTeamWithStampCard();

        await assert.rejects(service.listPlayers(teamId, otherCoach), TeamNotFoundError);
    });

    test("the email of the user is recorded as issuer", async () => {
        const { teamId, stampCardId } = await createTeamWithStampCard();
        const reference = { teamId, stampCardId };

        const { data: stampId } = await service.addStamp(reference, { reason: "Training" }, coach);

        const { data: card } = await service.getStampCard(reference, coach);
        assert.equal(card.findStamp(stampId).issuer, coach.email);
    });

    test("approving several stamps saves nothing if one of them is unknown", async () => {
        const { teamId, stampCardId } = await createTeamWithStampCard();
        const reference = { teamId, stampCardId };
        const { data: sharedCode } = await service.shareStampCard(reference, coach);
        const stampId = await service.submitStamp(reference, sharedCode.code, { reason: "Lauf", issuerName: "Oma" });
        const eventCount = store.eventTypesOf(teamStream(teamId)).length;

        await assert.rejects(service.approveStamps(teamId,
            { stampIds: [stampId, "00000000-0000-4000-8000-000000000000"] }, coach));

        assert.equal(store.eventTypesOf(teamStream(teamId)).length, eventCount);
    });

    test("a stamp submitted by an external user is listed as pending", async () => {
        const { teamId, stampCardId } = await createTeamWithStampCard();
        const reference = { teamId, stampCardId };
        const { data: sharedCode } = await service.shareStampCard(reference, coach);
        const stampId = await service.submitStamp(reference, sharedCode.code, { reason: "Lauf", issuerName: "Oma" });

        const { data: pending } = await service.listPendingStamps(teamId, coach);

        assert.deepEqual(pending.map(entry => entry.stamp.id), [stampId]);
    });

    test("coaches only see the teams they coach", async () => {
        store.projectedTeams = [
            { id: "id-a", streamId: "team-id-a", club: "raspo", name: "a", coaches: ["Coach@Raspo.de"], concurrencyToken: 0 },
            { id: "id-b", streamId: "team-id-b", club: "raspo", name: "b", coaches: [otherCoach.email], concurrencyToken: 0 }
        ];

        const teams = await service.listTeams(coach);

        assert.deepEqual(teams.map(team => team.name), ["a"]);
    });

    test("admins see all teams", async () => {
        store.projectedTeams = [
            { id: "id-a", streamId: "team-id-a", club: "raspo", name: "a", coaches: [coach.email], concurrencyToken: 0 },
            { id: "id-b", streamId: "team-id-b", club: "raspo", name: "b", coaches: [otherCoach.email], concurrencyToken: 0 }
        ];

        const teams = await service.listTeams(admin);

        assert.equal(teams.length, 2);
    });

    test("incomplete stamp cards are collected across all teams", async () => {
        const { teamId } = await createTeamWithStampCard();
        store.projectedTeams = [{ id: teamId, streamId: teamStream(teamId), club: CLUB_NAME, name: TEAM_NAME, coaches: [coach.email], concurrencyToken: 0 }];

        const incomplete = await service.listIncompleteStampCards({ accountingYear: 2026, requiredStamps: 1 });

        assert.deepEqual(incomplete.map(card => card.player?.firstName), ["Max"]);
    });
});

describe("concurrency tokens", () => {
    test("the token is the revision of the team stream", async () => {
        const { teamId } = await createTeamWithStampCard();

        const { concurrencyToken } = await service.listPlayers(teamId, coach);

        assert.equal(concurrencyToken, store.eventTypesOf(teamStream(teamId)).length - 1);
    });

    test("every change returns the token of the saved revision", async () => {
        const { teamId } = await createTeamWithStampCard();

        const added = await service.addPlayer(teamId, { ...max, firstName: "Moritz" }, coach);

        assert.equal(added.concurrencyToken, (await service.getTeam(teamId, coach)).concurrencyToken);
    });

    test("a change without effect keeps the token", async () => {
        const { teamId, playerId } = await createTeamWithStampCard();
        const { concurrencyToken } = await service.getTeam(teamId, coach);

        const enabled = await service.enablePlayer({ teamId, playerId }, coach);

        assert.equal(enabled.concurrencyToken, concurrencyToken);
    });

    test("listed teams carry the token of the projection, even if their stream is newer", async () => {
        const teamId = await createTeam([coach.email]);
        await service.addPlayer(teamId, max, coach);
        store.projectedTeams = [{ id: teamId, streamId: teamStream(teamId), club: CLUB_NAME, name: TEAM_NAME, coaches: [coach.email], concurrencyToken: 1 }];

        const [team] = await service.listTeams(coach);

        assert.equal(team?.concurrencyToken, 1);
    });

    test("an update with the token of a lagging projection is rejected", async () => {
        const teamId = await createTeam([coach.email]);
        await service.addPlayer(teamId, max, coach);
        store.projectedTeams = [{ id: teamId, streamId: teamStream(teamId), club: CLUB_NAME, name: TEAM_NAME, coaches: [coach.email], concurrencyToken: 1 }];
        const [listed] = await service.listTeams(admin);

        await assert.rejects(service.updateTeam(teamId, { name: "2017-2", concurrencyToken: listed!.concurrencyToken }, admin),
            ConcurrentModificationError);
    });

    test("incomplete stamp cards carry the token of their team", async () => {
        const { teamId } = await createTeamWithStampCard();
        store.projectedTeams = [{ id: teamId, streamId: teamStream(teamId), club: CLUB_NAME, name: TEAM_NAME, coaches: [coach.email], concurrencyToken: 0 }];

        const [incomplete] = await service.listIncompleteStampCards({ accountingYear: 2026, requiredStamps: 1 });

        assert.equal(incomplete?.concurrencyToken, (await service.getTeam(teamId, coach)).concurrencyToken);
    });
});

describe("updating a team", () => {
    test("a renamed team keeps its stream and returns the new token", async () => {
        const teamId = await createTeam();
        const { concurrencyToken } = await service.getTeam(teamId, admin);

        const updated = await service.updateTeam(teamId, { name: "2017-2", concurrencyToken }, admin);

        const renamed = await service.getTeam(teamId, admin);
        assert.equal(renamed.data.name, "2017-2");
        assert.equal(renamed.concurrencyToken, updated.concurrencyToken);
        assert.deepEqual([...store.streams.keys()], [teamStream(teamId)]);
    });

    test("an update with an outdated token is rejected", async () => {
        const teamId = await createTeam();
        const { concurrencyToken } = await service.getTeam(teamId, admin);
        await service.updateTeam(teamId, { name: "2017-2", concurrencyToken }, admin);

        await assert.rejects(service.updateTeam(teamId, { name: "2017-3", concurrencyToken }, admin),
            ConcurrentModificationError);
    });

    test("an update after any other change of the team is rejected", async () => {
        const teamId = await createTeam([coach.email]);
        const { concurrencyToken } = await service.getTeam(teamId, admin);
        await service.addPlayer(teamId, max, coach);

        await assert.rejects(service.updateTeam(teamId, { name: "2017-2", concurrencyToken }, admin),
            ConcurrentModificationError);
    });

    test("renaming a team to a name taken by another team of the club is rejected", async () => {
        const teamId = await createTeam();
        const { concurrencyToken } = await service.getTeam(teamId, admin);
        store.projectedTeams = [{ id: "id-a", streamId: "team-id-a", club: CLUB_NAME, name: "2017-2", coaches: [], concurrencyToken: 0 }];

        await assert.rejects(service.updateTeam(teamId, { name: "2017-2", concurrencyToken }, admin),
            TeamAlreadyExistsError);
    });

    test("a team keeps its own name when the update does not rename it", async () => {
        const teamId = await createTeam();
        const { concurrencyToken } = await service.getTeam(teamId, admin);
        store.projectedTeams = [{ id: teamId, streamId: teamStream(teamId), club: CLUB_NAME, name: TEAM_NAME, coaches: [], concurrencyToken: 0 }];

        const updated = await service.updateTeam(teamId, { name: TEAM_NAME, concurrencyToken }, admin);

        assert.equal(updated.concurrencyToken, concurrencyToken);
    });
});

describe("updating a player", () => {
    test("a player is updated with the current token", async () => {
        const { teamId, playerId } = await createTeamWithStampCard();
        const { concurrencyToken } = await service.getPlayer({ teamId, playerId }, coach);

        await service.updatePlayer({ teamId, playerId }, { ...max, lastName: "Mustermann", concurrencyToken }, coach);

        assert.equal((await service.getPlayer({ teamId, playerId }, coach)).data.lastName, "Mustermann");
    });

    test("an update with an outdated token is rejected and saves nothing", async () => {
        const { teamId, playerId } = await createTeamWithStampCard();
        const { concurrencyToken } = await service.getPlayer({ teamId, playerId }, coach);
        await service.addStamp({ teamId, stampCardId: (await service.listStampCards(teamId, {}, coach)).data[0]!.id },
            { reason: "Training" }, coach);
        const eventCount = store.eventTypesOf(teamStream(teamId)).length;

        await assert.rejects(service.updatePlayer({ teamId, playerId }, { ...max, lastName: "Mustermann", concurrencyToken }, coach),
            ConcurrentModificationError);
        assert.equal(store.eventTypesOf(teamStream(teamId)).length, eventCount);
    });
});
