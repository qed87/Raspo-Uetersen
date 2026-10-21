import type { KurrentDBClient } from "@kurrent/kurrentdb-client";
import { randomUUID } from "node:crypto";
import { Result } from "neverthrow";
import type { AuthenticatedUser } from "./authenticated-user.js";
import * as dtos from './stamp-card-dtos.js'
import { ConcurrentModificationError, TeamAlreadyExistsError, TeamNotFoundError } from "./stamp-card-errors.js";
import type { IncompleteStampCard, PendingStamp, Player, TeamAggregate, TeamDetails } from "./stamp-card-model.js";
import { TeamRepository } from "./stamp-card-repository.js";
import type { SharedCode, StampCard } from "./stamp-card.js";

/**
 * Data of a team together with the concurrency token of the team stream it was read from or written to.
 * Clients pass the token with updates, so that changes made in the meantime are not overwritten.
 */
export type Versioned<T> = { data: T, concurrencyToken: number };

export type VersionedIncompleteStampCard = IncompleteStampCard & { concurrencyToken: number };

/**
 * The stream is named after the team id only, so that renaming a team does not block its old name.
 */
function teamStreamName(teamId: string): string {
    return `team-${teamId}`;
}

/**
 * The concurrency token is the revision of the team stream, -1 if the stream does not exist yet.
 */
function concurrencyTokenOf(revision: bigint | undefined): number {
    return Number(revision ?? -1n);
}

function isCoachedBy(team: dtos.TeamReadDto, user: AuthenticatedUser): boolean {
    return team.coaches.some(email => email.toLowerCase() === user.email);
}

function matchesFilter(card: StampCard, filter: dtos.StampCardFilter): boolean {
    return (filter.playerId === undefined || card.playerId === filter.playerId)
        && (filter.accountingYear === undefined || card.accountingYear === filter.accountingYear);
}

/**
 * Teams the user does not coach are reported as not found, so that their existence is not revealed.
 */
function ensureVisible(team: TeamAggregate, user: AuthenticatedUser): void {
    if (!team.isVisibleTo(user)) {
        throw new TeamNotFoundError(team.streamName);
    }
}

/**
 * Updates pass the concurrency token of the revision the client has read,
 * any other revision means the team was changed since then.
 */
function ensureUnchanged(team: TeamAggregate, concurrencyToken: number): void {
    if (concurrencyTokenOf(team.revision) !== concurrencyToken) {
        throw new ConcurrentModificationError(team.streamName);
    }
}

export class StampCardService {
    private readonly teams: TeamRepository;
    private readonly clubName: string;

    constructor(private readonly eventStoreDb: KurrentDBClient, clubName: string) {
        this.teams = new TeamRepository(eventStoreDb);
        this.clubName = dtos.normalizeName(clubName);
    }

    /**
     * Creates the team in the club of the configuration, its stream is named team-{team id}.
     * The name must be unique within the club.
     */
    async createTeam(newTeam: dtos.CreateTeamDto, user: AuthenticatedUser): Promise<Versioned<string>> {
        await this.ensureNameIsAvailable(newTeam.name);
        const teamId = randomUUID();
        const team = await this.loadTeam(teamId);
        ensureVisible(team, user);
        team.create(teamId, { ...newTeam, club: this.clubName }, user.email);
        await this.saveTeam(team);
        return { data: teamId, concurrencyToken: concurrencyTokenOf(team.nextRevision) };
    }

    async updateTeam(teamId: string, update: dtos.UpdateTeamDto, user: AuthenticatedUser): Promise<Versioned<string>> {
        await this.ensureNameIsAvailable(update.name, teamId);
        const team = await this.loadTeam(teamId);
        ensureVisible(team, user);
        ensureUnchanged(team, update.concurrencyToken);
        team.rename(update.name, user.email);
        await this.saveTeam(team);
        return { data: teamId, concurrencyToken: concurrencyTokenOf(team.nextRevision) };
    }

    async deleteTeam(teamId: string, user: AuthenticatedUser): Promise<Versioned<string>> {
        const team = await this.loadTeam(teamId);
        ensureVisible(team, user);
        team.delete(user.email);
        await this.saveTeam(team);
        return { data: teamId, concurrencyToken: concurrencyTokenOf(team.nextRevision) };
    }

