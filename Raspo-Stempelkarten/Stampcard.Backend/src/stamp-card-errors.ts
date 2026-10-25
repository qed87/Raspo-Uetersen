export class NotFoundError extends Error {
}

export class ConflictError extends Error {
}

export class AccessDeniedError extends Error {
}

export class SharedCodeRevokedError extends AccessDeniedError {
    constructor() {
        super("The share code has been revoked and can no longer be used to submit stamps.");
        this.name = "SharedCodeRevokedError";
    }
}

export class TeamNotFoundError extends NotFoundError {
    constructor(streamName: string) {
        super(`Team '${streamName}' does not exist.`);
        this.name = "TeamNotFoundError";
    }
}

export class CoachNotFoundError extends NotFoundError {
    constructor(email: string) {
        super(`Coach '${email}' does not exist.`);
        this.name = "CoachNotFoundError";
    }
}

export class PlayerNotFoundError extends NotFoundError {
    constructor(playerId: string) {
        super(`Player '${playerId}' does not exist.`);
        this.name = "PlayerNotFoundError";
    }
}

export class StampCardNotFoundError extends NotFoundError {
    constructor(stampCardId: string) {
        super(`Stamp card '${stampCardId}' does not exist.`);
        this.name = "StampCardNotFoundError";
    }
}

export class StampNotFoundError extends NotFoundError {
    constructor(stampId: string) {
        super(`Stamp '${stampId}' does not exist.`);
        this.name = "StampNotFoundError";
    }
}

export class SharedCodeNotFoundError extends NotFoundError {
    constructor(sharedCodeId: string) {
        super(`Shared code '${sharedCodeId}' does not exist.`);
        this.name = "SharedCodeNotFoundError";
    }
}

export class InvalidShareCodeError extends NotFoundError {
    constructor() {
        super("The share code is invalid or has been revoked.");
        this.name = "InvalidShareCodeError";
    }
}

export class StampNotPendingError extends ConflictError {
    constructor(stampId: string) {
        super(`Stamp '${stampId}' is not waiting for approval.`);
        this.name = "StampNotPendingError";
    }
}

export class StampCardAlreadyExistsError extends ConflictError {
    constructor(card: { playerId: string, accountingYear: number }) {
        super(`Stamp card of player '${card.playerId}' for ${card.accountingYear} already exists.`);
        this.name = "StampCardAlreadyExistsError";
    }
}

export class PlayerDisabledError extends ConflictError {
    constructor(playerId: string) {
        super(`Player '${playerId}' is disabled and collects no stamps.`);
        this.name = "PlayerDisabledError";
    }
}

export class TeamAlreadyExistsError extends ConflictError {
    constructor(club: string, name: string) {
        super(`Team '${name}' of club '${club}' already exists.`);
        this.name = "TeamAlreadyExistsError";
    }
}

export class PlayerAlreadyExistsError extends ConflictError {
    constructor(player: { firstName: string, lastName: string, birthdate: string }) {
        super(`Player '${player.firstName} ${player.lastName}' born on ${player.birthdate} already exists.`);
        this.name = "PlayerAlreadyExistsError";
    }
}

export class ConcurrentModificationError extends ConflictError {
    constructor(streamName: string) {
        super(`Team '${streamName}' was modified concurrently. Please retry.`);
        this.name = "ConcurrentModificationError";
    }
}

/**
 * A stored event that could not be replayed, either because its data is invalid
 * or because it does not fit the state built by the events before it.
 */
export type ReplayProblem = { revision: bigint, eventType: string, reason: string };

export class StreamReplayError extends Error {
    constructor(readonly streamName: string, readonly problems: readonly ReplayProblem[]) {
        const details = problems.map(problem => `#${problem.revision} ${problem.eventType}: ${problem.reason}`);
        super(`Stream '${streamName}' could not be replayed: ${details.join(" | ")}`);
        this.name = "StreamReplayError";
    }
}

export class InconsistentStreamError extends Error {
    constructor(streamName: string, reason: string) {
        super(`Stream '${streamName}' is inconsistent: ${reason}`);
        this.name = "InconsistentStreamError";
    }
}
