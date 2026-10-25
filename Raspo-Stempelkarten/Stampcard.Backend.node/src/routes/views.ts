import { z } from 'zod';
import type { IncompleteStampCard, PendingStamp, Player, TeamDetails } from '../stamp-card-model.js';
import type { SharedCode, SharedStampCard, Stamp, StampCard } from '../stamp-card.js';

export const teamViewSchema = z.object({
  Id: z.string(),
  Club: z.string(),
  Name: z.string(),
  Coaches: z.array(z.string())
});

export function toTeamView(team: TeamDetails) {
  return { Id: team.id, Club: team.club, Name: team.name, Coaches: team.coaches };
}

export const teamListViewSchema = teamViewSchema.extend({
  ConcurrencyToken: z.int()
});

export function toTeamListView(team: TeamDetails & { concurrencyToken: number }) {
  return { ...toTeamView(team), ConcurrencyToken: team.concurrencyToken };
}

export const playerViewSchema = z.object({
  Id: z.uuid(),
  FirstName: z.string(),
  LastName: z.string(),
  Birthdate: z.iso.date(),
  Active: z.boolean()
});

export function toPlayerView(player: Player) {
  return {
    Id: player.id,
    FirstName: player.firstName,
    LastName: player.lastName,
    Birthdate: player.birthdate,
    Active: player.active
  };
}

export const stampViewSchema = z.object({
  Id: z.uuid(),
  Reason: z.string(),
  Issuer: z.string(),
  IssuedOn: z.date(),
  Status: z.enum(["pending", "accepted"])
});

export function toStampView(stamp: Stamp) {
  return { Id: stamp.id, Reason: stamp.reason, Issuer: stamp.issuer, IssuedOn: stamp.issuedOn, Status: stamp.status };
}

export const stampCardViewSchema = z.object({
  Id: z.uuid(),
  PlayerId: z.uuid(),
  AccountingYear: z.int(),
  Stamps: z.array(stampViewSchema)
});

export function toStampCardView(card: StampCard) {
  return {
    Id: card.id,
    PlayerId: card.playerId,
    AccountingYear: card.accountingYear,
    Stamps: card.listStamps().map(toStampView)
  };
}

// External users do not see who issued a stamp, issuers of accepted stamps are coach email addresses
const sharedStampViewSchema = stampViewSchema.omit({ Issuer: true });

// The holder of the share code is shown, because the stamps the external user submits are issued in their name
export const sharedStampCardViewSchema = stampCardViewSchema.extend({
  Stamps: z.array(sharedStampViewSchema),
  HolderName: z.string()
});

export function toSharedStampCardView(shared: SharedStampCard) {
  const view = toStampCardView(shared.stampCard);
  return {
    ...view,
    Stamps: view.Stamps.map(({ Issuer, ...stamp }) => stamp),
    HolderName: shared.sharedCode.holderName
  };
}

export const pendingStampViewSchema = stampViewSchema.extend({
  StampCardId: z.uuid(),
  PlayerId: z.uuid()
});

export function toPendingStampView(pending: PendingStamp) {
  return { ...toStampView(pending.stamp), StampCardId: pending.stampCardId, PlayerId: pending.playerId };
}

export const sharedCodeViewSchema = z.object({
  Id: z.uuid(),
  Code: z.string(),
  HolderName: z.string(),
  Revoked: z.boolean()
});

export function toSharedCodeView(sharedCode: SharedCode) {
  return { Id: sharedCode.id, Code: sharedCode.code, HolderName: sharedCode.holderName, Revoked: sharedCode.revoked };
}

export const incompleteStampCardViewSchema = z.object({
  TeamId: z.string(),
  TeamName: z.string(),
  ConcurrencyToken: z.int(),
  StampCardId: z.uuid(),
  AccountingYear: z.int(),
  PlayerId: z.uuid(),
  FirstName: z.string().nullable(),
  LastName: z.string().nullable(),
  AcceptedStamps: z.int()
});

export function toIncompleteStampCardView(incomplete: IncompleteStampCard & { concurrencyToken: number }) {
  return {
    TeamId: incomplete.team.id,
    TeamName: incomplete.team.name,
    ConcurrencyToken: incomplete.concurrencyToken,
    StampCardId: incomplete.stampCard.id,
    AccountingYear: incomplete.stampCard.accountingYear,
    PlayerId: incomplete.stampCard.playerId,
    FirstName: incomplete.player?.firstName ?? null,
    LastName: incomplete.player?.lastName ?? null,
    AcceptedStamps: incomplete.acceptedStamps
  };
}
