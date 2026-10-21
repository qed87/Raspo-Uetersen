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
import { errorResponseSchemas, versionedResponse, versionedResponseSchema } from './api-response.js';
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
    "/api/teams/:teamId/stampcard",
    {
      schema: {
        params: teamReferenceSchema,
        querystring: stampCardFilterSchema,
        response: {
          200: versionedResponseSchema(z.array(stampCardViewSchema)),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const cards = await service.listStampCards(request.params.teamId, request.query, request.user);
      return reply.send(versionedResponse(cards, data => data.map(toStampCardView)));
    }
  );

  routes.get(
    "/api/teams/:teamId/stampcard/:stampCardId",
    {
      schema: {
        params: stampCardReferenceSchema,
        response: {
          200: versionedResponseSchema(stampCardViewSchema),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const card = await service.getStampCard(request.params, request.user);
      return reply.send(versionedResponse(card, toStampCardView));
    }
  );

  routes.post(
    "/api/teams/:teamId/stampcard",
    {
      schema: {
        params: teamReferenceSchema,
        body: stampCardDtoSchema,
        response: {
          201: versionedResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const stampCardId = await service.addStampCard(request.params.teamId, request.body, request.user);
      return reply.code(201).send(versionedResponse(stampCardId));
    }
  );

  routes.delete(
    "/api/teams/:teamId/stampcard/:stampCardId",
    {
      schema: {
        params: stampCardReferenceSchema,
        response: {
          200: versionedResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const changed = await service.removeStampCard(request.params, request.user);
      return reply.send(versionedResponse(changed));
    }
  );

  routes.post(
    "/api/teams/:teamId/stampcard/:stampCardId/stamp",
    {
      schema: {
        params: stampCardReferenceSchema,
        body: stampDtoSchema,
        response: {
          201: versionedResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const stampId = await service.addStamp(request.params, request.body, request.user);
      return reply.code(201).send(versionedResponse(stampId));
    }
  );

  routes.delete(
    "/api/teams/:teamId/stampcard/:stampCardId/stamp/:stampId",
    {
      schema: {
        params: stampReferenceSchema,
        response: {
          200: versionedResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const changed = await service.removeStamp(request.params, request.user);
      return reply.send(versionedResponse(changed));
    }
  );

  routes.get(
    "/api/teams/:teamId/stamp/pending",
    {
      schema: {
        params: teamReferenceSchema,
        response: {
          200: versionedResponseSchema(z.array(pendingStampViewSchema)),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const pendingStamps = await service.listPendingStamps(request.params.teamId, request.user);
      return reply.send(versionedResponse(pendingStamps, data => data.map(toPendingStampView)));
    }
  );

  routes.post(
    "/api/teams/:teamId/stamp/pending/approve",
    {
      schema: {
        params: teamReferenceSchema,
        body: approveStampsDtoSchema,
        response: {
          200: versionedResponseSchema(z.array(z.uuid())),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const changed = await service.approveStamps(request.params.teamId, request.body, request.user);
      return reply.send(versionedResponse(changed));
    }
  );

  routes.post(
    "/api/teams/:teamId/stamp/pending/:stampId/approve",
    {
      schema: {
        params: pendingStampReferenceSchema,
        response: {
          200: versionedResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const changed = await service.approvePendingStamp(request.params, request.user);
      return reply.send(versionedResponse(changed));
    }
  );

  routes.post(
    "/api/teams/:teamId/stampcard/:stampCardId/stamp/:stampId/approve",
    {
      schema: {
        params: stampReferenceSchema,
        response: {
          200: versionedResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const changed = await service.approveStamp(request.params, request.user);
      return reply.send(versionedResponse(changed));
    }
  );

  routes.post(
    "/api/teams/:teamId/stampcard/:stampCardId/stamp/:stampId/reject",
    {
      schema: {
        params: stampReferenceSchema,
        response: {
          200: versionedResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const changed = await service.rejectStamp(request.params, request.user);
      return reply.send(versionedResponse(changed));
    }
  );

  routes.get(
    "/api/teams/:teamId/stampcard/:stampCardId/share",
    {
      schema: {
        params: stampCardReferenceSchema,
        response: {
          200: versionedResponseSchema(z.array(sharedCodeViewSchema)),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const sharedCodes = await service.listSharedCodes(request.params, request.user);
      return reply.send(versionedResponse(sharedCodes, data => data.map(toSharedCodeView)));
    }
  );

  routes.post(
    "/api/teams/:teamId/stampcard/:stampCardId/share",
    {
      schema: {
        params: stampCardReferenceSchema,
        response: {
          201: versionedResponseSchema(sharedCodeViewSchema),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const sharedCode = await service.shareStampCard(request.params, request.user);
      return reply.code(201).send(versionedResponse(sharedCode, toSharedCodeView));
    }
  );

  routes.post(
    "/api/teams/:teamId/stampcard/:stampCardId/share/:sharedCodeId/revoke",
    {
      schema: {
        params: sharedCodeReferenceSchema,
        response: {
          200: versionedResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const changed = await service.revokeSharedCode(request.params, request.user);
      return reply.send(versionedResponse(changed));
    }
  );

  routes.delete(
    "/api/teams/:teamId/stampcard/:stampCardId/share/:sharedCodeId",
    {
      schema: {
        params: sharedCodeReferenceSchema,
        response: {
          200: versionedResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const changed = await service.removeSharedCode(request.params, request.user);
      return reply.send(versionedResponse(changed));
    }
  );
}
