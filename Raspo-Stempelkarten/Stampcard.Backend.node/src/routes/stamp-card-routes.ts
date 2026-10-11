import type { FastifyInstance } from 'fastify';
import type { ZodTypeProvider } from 'fastify-type-provider-zod';
import { z } from 'zod';
import {
  approveStampsDtoSchema,
  pendingStampReferenceSchema,
  sharedCodeReferenceSchema,
  stampCardDtoSchema,
  stampCardFilterSchema,
  stampCardReferenceSchema,
  stampDtoSchema,
  stampReferenceSchema,
  teamReferenceSchema
} from '../stamp-card-dtos.js';
import { apiResponseSchema, errorResponseSchemas, successResponse } from './api-response.js';
import {
  pendingStampViewSchema,
  sharedCodeViewSchema,
  stampCardViewSchema,
  toPendingStampView,
  toSharedCodeView,
  toStampCardView
} from './views.js';

export default async function stampCardRoutes(fastify: FastifyInstance) {
  const routes = fastify.withTypeProvider<ZodTypeProvider>();
  const service = fastify.stampCardService;

  routes.get(
    "/api/teams/:teamName/stampcard",
    {
      schema: {
        params: teamReferenceSchema,
        querystring: stampCardFilterSchema,
        response: {
          200: apiResponseSchema(z.array(stampCardViewSchema)),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const cards = await service.listStampCards(request.params.teamName, request.query, request.user);
      return reply.send(successResponse(cards.map(toStampCardView)));
    }
  );

  routes.get(
    "/api/teams/:teamName/stampcard/:stampCardId",
    {
      schema: {
        params: stampCardReferenceSchema,
        response: {
          200: apiResponseSchema(stampCardViewSchema),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const card = await service.getStampCard(request.params, request.user);
      return reply.send(successResponse(toStampCardView(card)));
    }
  );

  routes.post(
    "/api/teams/:teamName/stampcard",
    {
      schema: {
        params: teamReferenceSchema,
        body: stampCardDtoSchema,
        response: {
          201: apiResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const stampCardId = await service.addStampCard(request.params.teamName, request.body, request.user);
      return reply.code(201).send(successResponse(stampCardId));
    }
  );

  routes.delete(
    "/api/teams/:teamName/stampcard/:stampCardId",
    {
      schema: {
        params: stampCardReferenceSchema,
        response: {
          200: apiResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      await service.removeStampCard(request.params, request.user);
      return reply.send(successResponse(request.params.stampCardId));
    }
  );

  routes.post(
    "/api/teams/:teamName/stampcard/:stampCardId/stamp",
    {
      schema: {
        params: stampCardReferenceSchema,
        body: stampDtoSchema,
        response: {
          201: apiResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const stampId = await service.addStamp(request.params, request.body, request.user);
      return reply.code(201).send(successResponse(stampId));
    }
  );

  routes.delete(
    "/api/teams/:teamName/stampcard/:stampCardId/stamp/:stampId",
    {
      schema: {
        params: stampReferenceSchema,
        response: {
          200: apiResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      await service.removeStamp(request.params, request.user);
      return reply.send(successResponse(request.params.stampId));
    }
  );

  routes.get(
    "/api/teams/:teamName/stamp/pending",
    {
      schema: {
        params: teamReferenceSchema,
        response: {
          200: apiResponseSchema(z.array(pendingStampViewSchema)),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const pendingStamps = await service.listPendingStamps(request.params.teamName, request.user);
      return reply.send(successResponse(pendingStamps.map(toPendingStampView)));
    }
  );

  routes.post(
    "/api/teams/:teamName/stamp/pending/approve",
    {
      schema: {
        params: teamReferenceSchema,
        body: approveStampsDtoSchema,
        response: {
          200: apiResponseSchema(z.array(z.uuid())),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      await service.approveStamps(request.params.teamName, request.body, request.user);
      return reply.send(successResponse(request.body.stampIds));
    }
  );

  routes.post(
    "/api/teams/:teamName/stamp/pending/:stampId/approve",
    {
      schema: {
        params: pendingStampReferenceSchema,
        response: {
          200: apiResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      await service.approvePendingStamp(request.params, request.user);
      return reply.send(successResponse(request.params.stampId));
    }
  );

  routes.post(
    "/api/teams/:teamName/stampcard/:stampCardId/stamp/:stampId/approve",
    {
      schema: {
        params: stampReferenceSchema,
        response: {
          200: apiResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      await service.approveStamp(request.params, request.user);
      return reply.send(successResponse(request.params.stampId));
    }
  );

  routes.post(
    "/api/teams/:teamName/stampcard/:stampCardId/stamp/:stampId/reject",
    {
      schema: {
        params: stampReferenceSchema,
        response: {
          200: apiResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      await service.rejectStamp(request.params, request.user);
      return reply.send(successResponse(request.params.stampId));
    }
  );

  routes.get(
    "/api/teams/:teamName/stampcard/:stampCardId/share",
    {
      schema: {
        params: stampCardReferenceSchema,
        response: {
          200: apiResponseSchema(z.array(sharedCodeViewSchema)),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const sharedCodes = await service.listSharedCodes(request.params, request.user);
      return reply.send(successResponse(sharedCodes.map(toSharedCodeView)));
    }
  );

  routes.post(
    "/api/teams/:teamName/stampcard/:stampCardId/share",
    {
      schema: {
        params: stampCardReferenceSchema,
        response: {
          201: apiResponseSchema(sharedCodeViewSchema),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const sharedCode = await service.shareStampCard(request.params, request.user);
      return reply.code(201).send(successResponse(toSharedCodeView(sharedCode)));
    }
  );

  routes.post(
    "/api/teams/:teamName/stampcard/:stampCardId/share/:sharedCodeId/revoke",
    {
      schema: {
        params: sharedCodeReferenceSchema,
        response: {
          200: apiResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      await service.revokeSharedCode(request.params, request.user);
      return reply.send(successResponse(request.params.sharedCodeId));
    }
  );

  routes.delete(
    "/api/teams/:teamName/stampcard/:stampCardId/share/:sharedCodeId",
    {
      schema: {
        params: sharedCodeReferenceSchema,
        response: {
          200: apiResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      await service.removeSharedCode(request.params, request.user);
      return reply.send(successResponse(request.params.sharedCodeId));
    }
  );
}
