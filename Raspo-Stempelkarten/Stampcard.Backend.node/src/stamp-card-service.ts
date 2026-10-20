import type { KurrentDBClient } from "@kurrent/kurrentdb-client";
import type EventEmitter from "node:events";
import { Result } from "neverthrow";
import type { AuthenticatedUser } from "./authenticated-user.js";
import * as dtos from './stamp-card-dtos.js'
import { TeamNotFoundError } from "./stamp-card-errors.js";
import type { IncompleteStampCard, PendingStamp, Player, TeamAggregate, TeamDetails } from "./stamp-card-model.js";
import { TeamRepository } from "./stamp-card-repository.js";
import type { SharedCode, StampCard } from "./stamp-card.js";

function teamStreamName(team: { club: string, name: string }): string {
    return `team-${team.club}-${team.name}`;
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

export class StampCardService {
    private readonly teams: TeamRepository;
    private readonly clubName: string;

    constructor(private readonly eventStoreDb: KurrentDBClient,
        private readonly eventBus: EventEmitter,
        clubName: string) {
        this.teams = new TeamRepository(eventStoreDb);
        this.clubName = dtos.normalizeName(clubName);
    }

    /**
     * Creates the team in the club of the configuration, its stream is named team-{club}-{team name}.
     */
    async createTeam(newTeam: dtos.CreateTeamDto, user: AuthenticatedUser): Promise<string> {
        const team = await this.loadTeam(this.buildStreamName(newTeam.name));
        ensureVisible(team, user);
        const teamId = team.create({ ...newTeam, club: this.clubName }, user.email);
        await this.saveTeam(team);
        return teamId;
    }

    async deleteTeam(teamName: string, user: AuthenticatedUser): Promise<void> {
        const team = await this.loadTeam(this.buildStreamName(teamName));
        ensureVisible(team, user);
        team.delete(user.email);
        await this.saveTeam(team);
    }

    async addPlayer(teamName: string, player: dtos.PlayerDto, user: AuthenticatedUser): Promise<string> {
        const team = await this.loadTeam(this.buildStreamName(teamName));
        ensureVisible(team, user);
        const playerId = team.addPlayer(player, user.email);
        await this.saveTeam(team);
        return playerId;
    }

    async updatePlayer(reference: dtos.PlayerReference, player: dtos.PlayerDto, user: AuthenticatedUser): Promise<void> {
        const team = await this.loadTeam(this.buildStreamName(reference.teamName));
        ensureVisible(team, user);
        team.updatePlayer(reference.playerId, player, user.email);
        await this.saveTeam(team);
    }

    async removePlayer(reference: dtos.PlayerReference, user: AuthenticatedUser): Promise<void> {
        const team = await this.loadTeam(this.buildStreamName(reference.teamName));
        ensureVisible(team, user);
        team.removePlayer(reference.playerId, user.email);
        await this.saveTeam(team);
    }

    async enablePlayer(reference: dtos.PlayerReference, user: AuthenticatedUser): Promise<void> {
        const team = await this.loadTeam(this.buildStreamName(reference.teamName));
        ensureVisible(team, user);
        team.enablePlayer(reference.playerId, user.email);
        await this.saveTeam(team);
    }

    async disablePlayer(reference: dtos.PlayerReference, user: AuthenticatedUser): Promise<void> {
        const team = await this.loadTeam(this.buildStreamName(reference.teamName));
        ensureVisible(team, user);
        team.disablePlayer(reference.playerId, user.email);
        await this.saveTeam(team);
    }

    async addStampCard(teamName: string, card: dtos.StampCardDto, user: AuthenticatedUser): Promise<string> {
        const team = await this.loadTeam(this.buildStreamName(teamName));
        ensureVisible(team, user);
        const stampCardId = team.addStampCard(card, user.email);
        await this.saveTeam(team);
        return stampCardId;
    }

    async removeStampCard(reference: dtos.StampCardReference, user: AuthenticatedUser): Promise<void> {
        const team = await this.loadTeam(this.buildStreamName(reference.teamName));
        ensureVisible(team, user);
        team.removeStampCard(reference.stampCardId, user.email);
        await this.saveTeam(team);
    }

    async addStamp(reference: dtos.StampCardReference, stamp: dtos.StampDto, user: AuthenticatedUser): Promise<string> {
        const team = await this.loadTeam(this.buildStreamName(reference.teamName));
        ensureVisible(team, user);
        const stampId = team.addStamp(reference.stampCardId, stamp, user.email);
        await this.saveTeam(team);
        return stampId;
    }

    async removeStamp(reference: dtos.StampReference, user: AuthenticatedUser): Promise<void> {
        const team = await this.loadTeam(this.buildStreamName(reference.teamName));
        ensureVisible(team, user);
        team.removeStamp(reference.stampCardId, reference.stampId, user.email);
        await this.saveTeam(team);
    }

    async shareStampCard(reference: dtos.StampCardReference, user: AuthenticatedUser): Promise<SharedCode> {
        const team = await this.loadTeam(this.buildStreamName(reference.teamName));
        ensureVisible(team, user);
        const sharedCode = team.shareStampCard(reference.stampCardId, user.email);
        await this.saveTeam(team);
        return sharedCode;
    }

    async revokeSharedCode(reference: dtos.SharedCodeReference, user: AuthenticatedUser): Promise<void> {
        const team = await this.loadTeam(this.buildStreamName(reference.teamName));
        ensureVisible(team, user);
        team.revokeSharedCode(reference.stampCardId, reference.sharedCodeId, user.email);
        await this.saveTeam(team);
    }

    async removeSharedCode(reference: dtos.SharedCodeReference, user: AuthenticatedUser): Promise<void> {
        const team = await this.loadTeam(this.buildStreamName(reference.teamName));
        ensureVisible(team, user);
        team.removeSharedCode(reference.stampCardId, reference.sharedCodeId, user.email);
        await this.saveTeam(team);
    }

    async approveStamp(reference: dtos.StampReference, user: AuthenticatedUser): Promise<void> {
        const team = await this.loadTeam(this.buildStreamName(reference.teamName));
        ensureVisible(team, user);
        team.approveStamp(reference.stampCardId, reference.stampId, user.email);
        await this.saveTeam(team);
    }

    async approvePendingStamp(reference: dtos.PendingStampReference, user: AuthenticatedUser): Promise<void> {
        const team = await this.loadTeam(this.buildStreamName(reference.teamName));
        ensureVisible(team, user);
        team.approvePendingStamp(reference.stampId, user.email);
        await this.saveTeam(team);
    }

    async approveStamps(teamName: string, approval: dtos.ApproveStampsDto, user: AuthenticatedUser): Promise<void> {
        const team = await this.loadTeam(this.buildStreamName(teamName));
        ensureVisible(team, user);
        team.approveStamps(approval.stampIds, user.email);
        await this.saveTeam(team);
    }

    async rejectStamp(reference: dtos.StampReference, user: AuthenticatedUser): Promise<void> {
        const team = await this.loadTeam(this.buildStreamName(reference.teamName));
        ensureVisible(team, user);
        team.rejectStamp(reference.stampCardId, reference.stampId, user.email);
        await this.saveTeam(team);
    }

    /**
     * Submits a stamp of an external user. The share code authorizes the access instead of a user,
     * the stamp only becomes active once a coach approves it.
     */
    async submitStamp(reference: dtos.StampCardReference, code: string, stamp: dtos.SubmittedStampDto): Promise<string> {
        const team = await this.loadTeam(this.buildStreamName(reference.teamName));
        const stampId = team.submitStamp(reference.stampCardId, code, stamp);
        await this.saveTeam(team);
        return stampId;
    }

    getTeam(teamName: string, user: AuthenticatedUser): Promise<TeamDetails> {
        return this.queryClubTeam(teamName, user, team => team.describe());
    }

    listPlayers(teamName: string, user: AuthenticatedUser): Promise<Player[]> {
        return this.queryClubTeam(teamName, user, team => team.listPlayers());
    }

    getPlayer(reference: dtos.PlayerReference, user: AuthenticatedUser): Promise<Player> {
        return this.queryClubTeam(reference.teamName, user, team => team.findPlayer(reference.playerId));
    }

    listStampCards(teamName: string, filter: dtos.StampCardFilter, user: AuthenticatedUser): Promise<StampCard[]> {
        return this.queryClubTeam(teamName, user,
            team => team.listStampCards().filter(card => matchesFilter(card, filter)));
    }

    getStampCard(reference: dtos.StampCardReference, user: AuthenticatedUser): Promise<StampCard> {
        return this.queryClubTeam(reference.teamName, user, team => team.findStampCard(reference.stampCardId));
    }

    listSharedCodes(reference: dtos.StampCardReference, user: AuthenticatedUser): Promise<SharedCode[]> {
        return this.queryClubTeam(reference.teamName, user,
            team => team.findStampCard(reference.stampCardId).listSharedCodes());
    }

    listPendingStamps(teamName: string, user: AuthenticatedUser): Promise<PendingStamp[]> {
        return this.queryClubTeam(teamName, user, team => team.listPendingStamps());
    }

    /**
     * Returns the stamp card an external user may see with the given share code.
     */
    async getSharedStampCard(reference: dtos.StampCardReference, code: string): Promise<StampCard> {
        const team = await this.loadTeam(this.buildStreamName(reference.teamName));
        return team.findSharedStampCard(reference.stampCardId, code);
    }

    async listTeams(user: AuthenticatedUser): Promise<dtos.TeamReadDto[]> {
        const teams = await this.listAllTeams();
        return user.isAdmin ? teams : teams.filter(team => isCoachedBy(team, user));
    }

    /**
     * Evaluates the stamp cards of all teams. Authorizing the accounting role is up to the caller.
     */
    async listIncompleteStampCards(query: dtos.IncompleteStampCardQuery): Promise<IncompleteStampCard[]> {
        const loadedTeams = await Promise.all((await this.listAllTeams()).map(team => this.teams.load(team.id)));
        const teams = Result.combineWithAllErrors(loadedTeams);
        if (teams.isErr()) {
            throw new AggregateError(teams.error, "Stamp cards could not be evaluated, some team streams are invalid.");
        }
        return teams.value.flatMap(team => team.listIncompleteStampCards(query.accountingYear, query.requiredStamps));
    }

    private async listAllTeams(): Promise<dtos.TeamReadDto[]> {
        const projectionState = await this.eventStoreDb
            .getProjectionState<{ teams?: dtos.TeamReadDto[] }>("all-club-teams");
        return projectionState.teams ?? [];
    }

    private async queryClubTeam<Output>(
        teamName: string,
        user: AuthenticatedUser,
        query: (team: TeamAggregate) => Output): Promise<Output> {
        const team = await this.loadTeam(this.buildStreamName(teamName));
        ensureVisible(team, user);
        return query(team);
    }

    private buildStreamName(teamName: string): string {
        return teamStreamName({ club: this.clubName, name: teamName });
    }

    // The routes report failures through exceptions, so the repository results end here.
    private async loadTeam(streamName: string): Promise<TeamAggregate> {
        const loaded = await this.teams.load(streamName);
        if (loaded.isErr()) throw loaded.error;
        return loaded.value;
    }

    private async saveTeam(team: TeamAggregate): Promise<void> {
        const saved = await this.teams.save(team);
        if (saved.isErr()) throw saved.error;
    }
}
