import type { FastifyInstance } from 'fastify';
import type { ZodTypeProvider } from 'fastify-type-provider-zod';
import { z } from 'zod';
import { playerDtoSchema, playerReferenceSchema, teamReferenceSchema, updatePlayerDtoSchema } from '../stamp-card-dtos.js';
import { errorResponseSchemas, versionedResponse, versionedResponseSchema } from './api-response.js';
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
          200: versionedResponseSchema(z.array(playerViewSchema)),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const players = await service.listPlayers(request.params.teamId, request.user);
      return reply.send(versionedResponse(players, data => data.map(toPlayerView)));
    }
  );

  routes.get(
    "/api/teams/:teamId/player/:playerId",
    {
      schema: {
        params: playerReferenceSchema,
        response: {
          200: versionedResponseSchema(playerViewSchema),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const player = await service.getPlayer(request.params, request.user);
      return reply.send(versionedResponse(player, toPlayerView));
    }
  );

  routes.post(
    "/api/teams/:teamId/player",
    {
      schema: {
        params: teamReferenceSchema,
        body: playerDtoSchema,
        response: {
          201: versionedResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const playerId = await service.addPlayer(request.params.teamId, request.body, request.user);
      return reply.code(201).send(versionedResponse(playerId));
    }
  );

  routes.put(
    "/api/teams/:teamId/player/:playerId",
    {
      schema: {
        params: playerReferenceSchema,
        body: updatePlayerDtoSchema,
        response: {
          200: versionedResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const changed = await service.updatePlayer(request.params, request.body, request.user);
      return reply.send(versionedResponse(changed));
    }
  );

  routes.delete(
    "/api/teams/:teamId/player/:playerId",
    {
      schema: {
        params: playerReferenceSchema,
        response: {
          200: versionedResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const changed = await service.removePlayer(request.params, request.user);
      return reply.send(versionedResponse(changed));
    }
  );

  routes.post(
    "/api/teams/:teamId/player/:playerId/enable",
    {
      schema: {
        params: playerReferenceSchema,
        response: {
          200: versionedResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const changed = await service.enablePlayer(request.params, request.user);
      return reply.send(versionedResponse(changed));
    }
  );

  routes.post(
    "/api/teams/:teamId/player/:playerId/disable",
    {
      schema: {
        params: playerReferenceSchema,
        response: {
          200: versionedResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const changed = await service.disablePlayer(request.params, request.user);
      return reply.send(versionedResponse(changed));
    }
  );
}
