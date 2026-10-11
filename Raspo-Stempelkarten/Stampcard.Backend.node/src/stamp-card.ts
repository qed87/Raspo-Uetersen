import type { StampCardDto, StampDto } from "./stamp-card-dtos.js";
import {
    InvalidShareCodeError,
    SharedCodeNotFoundError,
    SharedCodeRevokedError,
    StampNotFoundError,
    StampNotPendingError
} from "./stamp-card-errors.js";

/**
 * A stamp is only active once it is accepted. Stamps of coaches are accepted immediately,
 * stamps submitted via a share code are pending until a coach approves them.
 */
export type StampStatus = "accepted" | "pending";

export type Stamp = StampDto & { id: string, issuer: string, issuedOn: Date, status: StampStatus };

/**
 * A revoked shared code still grants read access to its stamp card, but no stamps can be submitted with it.
 */
export type SharedCode = { id: string, code: string, revoked: boolean };

/**
 * A stamp card of a team, holding its stamps and the codes it was shared with.
 * Queries throw domain errors, state changes are only made while applying events.
 */
export class StampCard {
    private readonly stamps = new Map<string, Stamp>();
    private readonly sharedCodes = new Map<string, SharedCode>();

    constructor(readonly id: string, readonly playerId: string, readonly accountingYear: number) {
    }

    isFor(card: StampCardDto): boolean {
        return this.playerId === card.playerId && this.accountingYear === card.accountingYear;
    }

    containsStamp(stampId: string): boolean {
        return this.stamps.has(stampId);
    }

    listStamps(): Stamp[] {
        return [...this.stamps.values()];
    }

    listPendingStamps(): Stamp[] {
        return this.listStamps().filter(stamp => stamp.status === "pending");
    }

    countAcceptedStamps(): number {
        return this.listStamps().filter(stamp => stamp.status === "accepted").length;
    }

    listSharedCodes(): SharedCode[] {
        return [...this.sharedCodes.values()];
    }

    findStamp(stampId: string): Stamp {
        const stamp = this.stamps.get(stampId);
        if (stamp === undefined) {
            throw new StampNotFoundError(stampId);
        }
        return stamp;
    }

    findPendingStamp(stampId: string): Stamp {
        const stamp = this.findStamp(stampId);
        if (stamp.status !== "pending") {
            throw new StampNotPendingError(stampId);
        }
        return stamp;
    }

    findSharedCode(sharedCodeId: string): SharedCode {
        const sharedCode = this.sharedCodes.get(sharedCodeId);
        if (sharedCode === undefined) {
            throw new SharedCodeNotFoundError(sharedCodeId);
        }
        return sharedCode;
    }

    findSharedCodeByCode(code: string): SharedCode {
        const sharedCode = [...this.sharedCodes.values()].find(existing => existing.code === code);
        if (sharedCode === undefined) {
            throw new InvalidShareCodeError();
        }
        return sharedCode;
    }

    findUsableSharedCodeByCode(code: string): SharedCode {
        const sharedCode = this.findSharedCodeByCode(code);
        if (sharedCode.revoked) {
            throw new SharedCodeRevokedError();
        }
        return sharedCode;
    }

    putStamp(stamp: Stamp): void {
        this.stamps.set(stamp.id, stamp);
    }

    deleteStamp(stampId: string): boolean {
        return this.stamps.delete(stampId);
    }

    approveStamp(stampId: string): boolean {
        const stamp = this.stamps.get(stampId);
        if (stamp !== undefined) {
            stamp.status = "accepted";
        }
        return stamp !== undefined;
    }

    putSharedCode(sharedCode: SharedCode): void {
        this.sharedCodes.set(sharedCode.id, sharedCode);
    }

    revokeSharedCode(sharedCodeId: string): boolean {
        const sharedCode = this.sharedCodes.get(sharedCodeId);
        if (sharedCode !== undefined) {
            sharedCode.revoked = true;
        }
        return sharedCode !== undefined;
    }

    deleteSharedCode(sharedCodeId: string): boolean {
        return this.sharedCodes.delete(sharedCodeId);
    }
}