    async addPlayer(teamId: string, player: dtos.PlayerDto, user: AuthenticatedUser): Promise<Versioned<string>> {
        const team = await this.loadTeam(teamId);
        ensureVisible(team, user);
        const playerId = team.addPlayer(player, user.email);
        await this.saveTeam(team);
        return { data: playerId, concurrencyToken: concurrencyTokenOf(team.nextRevision) };
    }

    async updatePlayer(reference: dtos.PlayerReference, update: dtos.UpdatePlayerDto, user: AuthenticatedUser): Promise<Versioned<string>> {
        const { concurrencyToken, ...player } = update;
        const team = await this.loadTeam(reference.teamId);
        ensureVisible(team, user);
        ensureUnchanged(team, concurrencyToken);
        team.updatePlayer(reference.playerId, player, user.email);
        await this.saveTeam(team);
        return { data: reference.playerId, concurrencyToken: concurrencyTokenOf(team.nextRevision) };
    }

    async removePlayer(reference: dtos.PlayerReference, user: AuthenticatedUser): Promise<Versioned<string>> {
        const team = await this.loadTeam(reference.teamId);
        ensureVisible(team, user);
        team.removePlayer(reference.playerId, user.email);
        await this.saveTeam(team);
        return { data: reference.playerId, concurrencyToken: concurrencyTokenOf(team.nextRevision) };
    }

    async enablePlayer(reference: dtos.PlayerReference, user: AuthenticatedUser): Promise<Versioned<string>> {
        const team = await this.loadTeam(reference.teamId);
        ensureVisible(team, user);
        team.enablePlayer(reference.playerId, user.email);
        await this.saveTeam(team);
        return { data: reference.playerId, concurrencyToken: concurrencyTokenOf(team.nextRevision) };
    }

    async disablePlayer(reference: dtos.PlayerReference, user: AuthenticatedUser): Promise<Versioned<string>> {
        const team = await this.loadTeam(reference.teamId);
        ensureVisible(team, user);
        team.disablePlayer(reference.playerId, user.email);
        await this.saveTeam(team);
        return { data: reference.playerId, concurrencyToken: concurrencyTokenOf(team.nextRevision) };
    }

    async addStampCard(teamId: string, card: dtos.StampCardDto, user: AuthenticatedUser): Promise<Versioned<string>> {
        const team = await this.loadTeam(teamId);
        ensureVisible(team, user);
        const stampCardId = team.addStampCard(card, user.email);
        await this.saveTeam(team);
        return { data: stampCardId, concurrencyToken: concurrencyTokenOf(team.nextRevision) };
    }

    async removeStampCard(reference: dtos.StampCardReference, user: AuthenticatedUser): Promise<Versioned<string>> {
        const team = await this.loadTeam(reference.teamId);
        ensureVisible(team, user);
        team.removeStampCard(reference.stampCardId, user.email);
        await this.saveTeam(team);
        return { data: reference.stampCardId, concurrencyToken: concurrencyTokenOf(team.nextRevision) };
    }

    async addStamp(reference: dtos.StampCardReference, stamp: dtos.StampDto, user: AuthenticatedUser): Promise<Versioned<string>> {
        const team = await this.loadTeam(reference.teamId);
        ensureVisible(team, user);
        const stampId = team.addStamp(reference.stampCardId, stamp, user.email);
        await this.saveTeam(team);
        return { data: stampId, concurrencyToken: concurrencyTokenOf(team.nextRevision) };
    }

    async removeStamp(reference: dtos.StampReference, user: AuthenticatedUser): Promise<Versioned<string>> {
        const team = await this.loadTeam(reference.teamId);
        ensureVisible(team, user);
        team.removeStamp(reference.stampCardId, reference.stampId, user.email);
        await this.saveTeam(team);
        return { data: reference.stampId, concurrencyToken: concurrencyTokenOf(team.nextRevision) };
    }

    async shareStampCard(reference: dtos.StampCardReference, user: AuthenticatedUser): Promise<Versioned<SharedCode>> {
        const team = await this.loadTeam(reference.teamId);
        ensureVisible(team, user);
        const sharedCode = team.shareStampCard(reference.stampCardId, user.email);
        await this.saveTeam(team);
        return { data: sharedCode, concurrencyToken: concurrencyTokenOf(team.nextRevision) };
    }

