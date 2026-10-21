import { z } from 'zod';
import type { Versioned } from '../stamp-card-service.js';

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

/**
 * Responses of a single team carry its concurrency token, which updates of the team have to pass back.
 */
export function versionedResponseSchema<T extends z.ZodType>(data: T) {
  return apiResponseSchema(data).extend({
    ConcurrencyToken: z.int()
  });
}

export function versionedResponse<T>(versioned: Versioned<T>): VersionedResponse<T>;
export function versionedResponse<T, V>(versioned: Versioned<T>, toView: (data: T) => V): VersionedResponse<V>;
export function versionedResponse<T>(versioned: Versioned<T>, toView: (data: T) => unknown = data => data) {
  return { ...successResponse(toView(versioned.data)), ConcurrencyToken: versioned.concurrencyToken };
}

type VersionedResponse<T> = { Message: string, HasError: boolean, Data: T, ConcurrencyToken: number };

export const errorResponseSchemas = {
  404: apiResponseSchema(z.null()),
  409: apiResponseSchema(z.null())
};
