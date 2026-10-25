import { describe, test } from "node:test";
import assert from "node:assert/strict";
import {
    CoachNotFoundError,
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
    expectOk,
    ISSUER,
    max,
    otherCoach,
    replayed,
    TEAM_ID,
    teamWithStampCard
} from "./helpers/fixtures.js";
import { parseTeamEvent } from "../src/stamp-card-events.js";

const guestStamp = { reason: "Lauftraining" };

function sharedTeam() {
    const { team, playerId, stampCardId } = teamWithStampCard();
    const sharedCode = team.shareStampCard(stampCardId, { holderName: "Oma Erna" }, ISSUER);
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

describe("coaches", () => {
    test("an added coach can see the team after replaying", () => {
        const team = createdTeam();

        team.addCoach(otherCoach.email, ISSUER);

        assert.equal(replayed(team).isVisibleTo(otherCoach), true);
    });

    test("adding a coach of the team again records nothing", () => {
        const team = createdTeam();
        const eventCount = team.changes.length;

        team.addCoach(coach.email, ISSUER);

        assert.equal(team.changes.length, eventCount);
    });

    test("a removed coach can no longer see the team after replaying", () => {
        const team = createdTeam();

        team.removeCoach(coach.email, ISSUER);

        assert.deepEqual(replayed(team).describe().coaches, []);
        assert.equal(replayed(team).isVisibleTo(coach), false);
    });

    test("removing a coach who does not coach the team is rejected", () => {
        assert.throws(() => createdTeam().removeCoach(otherCoach.email, ISSUER), CoachNotFoundError);
    });

    test("coaches of a team that does not exist cannot be changed", () => {
        assert.throws(() => emptyTeam().addCoach(coach.email, ISSUER), TeamNotFoundError);
        assert.throws(() => emptyTeam().removeCoach(coach.email, ISSUER), TeamNotFoundError);
    });

    test("replaying the removal of an unknown coach reports the problem", () => {
        const reason = expectErr(createdTeam().replay(
            { type: "CoachRemoved", data: { email: otherCoach.email, issuer: ISSUER, issuedOn: new Date() } }));

        assert.match(reason, /unknown coach/);
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

    test("removing a player removes its stamp cards and their shared codes", () => {
        const { team, playerId, stampCardId, sharedCode } = sharedTeam();

        team.removePlayer(playerId, ISSUER);

        assert.deepEqual(team.listStampCards(), []);
        assert.throws(() => team.findSharedStampCard(stampCardId, sharedCode.code));
    });

    test("removing a player keeps the stamp cards of other players", () => {
        const { team, playerId } = teamWithStampCard();
        const otherPlayerId = team.addPlayer({ ...max, firstName: "Moritz" }, ISSUER);
        const otherStampCardId = team.addStampCard({ playerId: otherPlayerId, accountingYear: 2026 }, ISSUER);

        team.removePlayer(playerId, ISSUER);

        assert.deepEqual(team.listStampCards().map(card => card.id), [otherStampCardId]);
    });

    test("removing a player records its stamp card removals before the player removal", () => {
        const { team, playerId } = teamWithStampCard();
        const eventCount = team.changes.length;

        team.removePlayer(playerId, ISSUER);

        assert.deepEqual(team.changes.slice(eventCount).map(event => event.type), ["StampCardRemoved", "PlayerRemoved"]);
    });

    test("removed stamp cards of a removed player are not restored when replaying", () => {
        const { team, playerId } = teamWithStampCard();
        team.removePlayer(playerId, ISSUER);

        assert.deepEqual(replayed(team).listStampCards(), []);
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

    test("the holder of the share code is recorded as issuer", () => {
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

        assert.equal(team.findSharedStampCard(stampCardId, sharedCode.code).stampCard.id, stampCardId);
    });

    test("the shared stamp card tells the holder of the share code", () => {
        const { team, stampCardId, sharedCode } = sharedTeam();

        assert.equal(team.findSharedStampCard(stampCardId, sharedCode.code).sharedCode.holderName, "Oma Erna");
    });

    test("codes shared before they were issued to a holder are replayed with an unknown holder", () => {
        const { team, stampCardId } = teamWithStampCard();
        const sharedCodeAdded = expectOk(parseTeamEvent("SharedCodeAdded", {
            id: "00000000-0000-4000-8000-000000000001", stampCardId, code: "legacy", issuer: ISSUER, issuedOn: new Date()
        }));

        team.replay(sharedCodeAdded);

        assert.equal(team.findSharedStampCard(stampCardId, "legacy").sharedCode.holderName, "Unbekannt");
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

        assert.deepEqual(incomplete.map(card => [card.stampCard?.id, card.acceptedStamps]), [[stampCardId, 1]]);
    });

    test("pending stamps do not count as accepted", () => {
        const { team, stampCardId, sharedCode } = sharedTeam();
        team.addStamp(stampCardId, { reason: "Training" }, ISSUER);
        team.submitStamp(stampCardId, sharedCode.code, guestStamp);

        const incomplete = team.listIncompleteStampCards(2026, 2);

        assert.equal(incomplete.length, 1);
    });

    test("stamp cards with enough accepted stamps are not listed", () => {
        const { team, stampCardId } = teamWithStampCard();
        team.addStamp(stampCardId, { reason: "Training" }, ISSUER);

        assert.deepEqual(team.listIncompleteStampCards(2026, 1), []);
    });

    test("active players without a stamp card for the accounting year are listed without stamp card", () => {
        const { team, playerId } = teamWithStampCard();

        const incomplete = team.listIncompleteStampCards(2025, 1);

        assert.deepEqual(
            incomplete.map(card => [card.playerId, card.stampCard, card.acceptedStamps]),
            [[playerId, undefined, 0]]);
    });

    test("disabled players without a stamp card are not listed", () => {
        const { team, playerId } = teamWithStampCard();
        team.disablePlayer(playerId, ISSUER);

        assert.deepEqual(team.listIncompleteStampCards(2025, 1), []);
    });

    test("stamp cards of players removed by older streams are listed without player", () => {
        const { team, playerId } = teamWithStampCard();
        // older streams removed the player without its stamp cards
        team.replay({ type: "PlayerRemoved", data: { id: playerId, issuer: ISSUER, issuedOn: new Date() } });

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
