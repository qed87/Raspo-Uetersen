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

    createTeam(team: dtos.CreateTeamDto, user: AuthenticatedUser): Promise<string> {
        return this.changeTeam(teamStreamName(team), user, aggregate => aggregate.create(team, user.email));
    }

    deleteTeam(streamName: string, user: AuthenticatedUser): Promise<void> {
        return this.changeTeam(streamName, user, team => team.delete(user.email));
    }

    addPlayer(teamName: string, player: dtos.PlayerDto, user: AuthenticatedUser): Promise<string> {
        return this.changeClubTeam(teamName, user, team => team.addPlayer(player, user.email));
    }

    updatePlayer(reference: dtos.PlayerReference, player: dtos.PlayerDto, user: AuthenticatedUser): Promise<void> {
        return this.changeClubTeam(reference.teamName, user,
            team => team.updatePlayer(reference.playerId, player, user.email));
    }

    removePlayer(reference: dtos.PlayerReference, user: AuthenticatedUser): Promise<void> {
        return this.changeClubTeam(reference.teamName, user, team => team.removePlayer(reference.playerId, user.email));
    }

    enablePlayer(reference: dtos.PlayerReference, user: AuthenticatedUser): Promise<void> {
        return this.changeClubTeam(reference.teamName, user, team => team.enablePlayer(reference.playerId, user.email));
    }

    disablePlayer(reference: dtos.PlayerReference, user: AuthenticatedUser): Promise<void> {
        return this.changeClubTeam(reference.teamName, user, team => team.disablePlayer(reference.playerId, user.email));
    }

    addStampCard(teamName: string, card: dtos.StampCardDto, user: AuthenticatedUser): Promise<string> {
        return this.changeClubTeam(teamName, user, team => team.addStampCard(card, user.email));
    }

    removeStampCard(reference: dtos.StampCardReference, user: AuthenticatedUser): Promise<void> {
        return this.changeClubTeam(reference.teamName, user,
            team => team.removeStampCard(reference.stampCardId, user.email));
    }

    addStamp(reference: dtos.StampCardReference, stamp: dtos.StampDto, user: AuthenticatedUser): Promise<string> {
        return this.changeClubTeam(reference.teamName, user,
            team => team.addStamp(reference.stampCardId, stamp, user.email));
    }

    removeStamp(reference: dtos.StampReference, user: AuthenticatedUser): Promise<void> {
        return this.changeClubTeam(reference.teamName, user,
            team => team.removeStamp(reference.stampCardId, reference.stampId, user.email));
    }

    shareStampCard(reference: dtos.StampCardReference, user: AuthenticatedUser): Promise<SharedCode> {
        return this.changeClubTeam(reference.teamName, user,
            team => team.shareStampCard(reference.stampCardId, user.email));
    }

    revokeSharedCode(reference: dtos.SharedCodeReference, user: AuthenticatedUser): Promise<void> {
        return this.changeClubTeam(reference.teamName, user,
            team => team.revokeSharedCode(reference.stampCardId, reference.sharedCodeId, user.email));
    }

    removeSharedCode(reference: dtos.SharedCodeReference, user: AuthenticatedUser): Promise<void> {
        return this.changeClubTeam(reference.teamName, user,
            team => team.removeSharedCode(reference.stampCardId, reference.sharedCodeId, user.email));
    }

    approveStamp(reference: dtos.StampReference, user: AuthenticatedUser): Promise<void> {
        return this.changeClubTeam(reference.teamName, user,
            team => team.approveStamp(reference.stampCardId, reference.stampId, user.email));
    }

    approvePendingStamp(reference: dtos.PendingStampReference, user: AuthenticatedUser): Promise<void> {
        return this.changeClubTeam(reference.teamName, user,
            team => team.approvePendingStamp(reference.stampId, user.email));
    }

    approveStamps(teamName: string, approval: dtos.ApproveStampsDto, user: AuthenticatedUser): Promise<void> {
        return this.changeClubTeam(teamName, user, team => team.approveStamps(approval.stampIds, user.email));
    }

    rejectStamp(reference: dtos.StampReference, user: AuthenticatedUser): Promise<void> {
        return this.changeClubTeam(reference.teamName, user,
            team => team.rejectStamp(reference.stampCardId, reference.stampId, user.email));
    }

    /**
     * Submits a stamp of an external user. The share code authorizes the access instead of a user,
     * the stamp only becomes active once a coach approves it.
     */
    submitStamp(reference: dtos.StampCardReference, code: string, stamp: dtos.SubmittedStampDto): Promise<string> {
        return this.applyChange(this.clubTeamStreamName(reference.teamName),
            team => team.submitStamp(reference.stampCardId, code, stamp));
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
        const team = await this.loadTeam(this.clubTeamStreamName(reference.teamName));
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

    private changeClubTeam<Output>(
        teamName: string,
        user: AuthenticatedUser,
        change: (team: TeamAggregate) => Output): Promise<Output> {
        return this.changeTeam(this.clubTeamStreamName(teamName), user, change);
    }

    private async queryClubTeam<Output>(
        teamName: string,
        user: AuthenticatedUser,
        query: (team: TeamAggregate) => Output): Promise<Output> {
        const team = await this.loadTeam(this.clubTeamStreamName(teamName));
        ensureVisible(team, user);
        return query(team);
    }

    private clubTeamStreamName(teamName: string): string {
        return teamStreamName({ club: this.clubName, name: teamName });
    }

    /**
     * Teams the user does not coach are reported as not found, so that their existence is not revealed.
     */
    private changeTeam<Output>(
        streamName: string,
        user: AuthenticatedUser,
        change: (team: TeamAggregate) => Output): Promise<Output> {
        return this.applyChange(streamName, team => {
            ensureVisible(team, user);
            return change(team);
        });
    }

    private async applyChange<Output>(streamName: string, change: (team: TeamAggregate) => Output): Promise<Output> {
        const team = await this.loadTeam(streamName);
        const output = change(team);
        await this.saveTeam(team);
        return output;
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
