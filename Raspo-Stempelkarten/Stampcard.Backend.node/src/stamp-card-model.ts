import { randomBytes, randomUUID } from "node:crypto";
import { err, ok, type Result } from "neverthrow";
import type { AuthenticatedUser } from "./authenticated-user.js";
import type { CreateTeamDto, PlayerDto, StampCardDto, StampDto, SubmittedStampDto } from "./stamp-card-dtos.js";
import type { TeamEvent } from "./stamp-card-events.js";
import {
    InconsistentStreamError,
    PlayerAlreadyExistsError,
    PlayerNotFoundError,
    StampCardAlreadyExistsError,
    StampCardNotFoundError,
    StampNotFoundError,
    TeamAlreadyExistsError,
    TeamNotFoundError
} from "./stamp-card-errors.js";
import { StampCard, type SharedCode, type Stamp } from "./stamp-card.js";

export type Player = PlayerDto & { id: string, active: boolean };

export type NewTeam = CreateTeamDto & { club: string };

export type TeamDetails = { id: string, club: string, name: string, coaches: string[] };

export type PendingStamp = { stampCardId: string, playerId: string, stamp: Stamp };

export type IncompleteStampCard = {
    team: TeamDetails,
    stampCard: StampCard,
    player: Player | undefined,
    acceptedStamps: number
};

const SHARE_CODE_BYTES = 24;

function referenceExists(exists: boolean, event: TeamEvent & { data: { id: string } }): Result<void, string> {
    return exists ? ok(undefined) : err(`${event.type} references unknown '${event.data.id}'.`);
}

function isSamePlayer(player: PlayerDto, other: PlayerDto): boolean {
    return player.firstName.toLowerCase() === other.firstName.toLowerCase()
        && player.lastName.toLowerCase() === other.lastName.toLowerCase()
        && player.birthdate === other.birthdate;
}

/**
 * Current state of a team, rebuilt from its event stream by replaying the stored events.
 * Every change is validated, applied to the state and recorded as a pending event,
 * so that the repository can append exactly these events to the stream.
 */
export class TeamAggregate {
    private readonly pendingEvents: TeamEvent[] = [];
    private readonly players = new Map<string, Player>();
    private readonly coaches = new Set<string>();
    private readonly stampCards = new Map<string, StampCard>();
    private exists = false;
    private details: Omit<TeamDetails, "coaches"> = { id: "", club: "", name: "" };

    constructor(readonly streamName: string, readonly revision: bigint | undefined) {
    }

    get changes(): readonly TeamEvent[] {
        return this.pendingEvents;
    }

    /**
     * Applies an event loaded from the stream. Returns the reason if it does not fit the current state,
     * e.g. because it references a player that does not exist.
     */
    replay(event: TeamEvent): Result<void, string> {
        return this.apply(event);
    }

    isVisibleTo(user: AuthenticatedUser): boolean {
        return user.isAdmin || this.coaches.has(user.email);
    }

    describe(): TeamDetails {
        this.ensureExists();
        return { ...this.details, coaches: [...this.coaches] };
    }

    listPlayers(): Player[] {
        this.ensureExists();
        return [...this.players.values()];
    }

    listStampCards(): StampCard[] {
        this.ensureExists();
        return [...this.stampCards.values()];
    }

    listPendingStamps(): PendingStamp[] {
        return this.listStampCards().flatMap(card => card.listPendingStamps()
            .map(stamp => ({ stampCardId: card.id, playerId: card.playerId, stamp })));
    }

    /**
     * Stamp cards of the accounting year with fewer accepted stamps than required.
     * Cards of removed players are included, their player is unknown.
     */
    listIncompleteStampCards(accountingYear: number, requiredStamps: number): IncompleteStampCard[] {
        if (!this.exists) return [];
        const team = this.describe();
        return this.listStampCards()
            .filter(card => card.accountingYear === accountingYear && card.countAcceptedStamps() < requiredStamps)
            .map(card => ({
                team,
                stampCard: card,
                player: this.players.get(card.playerId),
                acceptedStamps: card.countAcceptedStamps()
            }));
    }

    findPlayer(playerId: string): Player {
        this.ensureExists();
        const player = this.players.get(playerId);
        if (player === undefined) {
            throw new PlayerNotFoundError(playerId);
        }
        return player;
    }

    findStampCard(stampCardId: string): StampCard {
        this.ensureExists();
        const stampCard = this.stampCards.get(stampCardId);
        if (stampCard === undefined) {
            throw new StampCardNotFoundError(stampCardId);
        }
        return stampCard;
    }

    /**
     * Returns the stamp card an external user may access with the given share code.
     */
    findSharedStampCard(stampCardId: string, code: string): StampCard {
        const stampCard = this.findStampCard(stampCardId);
        stampCard.findSharedCodeByCode(code);
        return stampCard;
    }

