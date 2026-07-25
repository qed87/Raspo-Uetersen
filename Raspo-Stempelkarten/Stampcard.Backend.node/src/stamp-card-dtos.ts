import z from "zod";

export const teamReadDtoSchema = z.object({
  id: z.uuid(),
  club: z.string(),
  name: z.string(),
  coaches: z.array(z.string())
});

export type TeamReadDto = z.infer<typeof teamReadDtoSchema>;