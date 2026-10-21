import type { FastifyInstance } from 'fastify';
import type { ZodTypeProvider } from 'fastify-type-provider-zod';
import { z } from 'zod';
import { playerDtoSchema, playerReferenceSchema, teamReferenceSchema } from '../stamp-card-dtos.js';
import { apiResponseSchema, errorResponseSchemas, successResponse } from './api-response.js';
import { playerViewSchema, toPlayerView } from './views.js';

export default async function playerRoutes(fastify: FastifyInstance) {
  const routes = fastify.withTypeProvider<ZodTypeProvider>();
  const service = fastify.stampCardService;

  routes.get(
    "/api/teams/:teamId/player",
    {
      schema: {
        params: teamReferenceSchema,
        response: {
          200: apiResponseSchema(z.array(playerViewSchema)),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const players = await service.listPlayers(request.params.teamId, request.user);
      return reply.send(successResponse(players.map(toPlayerView)));
    }
  );

  routes.get(
    "/api/teams/:teamId/player/:playerId",
    {
      schema: {
        params: playerReferenceSchema,
        response: {
          200: apiResponseSchema(playerViewSchema),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const player = await service.getPlayer(request.params, request.user);
      return reply.send(successResponse(toPlayerView(player)));
    }
  );

  routes.post(
    "/api/teams/:teamId/player",
    {
      schema: {
        params: teamReferenceSchema,
        body: playerDtoSchema,
        response: {
          201: apiResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const playerId = await service.addPlayer(request.params.teamId, request.body, request.user);
      return reply.code(201).send(successResponse(playerId));
    }
  );

  routes.put(
    "/api/teams/:teamId/player/:playerId",
    {
      schema: {
        params: playerReferenceSchema,
        body: playerDtoSchema,
        response: {
          200: apiResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      await service.updatePlayer(request.params, request.body, request.user);
      return reply.send(successResponse(request.params.playerId));
    }
  );

  routes.delete(
    "/api/teams/:teamId/player/:playerId",
    {
      schema: {
        params: playerReferenceSchema,
        response: {
          200: apiResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      await service.removePlayer(request.params, request.user);
      return reply.send(successResponse(request.params.playerId));
    }
  );

  routes.post(
    "/api/teams/:teamId/player/:playerId/enable",
    {
      schema: {
        params: playerReferenceSchema,
        response: {
          200: apiResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      await service.enablePlayer(request.params, request.user);
      return reply.send(successResponse(request.params.playerId));
    }
  );

  routes.post(
    "/api/teams/:teamId/player/:playerId/disable",
    {
      schema: {
        params: playerReferenceSchema,
        response: {
          200: apiResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      await service.disablePlayer(request.params, request.user);
      return reply.send(successResponse(request.params.playerId));
    }
  );
}