    create(team: NewTeam, issuer: string): string {
        if (this.exists) {
            throw new TeamAlreadyExistsError(team.club, team.name);
        }
        const id = randomUUID();
        const issuedOn = new Date();
        this.record({ type: "TeamAdded", data: { id, club: team.club, name: team.name, issuer, issuedOn } });
        team.coaches.forEach(email => this.record({ type: "CoachAdded", data: { email, issuer, issuedOn } }));
        return id;
    }

    delete(issuer: string): void {
        this.ensureExists();
        this.record({ type: "TeamDeleted", data: { issuer, issuedOn: new Date() } });
    }

    addPlayer(player: PlayerDto, issuer: string): string {
        this.ensureExists();
        this.ensureUniquePlayer(player);
        const id = randomUUID();
        this.record({ type: "PlayerAdded", data: { id, ...player, issuer, issuedOn: new Date() } });
        return id;
    }

    updatePlayer(playerId: string, player: PlayerDto, issuer: string): void {
        this.findPlayer(playerId);
        this.ensureUniquePlayer(player, playerId);
        this.record({ type: "PlayerUpdated", data: { id: playerId, ...player, issuer, issuedOn: new Date() } });
    }

    removePlayer(playerId: string, issuer: string): void {
        this.findPlayer(playerId);
        this.record({ type: "PlayerRemoved", data: { id: playerId, issuer, issuedOn: new Date() } });
    }

    enablePlayer(playerId: string, issuer: string): void {
        if (!this.findPlayer(playerId).active) {
            this.record({ type: "PlayerEnabled", data: { id: playerId, issuer, issuedOn: new Date() } });
        }
    }

    disablePlayer(playerId: string, issuer: string): void {
        if (this.findPlayer(playerId).active) {
            this.record({ type: "PlayerDisabled", data: { id: playerId, issuer, issuedOn: new Date() } });
        }
    }

    addStampCard(card: StampCardDto, issuer: string): string {
        this.findPlayer(card.playerId);
        if ([...this.stampCards.values()].some(existing => existing.isFor(card))) {
            throw new StampCardAlreadyExistsError(card);
        }
        const id = randomUUID();
        this.record({ type: "StampCardAdded", data: { id, ...card, issuer, issuedOn: new Date() } });
        return id;
    }

    removeStampCard(stampCardId: string, issuer: string): void {
        this.findStampCard(stampCardId);
        this.record({ type: "StampCardRemoved", data: { id: stampCardId, issuer, issuedOn: new Date() } });
    }

    addStamp(stampCardId: string, stamp: StampDto, issuer: string): string {
        this.findStampCard(stampCardId);
        const id = randomUUID();
        this.record({ type: "StampAdded", data: { id, stampCardId, ...stamp, issuer, issuedOn: new Date() } });
        return id;
    }

    removeStamp(stampCardId: string, stampId: string, issuer: string): void {
        this.findStampCard(stampCardId).findStamp(stampId);
        this.record({ type: "StampRemoved", data: { id: stampId, stampCardId, issuer, issuedOn: new Date() } });
    }

    shareStampCard(stampCardId: string, issuer: string): SharedCode {
        this.findStampCard(stampCardId);
        const id = randomUUID();
        const code = randomBytes(SHARE_CODE_BYTES).toString("base64url");
        this.record({ type: "SharedCodeAdded", data: { id, code, stampCardId, issuer, issuedOn: new Date() } });
        return this.findStampCard(stampCardId).findSharedCode(id);
    }

    revokeSharedCode(stampCardId: string, sharedCodeId: string, issuer: string): void {
        if (!this.findStampCard(stampCardId).findSharedCode(sharedCodeId).revoked) {
            this.record({ type: "SharedCodeRevoked", data: { id: sharedCodeId, stampCardId, issuer, issuedOn: new Date() } });
        }
    }

    removeSharedCode(stampCardId: string, sharedCodeId: string, issuer: string): void {
        this.findStampCard(stampCardId).findSharedCode(sharedCodeId);
        this.record({ type: "SharedCodeRemoved", data: { id: sharedCodeId, stampCardId, issuer, issuedOn: new Date() } });
    }

    submitStamp(stampCardId: string, code: string, stamp: SubmittedStampDto): string {
        const sharedCode = this.findStampCard(stampCardId).findUsableSharedCodeByCode(code);
        const id = randomUUID();
        this.record({
            type: "StampSubmitted",
            data: {
                id,
                stampCardId,
                reason: stamp.reason,
                sharedCodeId: sharedCode.id,
                issuer: stamp.issuerName,
                issuedOn: new Date()
            }
        });
        return id;
    }

    approveStamp(stampCardId: string, stampId: string, issuer: string): void {
        this.findStampCard(stampCardId).findPendingStamp(stampId);
        this.record({ type: "StampApproved", data: { id: stampId, stampCardId, issuer, issuedOn: new Date() } });
    }

    /**
     * Approves several pending stamps at once. Either all of them are approved or none.
     */
    approveStamps(stampIds: readonly string[], issuer: string): void {
        stampIds.forEach(stampId => this.approvePendingStamp(stampId, issuer));
    }