    async revokeSharedCode(reference: dtos.SharedCodeReference, user: AuthenticatedUser): Promise<Versioned<string>> {
        const team = await this.loadTeam(reference.teamId);
        ensureVisible(team, user);
        team.revokeSharedCode(reference.stampCardId, reference.sharedCodeId, user.email);
        await this.saveTeam(team);
        return { data: reference.sharedCodeId, concurrencyToken: concurrencyTokenOf(team.nextRevision) };
    }

    async removeSharedCode(reference: dtos.SharedCodeReference, user: AuthenticatedUser): Promise<Versioned<string>> {
        const team = await this.loadTeam(reference.teamId);
        ensureVisible(team, user);
        team.removeSharedCode(reference.stampCardId, reference.sharedCodeId, user.email);
        await this.saveTeam(team);
        return { data: reference.sharedCodeId, concurrencyToken: concurrencyTokenOf(team.nextRevision) };
    }

    async approveStamp(reference: dtos.StampReference, user: AuthenticatedUser): Promise<Versioned<string>> {
        const team = await this.loadTeam(reference.teamId);
        ensureVisible(team, user);
        team.approveStamp(reference.stampCardId, reference.stampId, user.email);
        await this.saveTeam(team);
        return { data: reference.stampId, concurrencyToken: concurrencyTokenOf(team.nextRevision) };
    }

    async approvePendingStamp(reference: dtos.PendingStampReference, user: AuthenticatedUser): Promise<Versioned<string>> {
        const team = await this.loadTeam(reference.teamId);
        ensureVisible(team, user);
        team.approvePendingStamp(reference.stampId, user.email);
        await this.saveTeam(team);
        return { data: reference.stampId, concurrencyToken: concurrencyTokenOf(team.nextRevision) };
    }

    async approveStamps(teamId: string, approval: dtos.ApproveStampsDto, user: AuthenticatedUser): Promise<Versioned<string[]>> {
        const team = await this.loadTeam(teamId);
        ensureVisible(team, user);
        team.approveStamps(approval.stampIds, user.email);
        await this.saveTeam(team);
        return { data: approval.stampIds, concurrencyToken: concurrencyTokenOf(team.nextRevision) };
    }

    async rejectStamp(reference: dtos.StampReference, user: AuthenticatedUser): Promise<Versioned<string>> {
        const team = await this.loadTeam(reference.teamId);
        ensureVisible(team, user);
        team.rejectStamp(reference.stampCardId, reference.stampId, user.email);
        await this.saveTeam(team);
        return { data: reference.stampId, concurrencyToken: concurrencyTokenOf(team.nextRevision) };
    }

    /**
     * Submits a stamp of an external user. The share code authorizes the access instead of a user,
     * the stamp only becomes active once a coach approves it.
     */
    async submitStamp(reference: dtos.StampCardReference, code: string, stamp: dtos.SubmittedStampDto): Promise<string> {
        const team = await this.loadTeam(reference.teamId);
        const stampId = team.submitStamp(reference.stampCardId, code, stamp);
        await this.saveTeam(team);
        return stampId;
    }

    async getTeam(teamId: string, user: AuthenticatedUser): Promise<Versioned<TeamDetails>> {
        const team = await this.loadTeam(teamId);
        ensureVisible(team, user);
        return { data: team.describe(), concurrencyToken: concurrencyTokenOf(team.revision) };
    }

    async listPlayers(teamId: string, user: AuthenticatedUser): Promise<Versioned<Player[]>> {
        const team = await this.loadTeam(teamId);
        ensureVisible(team, user);
        return { data: team.listPlayers(), concurrencyToken: concurrencyTokenOf(team.revision) };
    }

    async getPlayer(reference: dtos.PlayerReference, user: AuthenticatedUser): Promise<Versioned<Player>> {
        const team = await this.loadTeam(reference.teamId);
        ensureVisible(team, user);
        return { data: team.findPlayer(reference.playerId), concurrencyToken: concurrencyTokenOf(team.revision) };
    }

