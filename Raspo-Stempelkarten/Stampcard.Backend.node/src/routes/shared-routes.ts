import type { FastifyInstance } from 'fastify';
import type { ZodTypeProvider } from 'fastify-type-provider-zod';
import { z } from 'zod';
import {
  SHARE_CODE_HEADER,
  shareCodeHeadersSchema,
  stampCardReferenceSchema,
  stampDtoSchema
} from '../stamp-card-dtos.js';
import { apiResponseSchema, errorResponseSchemas, successResponse } from './api-response.js';
import { sharedStampCardViewSchema, toSharedStampCardView } from './views.js';

/**
 * Public routes for external users. They are authorized by the share code header instead of a JWT
 * and rate limited per client to make guessing share codes impractical.
 */
const publicRouteConfig = {
  public: true,
  rateLimit: { max: 30, timeWindow: "1 minute" }
};

export default async function sharedRoutes(fastify: FastifyInstance) {
  const routes = fastify.withTypeProvider<ZodTypeProvider>();
  const service = fastify.stampCardService;

  routes.get(
    "/api/shared/teams/:teamId/stampcard/:stampCardId",
    {
      config: publicRouteConfig,
      schema: {
        params: stampCardReferenceSchema,
        headers: shareCodeHeadersSchema,
        response: {
          200: apiResponseSchema(sharedStampCardViewSchema),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const card = await service.getSharedStampCard(request.params, request.headers[SHARE_CODE_HEADER]);
      return reply.send(successResponse(toSharedStampCardView(card)));
    }
  );

  routes.post(
    "/api/shared/teams/:teamId/stampcard/:stampCardId/stamp",
    {
      config: publicRouteConfig,
      schema: {
        params: stampCardReferenceSchema,
        headers: shareCodeHeadersSchema,
        body: stampDtoSchema,
        response: {
          201: apiResponseSchema(z.uuid()),
          403: apiResponseSchema(z.null()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const stampId = await service.submitStamp(request.params, request.headers[SHARE_CODE_HEADER], request.body);
      return reply.code(201).send(successResponse(stampId));
    }
  );
}
