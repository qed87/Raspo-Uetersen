import z from "zod";

export const teamReadDtoSchema = z.object({
  id: z.string(),
  club: z.string(),
  name: z.string(),
  coaches: z.array(z.string())
});

export type TeamReadDto = z.infer<typeof teamReadDtoSchema>;

export function normalizeName(value: string): string {
  return value.trim().toLowerCase().replace(/\s+/g, "-");
}

export const normalizedNameSchema = z.string()
  .trim()
  .min(1)
  .transform(normalizeName);

export const createTeamDtoSchema = z.object({
  club: normalizedNameSchema,
  name: normalizedNameSchema,
  coaches: z.array(z.email()).default([]).transform(coaches => [...new Set(coaches.map(email => email.toLowerCase()))])
});

export type CreateTeamDto = z.infer<typeof createTeamDtoSchema>;

const MAX_PLAYER_AGE_IN_YEARS = 120;

function isNotInFuture(date: string): boolean {
  return new Date(date) <= new Date();
}

function isPlausibleBirthdate(birthdate: string): boolean {
  const oldestBirthdate = new Date();
  oldestBirthdate.setFullYear(oldestBirthdate.getFullYear() - MAX_PLAYER_AGE_IN_YEARS);
  return isNotInFuture(birthdate) && new Date(birthdate) > oldestBirthdate;
}

export const playerDtoSchema = z.object({
  firstName: z.string().trim().min(2),
  lastName: z.string().trim().min(2),
  birthdate: z.iso.date().refine(isPlausibleBirthdate, {
    message: `Birthdate must not be in the future or more than ${MAX_PLAYER_AGE_IN_YEARS} years ago.`
  })
});

export type PlayerDto = z.infer<typeof playerDtoSchema>;

const MAX_ACCOUNTING_YEARS_IN_PAST = 25;

function isRecentAccountingYear(accountingYear: number): boolean {
  return accountingYear > new Date().getFullYear() - MAX_ACCOUNTING_YEARS_IN_PAST;
}

export const stampCardDtoSchema = z.object({
  playerId: z.uuid(),
  accountingYear: z.int().refine(isRecentAccountingYear, {
    message: `Accounting year must not be more than ${MAX_ACCOUNTING_YEARS_IN_PAST} years in the past.`
  })
});

export type StampCardDto = z.infer<typeof stampCardDtoSchema>;

export const teamReferenceSchema = z.object({
  teamName: normalizedNameSchema
});

export const playerReferenceSchema = teamReferenceSchema.extend({
  playerId: z.uuid()
});

export type PlayerReference = z.infer<typeof playerReferenceSchema>;

export const stampCardReferenceSchema = teamReferenceSchema.extend({
  stampCardId: z.uuid()
});

export type StampCardReference = z.infer<typeof stampCardReferenceSchema>;

export const stampDtoSchema = z.object({
  reason: z.string().trim().min(1)
});

export type StampDto = z.infer<typeof stampDtoSchema>;

export const stampReferenceSchema = stampCardReferenceSchema.extend({
  stampId: z.uuid()
});

export type StampReference = z.infer<typeof stampReferenceSchema>;

export const submittedStampDtoSchema = stampDtoSchema.extend({
  issuerName: z.string().trim().min(2)
});

export type SubmittedStampDto = z.infer<typeof submittedStampDtoSchema>;

export const sharedCodeReferenceSchema = stampCardReferenceSchema.extend({
  sharedCodeId: z.uuid()
});

export type SharedCodeReference = z.infer<typeof sharedCodeReferenceSchema>;

export const approveStampsDtoSchema = z.object({
  stampIds: z.array(z.uuid()).min(1).transform(stampIds => [...new Set(stampIds)])
});

export type ApproveStampsDto = z.infer<typeof approveStampsDtoSchema>;

export const pendingStampReferenceSchema = teamReferenceSchema.extend({
  stampId: z.uuid()
});

export type PendingStampReference = z.infer<typeof pendingStampReferenceSchema>;

export const incompleteStampCardQuerySchema = z.object({
  accountingYear: z.coerce.number().int(),
  requiredStamps: z.coerce.number().int().min(1)
});

export type IncompleteStampCardQuery = z.infer<typeof incompleteStampCardQuerySchema>;

export const stampCardFilterSchema = z.object({
  playerId: z.uuid().optional(),
  accountingYear: z.coerce.number().int().optional()
});

export type StampCardFilter = z.infer<typeof stampCardFilterSchema>;

export const SHARE_CODE_HEADER = "x-share-code";

export const shareCodeHeadersSchema = z.object({
  [SHARE_CODE_HEADER]: z.string().min(1)
});