    approvePendingStamp(stampId: string, issuer: string): void {
        this.approveStamp(this.stampCardContaining(stampId).id, stampId, issuer);
    }

    rejectStamp(stampCardId: string, stampId: string, issuer: string): void {
        this.findStampCard(stampCardId).findPendingStamp(stampId);
        this.record({ type: "StampRejected", data: { id: stampId, stampCardId, issuer, issuedOn: new Date() } });
    }

    private stampCardContaining(stampId: string): StampCard {
        const stampCard = this.listStampCards().find(card => card.containsStamp(stampId));
        if (stampCard === undefined) {
            throw new StampNotFoundError(stampId);
        }
        return stampCard;
    }

    private ensureExists(): void {
        if (!this.exists) {
            throw new TeamNotFoundError(this.streamName);
        }
    }

    private ensureUniquePlayer(player: PlayerDto, ignoredPlayerId?: string): void {
        const duplicateExists = [...this.players.values()]
            .some(existing => existing.id !== ignoredPlayerId && isSamePlayer(existing, player));
        if (duplicateExists) {
            throw new PlayerAlreadyExistsError(player);
        }
    }

    private record(event: TeamEvent): void {
        this.apply(event).match(
            () => this.pendingEvents.push(event),
            reason => { throw new InconsistentStreamError(this.streamName, reason); });
    }

    private apply(event: TeamEvent): Result<void, string> {
        switch (event.type) {
            case "TeamAdded": {
                const { id, club, name } = event.data;
                this.details = { id, club, name };
                this.exists = true;
                this.players.clear();
                this.coaches.clear();
                this.stampCards.clear();
                return ok(undefined);
            }
            case "TeamDeleted":
                this.exists = false;
                return ok(undefined);
            case "CoachAdded":
                this.coaches.add(event.data.email.toLowerCase());
                return ok(undefined);
            case "PlayerAdded": {
                const { id, firstName, lastName, birthdate } = event.data;
                this.players.set(id, { id, firstName, lastName, birthdate, active: true });
                return ok(undefined);
            }
            case "PlayerUpdated": {
                const { firstName, lastName, birthdate } = event.data;
                return this.playerReferencedBy(event)
                    .map(player => { Object.assign(player, { firstName, lastName, birthdate }); });
            }
            case "PlayerRemoved":
                return this.playerReferencedBy(event).map(player => { this.players.delete(player.id); });
            case "PlayerEnabled":
                return this.playerReferencedBy(event).map(player => { player.active = true; });
            case "PlayerDisabled":
                return this.playerReferencedBy(event).map(player => { player.active = false; });
            default:
                return this.applyToStampCards(event);
        }
    }

    private applyToStampCards(event: TeamEvent): Result<void, string> {
        switch (event.type) {
            case "StampCardAdded": {
                const { id, playerId, accountingYear } = event.data;
                this.stampCards.set(id, new StampCard(id, playerId, accountingYear));
                return ok(undefined);
            }
            case "StampCardRemoved":
                return referenceExists(this.stampCards.delete(event.data.id), event);
            case "StampAdded": {
                const { id, reason, issuer, issuedOn } = event.data;
                return this.stampCardReferencedBy(event)
                    .map(card => card.putStamp({ id, reason, issuer, issuedOn, status: "accepted" }));
            }
            case "StampSubmitted": {
                const { id, reason, issuer, issuedOn } = event.data;
                return this.stampCardReferencedBy(event)
                    .map(card => card.putStamp({ id, reason, issuer, issuedOn, status: "pending" }));
            }
            case "StampApproved":
                return this.stampCardReferencedBy(event)
                    .andThen(card => referenceExists(card.approveStamp(event.data.id), event));
            case "StampRemoved":
            case "StampRejected":
                return this.stampCardReferencedBy(event)
                    .andThen(card => referenceExists(card.deleteStamp(event.data.id), event));
            case "SharedCodeAdded": {
                const { id, code } = event.data;
                return this.stampCardReferencedBy(event).map(card => card.putSharedCode({ id, code, revoked: false }));
            }
            case "SharedCodeRevoked":
                return this.stampCardReferencedBy(event)
                    .andThen(card => referenceExists(card.revokeSharedCode(event.data.id), event));
            case "SharedCodeRemoved":
                return this.stampCardReferencedBy(event)
                    .andThen(card => referenceExists(card.deleteSharedCode(event.data.id), event));
            default:
                return err(`${event.type} cannot be applied to a team.`);
        }
    }

    private stampCardReferencedBy(event: TeamEvent & { data: { stampCardId: string } }): Result<StampCard, string> {
        const stampCard = this.stampCards.get(event.data.stampCardId);
        return stampCard === undefined
            ? err(`${event.type} references unknown stamp card '${event.data.stampCardId}'.`)
            : ok(stampCard);
    }

    private playerReferencedBy(event: TeamEvent & { data: { id: string } }): Result<Player, string> {
        const player = this.players.get(event.data.id);
        return player === undefined
            ? err(`${event.type} references unknown player '${event.data.id}'.`)
            : ok(player);
    }
}
