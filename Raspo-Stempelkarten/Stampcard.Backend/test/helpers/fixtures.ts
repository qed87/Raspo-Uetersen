import assert from "node:assert/strict";
import { Result } from "neverthrow";
import type { AuthenticatedUser } from "../../src/authenticated-user.js";
import { TeamAggregate } from "../../src/stamp-card-model.js";

export const TEAM_ID = "44444444444444448444444444444444";
export const STREAM_NAME = `team-${TEAM_ID}`;
export const ISSUER = "coach@raspo.de";

export const coach: AuthenticatedUser = { email: "coach@raspo.de", isAdmin: false, isAccountant: false };
export const otherCoach: AuthenticatedUser = { email: "other@raspo.de", isAdmin: false, isAccountant: false };
export const admin: AuthenticatedUser = { email: "admin@raspo.de", isAdmin: true, isAccountant: false };

export const max = { firstName: "Max", lastName: "Muster", birthdate: "2017-03-04" };

export function emptyTeam(): TeamAggregate {
    return new TeamAggregate(STREAM_NAME, undefined);
}

export function createdTeam(): TeamAggregate {
    const team = emptyTeam();
    team.create(TEAM_ID, { club: "raspo", name: "2017-1", coaches: [coach.email] }, ISSUER);
    return team;
}

/**
 * A team with the player Max and his stamp card for 2026.
 */
export function teamWithStampCard() {
    const team = createdTeam();
    const playerId = team.addPlayer(max, ISSUER);
    const stampCardId = team.addStampCard({ playerId, accountingYear: 2026 }, ISSUER);
    return { team, playerId, stampCardId };
}

/**
 * Rebuilds the team from the events it recorded, like loading it from the stream.
 */
export function replayed(team: TeamAggregate): TeamAggregate {
    const copy = new TeamAggregate(team.streamName, BigInt(team.changes.length - 1));
    expectOk(Result.combine(team.changes.map(event => copy.replay(event))));
    return copy;
}

export function expectOk<T, E>(result: Result<T, E>): T {
    assert.ok(result.isOk(), `expected a successful result, but got: ${result.isErr() ? String(result.error) : ""}`);
    return result.value;
}

export function expectErr<T, E>(result: Result<T, E>): E {
    assert.ok(result.isErr(), "expected a failed result");
    return result.error;
}