    async listStampCards(teamId: string, filter: dtos.StampCardFilter, user: AuthenticatedUser): Promise<Versioned<StampCard[]>> {
        const team = await this.loadTeam(teamId);
        ensureVisible(team, user);
        const stampCards = team.listStampCards().filter(card => matchesFilter(card, filter));
        return { data: stampCards, concurrencyToken: concurrencyTokenOf(team.revision) };
    }

    async getStampCard(reference: dtos.StampCardReference, user: AuthenticatedUser): Promise<Versioned<StampCard>> {
        const team = await this.loadTeam(reference.teamId);
        ensureVisible(team, user);
        return { data: team.findStampCard(reference.stampCardId), concurrencyToken: concurrencyTokenOf(team.revision) };
    }

    async listSharedCodes(reference: dtos.StampCardReference, user: AuthenticatedUser): Promise<Versioned<SharedCode[]>> {
        const team = await this.loadTeam(reference.teamId);
        ensureVisible(team, user);
        const sharedCodes = team.findStampCard(reference.stampCardId).listSharedCodes();
        return { data: sharedCodes, concurrencyToken: concurrencyTokenOf(team.revision) };
    }

    async listPendingStamps(teamId: string, user: AuthenticatedUser): Promise<Versioned<PendingStamp[]>> {
        const team = await this.loadTeam(teamId);
        ensureVisible(team, user);
        return { data: team.listPendingStamps(), concurrencyToken: concurrencyTokenOf(team.revision) };
    }

    /**
     * Returns the stamp card an external user may see with the given share code.
     * External users do not change the team, so they get no concurrency token.
     */
    async getSharedStampCard(reference: dtos.StampCardReference, code: string): Promise<StampCard> {
        const team = await this.loadTeam(reference.teamId);
        return team.findSharedStampCard(reference.stampCardId, code);
    }

    /**
     * The teams and their concurrency tokens are read from the projection. If it lags behind,
     * data and token are outdated alike, so an update with that token is rejected instead of overwriting newer changes.
     */
    async listTeams(user: AuthenticatedUser): Promise<dtos.TeamReadDto[]> {
        const teams = await this.listAllTeams();
        return user.isAdmin ? teams : teams.filter(team => isCoachedBy(team, user));
    }

    /**
     * Evaluates the stamp cards of all teams. Authorizing the accounting role is up to the caller.
     */
    async listIncompleteStampCards(query: dtos.IncompleteStampCardQuery): Promise<VersionedIncompleteStampCard[]> {
        const loadedTeams = await Promise.all((await this.listAllTeams()).map(team => this.teams.load(team.streamId)));
        const teams = Result.combineWithAllErrors(loadedTeams);
        if (teams.isErr()) {
            throw new AggregateError(teams.error, "Stamp cards could not be evaluated, some team streams are invalid.");
        }
        return teams.value.flatMap(team => team.listIncompleteStampCards(query.accountingYear, query.requiredStamps)
            .map(card => ({ ...card, concurrencyToken: concurrencyTokenOf(team.revision) })));
    }

    /**
     * Team names are unique within the club, they are checked against the projection of all teams.
     */
    private async ensureNameIsAvailable(name: string, ignoredTeamId?: string): Promise<void> {
        const nameIsTaken = (await this.listAllTeams())
            .some(existing => existing.id !== ignoredTeamId && existing.club === this.clubName && existing.name === name);
        if (nameIsTaken) {
            throw new TeamAlreadyExistsError(this.clubName, name);
        }
    }

    private async listAllTeams(): Promise<dtos.TeamReadDto[]> {
        const projectionState = await this.eventStoreDb
            .getProjectionState<{ teams?: dtos.TeamReadDto[] }>("all-club-teams");
        return projectionState.teams ?? [];
    }

    // The routes report failures through exceptions, so the repository results end here.
    private async loadTeam(teamId: string): Promise<TeamAggregate> {
        const loaded = await this.teams.load(teamStreamName(teamId));
        if (loaded.isErr()) throw loaded.error;
        return loaded.value;
    }

    private async saveTeam(team: TeamAggregate): Promise<void> {
        const saved = await this.teams.save(team);
        if (saved.isErr()) throw saved.error;
    }
}
