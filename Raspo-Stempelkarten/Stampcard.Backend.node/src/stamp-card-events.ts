import z from "zod";

export const teamAddedSchema = z.object({
  club: z.string(),
  name: z.string(),
  issuer: z.string(),
  issuedOn: z.date()
});

export type TeamAdded = z.infer<typeof teamAddedSchema>;