import { describe, test } from "node:test";
import assert from "node:assert/strict";
import {
    InvalidShareCodeError,
    PlayerAlreadyExistsError,
    PlayerNotFoundError,
    SharedCodeRevokedError,
    StampCardAlreadyExistsError,
    StampNotFoundError,
    StampNotPendingError,
    TeamAlreadyExistsError,
    TeamNotFoundError
} from "../src/stamp-card-errors.js";
import {
    admin,
    coach,
    createdTeam,
    emptyTeam,
    expectErr,
    ISSUER,
    max,
    otherCoach,
    replayed,
    TEAM_ID,
    teamWithStampCard
} from "./helpers/fixtures.js";

const guestStamp = { reason: "Lauftraining", issuerName: "Oma Erna" };

function sharedTeam() {
    const { team, playerId, stampCardId } = teamWithStampCard();
    const sharedCode = team.shareStampCard(stampCardId, ISSUER);
    return { team, playerId, stampCardId, sharedCode };
}

describe("team lifecycle", () => {
    test("creating a team records TeamAdded and a CoachAdded per coach", () => {
        const team = createdTeam();

        assert.deepEqual(team.changes.map(event => event.type), ["TeamAdded", "CoachAdded"]);
    });

    test("creating an existing team is rejected", () => {
        const team = createdTeam();

        assert.throws(() => team.create(TEAM_ID, { club: "raspo", name: "2017-1", coaches: [] }, ISSUER), TeamAlreadyExistsError);
    });

    test("a deleted team can be created again without its former players", () => {
        const team = createdTeam();
        team.addPlayer(max, ISSUER);
        team.delete(ISSUER);

        team.create(TEAM_ID, { club: "raspo", name: "2017-1", coaches: [] }, ISSUER);

        assert.deepEqual(team.listPlayers(), []);
    });

    test("renaming a team changes its name after replaying", () => {
        const team = createdTeam();

        team.rename("2017-2", ISSUER);

        assert.equal(replayed(team).describe().name, "2017-2");
    });

    test("renaming a team to its current name records nothing", () => {
        const team = createdTeam();
        const eventCount = team.changes.length;

        team.rename("2017-1", ISSUER);

        assert.equal(team.changes.length, eventCount);
    });

    test("renaming a team that does not exist is rejected", () => {
        assert.throws(() => emptyTeam().rename("2017-2", ISSUER), TeamNotFoundError);
    });

    test("changing a team that does not exist is rejected", () => {
        const team = emptyTeam();

        assert.throws(() => team.addPlayer(max, ISSUER), TeamNotFoundError);
    });
});

describe("team visibility", () => {
    test("a coach of the team can see it", () => {
        assert.equal(createdTeam().isVisibleTo(coach), true);
    });

    test("a coach of another team cannot see it", () => {
        assert.equal(createdTeam().isVisibleTo(otherCoach), false);
    });

    test("an admin can see every team", () => {
        assert.equal(createdTeam().isVisibleTo(admin), true);
    });
});

describe("players", () => {
    test("a player with the same name and birthdate is rejected regardless of case", () => {
        const team = createdTeam();
        team.addPlayer(max, ISSUER);

        assert.throws(() => team.addPlayer({ ...max, firstName: "MAX" }, ISSUER), PlayerAlreadyExistsError);
    });

    test("updating a player to the identity of another player is rejected", () => {
        const team = createdTeam();
        team.addPlayer(max, ISSUER);
        const moritzId = team.addPlayer({ ...max, firstName: "Moritz" }, ISSUER);

        assert.throws(() => team.updatePlayer(moritzId, max, ISSUER), PlayerAlreadyExistsError);
    });

    test("disabling an inactive player records no event", () => {
        const team = createdTeam();
        const playerId = team.addPlayer(max, ISSUER);
        team.disablePlayer(playerId, ISSUER);
        const eventCount = team.changes.length;

        team.disablePlayer(playerId, ISSUER);

        assert.equal(team.changes.length, eventCount);
    });

    test("a removed player cannot be found", () => {
        const team = createdTeam();
        const playerId = team.addPlayer(max, ISSUER);

        team.removePlayer(playerId, ISSUER);

        assert.throws(() => team.findPlayer(playerId), PlayerNotFoundError);
    });
});

describe("stamp cards and stamps", () => {
    test("a second stamp card for the same player and accounting year is rejected", () => {
        const { team, playerId } = teamWithStampCard();

        assert.throws(() => team.addStampCard({ playerId, accountingYear: 2026 }, ISSUER), StampCardAlreadyExistsError);
    });

    test("a stamp card for an unknown player is rejected", () => {
        const team = createdTeam();

        assert.throws(
            () => team.addStampCard({ playerId: "00000000-0000-4000-8000-000000000000", accountingYear: 2026 }, ISSUER),
            PlayerNotFoundError);
    });

    test("a stamp of a coach is accepted immediately", () => {
        const { team, stampCardId } = teamWithStampCard();

        const stampId = team.addStamp(stampCardId, { reason: "Training" }, ISSUER);

        assert.equal(team.findStampCard(stampCardId).findStamp(stampId).status, "accepted");
    });

    test("removing an unknown stamp is rejected", () => {
        const { team, stampCardId } = teamWithStampCard();

        assert.throws(() => team.removeStamp(stampCardId, "00000000-0000-4000-8000-000000000000", ISSUER), StampNotFoundError);
    });
});

