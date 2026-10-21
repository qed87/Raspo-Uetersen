import { err, ok, type Result } from "neverthrow";
import z from "zod";

const issuedSchema = z.object({
  issuer: z.string(),
  issuedOn: z.coerce.date()
});

/**
 * Team ids are UUIDs written without hyphens, the format of the former .NET backend.
 */
export const teamIdSchema = z.string().regex(/^[0-9a-f]{32}$/, "Invalid team id");

export const teamAddedSchema = issuedSchema.extend({
  id: teamIdSchema,
  club: z.string(),
  name: z.string()
});

export const teamUpdatedSchema = issuedSchema.extend({
  name: z.string()
});

export const coachAddedSchema = issuedSchema.extend({
  email: z.email()
});

export const teamDeletedSchema = issuedSchema;

export const playerAddedSchema = issuedSchema.extend({
  id: z.uuid(),
  firstName: z.string(),
  lastName: z.string(),
  birthdate: z.iso.date()
});

export const playerUpdatedSchema = playerAddedSchema;

const playerReferenceSchema = issuedSchema.extend({
  id: z.uuid()
});

export const playerRemovedSchema = playerReferenceSchema;
export const playerEnabledSchema = playerReferenceSchema;
export const playerDisabledSchema = playerReferenceSchema;

export const stampCardAddedSchema = issuedSchema.extend({
  id: z.uuid(),
  playerId: z.uuid(),
  accountingYear: z.int()
});

export const stampCardRemovedSchema = issuedSchema.extend({
  id: z.uuid()
});

export const stampAddedSchema = issuedSchema.extend({
  id: z.uuid(),
  stampCardId: z.uuid(),
  reason: z.string()
});

const stampReferenceSchema = issuedSchema.extend({
  id: z.uuid(),
  stampCardId: z.uuid()
});

export const stampRemovedSchema = stampReferenceSchema;

export const stampSubmittedSchema = stampAddedSchema.extend({
  sharedCodeId: z.uuid()
});

export const stampApprovedSchema = stampReferenceSchema;
export const stampRejectedSchema = stampReferenceSchema;

export const sharedCodeAddedSchema = issuedSchema.extend({
  id: z.uuid(),
  stampCardId: z.uuid(),
  code: z.string()
});

export const sharedCodeRemovedSchema = issuedSchema.extend({
  id: z.uuid(),
  stampCardId: z.uuid()
});

export const sharedCodeRevokedSchema = sharedCodeRemovedSchema;

const teamEventSchemas = {
  TeamAdded: teamAddedSchema,
  TeamUpdated: teamUpdatedSchema,
  CoachAdded: coachAddedSchema,
  TeamDeleted: teamDeletedSchema,
  PlayerAdded: playerAddedSchema,
  PlayerUpdated: playerUpdatedSchema,
  PlayerRemoved: playerRemovedSchema,
  PlayerEnabled: playerEnabledSchema,
  PlayerDisabled: playerDisabledSchema,
  StampCardAdded: stampCardAddedSchema,
  StampCardRemoved: stampCardRemovedSchema,
  StampAdded: stampAddedSchema,
  StampRemoved: stampRemovedSchema,
  StampSubmitted: stampSubmittedSchema,
  StampApproved: stampApprovedSchema,
  StampRejected: stampRejectedSchema,
  SharedCodeAdded: sharedCodeAddedSchema,
  SharedCodeRemoved: sharedCodeRemovedSchema,
  SharedCodeRevoked: sharedCodeRevokedSchema
};

export type TeamEventType = keyof typeof teamEventSchemas;

export type TeamEvent = {
  [Type in TeamEventType]: { type: Type, data: z.infer<(typeof teamEventSchemas)[Type]> }
}[TeamEventType];

export function isTeamEventType(type: string): type is TeamEventType {
  return Object.hasOwn(teamEventSchemas, type);
}

export function parseTeamEvent(type: TeamEventType, data: unknown): Result<TeamEvent, string> {
  const parsed = teamEventSchemas[type].safeParse(data);
  return parsed.success
    ? ok({ type, data: parsed.data } as TeamEvent)
    : err(`invalid data (${z.prettifyError(parsed.error).replaceAll("\n", " ")})`);
}
