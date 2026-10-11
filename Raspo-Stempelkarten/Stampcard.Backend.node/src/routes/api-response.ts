import { z } from 'zod';

export function apiResponseSchema<T extends z.ZodType>(data: T) {
  return z.object({
    Message: z.string(),
    HasError: z.boolean(),
    Data: data
  });
}

export function successResponse<T>(data: T) {
  return { Message: "", HasError: false, Data: data };
}

export const errorResponseSchemas = {
  404: apiResponseSchema(z.null()),
  409: apiResponseSchema(z.null())
};