describe("approval of submitted stamps", () => {
    test("a stamp submitted with a share code is pending", () => {
        const { team, stampCardId, sharedCode } = sharedTeam();

        const stampId = team.submitStamp(stampCardId, sharedCode.code, guestStamp);

        assert.equal(team.findStampCard(stampCardId).findStamp(stampId).status, "pending");
    });

    test("the name of the external user is recorded as issuer", () => {
        const { team, stampCardId, sharedCode } = sharedTeam();

        const stampId = team.submitStamp(stampCardId, sharedCode.code, guestStamp);

        assert.equal(team.findStampCard(stampCardId).findStamp(stampId).issuer, "Oma Erna");
    });

    test("a stamp submitted with an unknown code is rejected", () => {
        const { team, stampCardId } = sharedTeam();

        assert.throws(() => team.submitStamp(stampCardId, "guessed", guestStamp), InvalidShareCodeError);
    });

    test("approving a pending stamp accepts it", () => {
        const { team, stampCardId, sharedCode } = sharedTeam();
        const stampId = team.submitStamp(stampCardId, sharedCode.code, guestStamp);

        team.approvePendingStamp(stampId, ISSUER);

        assert.equal(team.findStampCard(stampCardId).findStamp(stampId).status, "accepted");
    });

    test("approving an accepted stamp is rejected", () => {
        const { team, stampCardId } = teamWithStampCard();
        const stampId = team.addStamp(stampCardId, { reason: "Training" }, ISSUER);

        assert.throws(() => team.approvePendingStamp(stampId, ISSUER), StampNotPendingError);
    });

    test("rejecting a pending stamp removes it from the stamp card", () => {
        const { team, stampCardId, sharedCode } = sharedTeam();
        const stampId = team.submitStamp(stampCardId, sharedCode.code, guestStamp);

        team.rejectStamp(stampCardId, stampId, ISSUER);

        assert.throws(() => team.findStampCard(stampCardId).findStamp(stampId), StampNotFoundError);
    });

    test("approving several stamps fails if one of them is unknown", () => {
        const { team, stampCardId, sharedCode } = sharedTeam();
        const stampId = team.submitStamp(stampCardId, sharedCode.code, guestStamp);

        assert.throws(
            () => team.approveStamps([stampId, "00000000-0000-4000-8000-000000000000"], ISSUER),
            StampNotFoundError);
    });

    test("pending stamps are listed with their stamp card and player", () => {
        const { team, playerId, stampCardId, sharedCode } = sharedTeam();
        const stampId = team.submitStamp(stampCardId, sharedCode.code, guestStamp);

        const [pending] = team.listPendingStamps();

        assert.deepEqual({ ...pending, stamp: pending?.stamp.id }, { stampCardId, playerId, stamp: stampId });
    });
});

describe("shared codes", () => {
    test("a revoked code cannot be used to submit stamps", () => {
        const { team, stampCardId, sharedCode } = sharedTeam();

        team.revokeSharedCode(stampCardId, sharedCode.id, ISSUER);

        assert.throws(() => team.submitStamp(stampCardId, sharedCode.code, guestStamp), SharedCodeRevokedError);
    });

    test("a revoked code still grants read access to its stamp card", () => {
        const { team, stampCardId, sharedCode } = sharedTeam();

        team.revokeSharedCode(stampCardId, sharedCode.id, ISSUER);

        assert.equal(team.findSharedStampCard(stampCardId, sharedCode.code).id, stampCardId);
    });

    test("a removed code grants no access at all", () => {
        const { team, stampCardId, sharedCode } = sharedTeam();

        team.removeSharedCode(stampCardId, sharedCode.id, ISSUER);

        assert.throws(() => team.findSharedStampCard(stampCardId, sharedCode.code), InvalidShareCodeError);
    });
});

describe("incomplete stamp cards", () => {
    test("stamp cards with fewer accepted stamps than required are listed", () => {
        const { team, stampCardId } = teamWithStampCard();
        team.addStamp(stampCardId, { reason: "Training" }, ISSUER);

        const incomplete = team.listIncompleteStampCards(2026, 2);

        assert.deepEqual(incomplete.map(card => [card.stampCard.id, card.acceptedStamps]), [[stampCardId, 1]]);
    });

    test("pending stamps do not count as accepted", () => {
        const { team, stampCardId, sharedCode } = sharedTeam();
        team.addStamp(stampCardId, { reason: "Training" }, ISSUER);
        team.submitStamp(stampCardId, sharedCode.code, guestStamp);

        const incomplete = team.listIncompleteStampCards(2026, 2);

        assert.equal(incomplete.length, 1);
    });

    test("stamp cards of other accounting years are not listed", () => {
        const { team } = teamWithStampCard();

        assert.deepEqual(team.listIncompleteStampCards(2025, 1), []);
    });

    test("stamp cards of removed players are listed without player", () => {
        const { team, playerId } = teamWithStampCard();
        team.removePlayer(playerId, ISSUER);

        const [incomplete] = team.listIncompleteStampCards(2026, 1);

        assert.equal(incomplete?.player, undefined);
    });
});

describe("replaying recorded events", () => {
    test("rebuilds the same stamp cards, stamps and shared codes", () => {
        const { team, stampCardId, sharedCode } = sharedTeam();
        const stampId = team.submitStamp(stampCardId, sharedCode.code, guestStamp);
        team.revokeSharedCode(stampCardId, sharedCode.id, ISSUER);

        const card = replayed(team).findStampCard(stampCardId);

        assert.deepEqual(
            { status: card.findStamp(stampId).status, revoked: card.findSharedCode(sharedCode.id).revoked },
            { status: "pending", revoked: true });
    });

    test("an event referencing an unknown player cannot be replayed", () => {
        const team = createdTeam();
        const playerRemoved = {
            type: "PlayerRemoved" as const,
            data: { id: "00000000-0000-4000-8000-000000000000", issuer: ISSUER, issuedOn: new Date() }
        };

        const reason = expectErr(team.replay(playerRemoved));

        assert.match(reason, /unknown player/);
    });
});
