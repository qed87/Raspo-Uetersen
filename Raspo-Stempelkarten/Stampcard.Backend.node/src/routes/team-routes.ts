import type { FastifyInstance } from 'fastify';
import type { ZodTypeProvider } from 'fastify-type-provider-zod';
import { z } from 'zod';
import { requireAdmin } from '../auth-plugin/index.js';
import { createTeamDtoSchema, teamReferenceSchema, updateTeamDtoSchema } from '../stamp-card-dtos.js';
import { teamIdSchema } from '../stamp-card-events.js';
import {
  apiResponseSchema,
  errorResponseSchemas,
  successResponse,
  versionedResponse,
  versionedResponseSchema
} from './api-response.js';
import { teamListViewSchema, teamViewSchema, toTeamListView, toTeamView } from './views.js';

export default async function teamRoutes(fastify: FastifyInstance) {
  const routes = fastify.withTypeProvider<ZodTypeProvider>();
  const service = fastify.stampCardService;

  // The list covers several teams, so every team carries its own concurrency token.
  routes.get(
    "/api/teams",
    {
      schema: {
        response: {
          200: apiResponseSchema(z.array(teamListViewSchema))
        }
      }
    },
    async function (request, reply) {
      const teams = await service.listTeams(request.user);
      return reply.send(successResponse(teams.map(toTeamListView)));
    }
  );

  routes.post(
    "/api/teams",
    {
      preHandler: requireAdmin,
      schema: {
        body: createTeamDtoSchema,
        response: {
          201: versionedResponseSchema(teamIdSchema),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const teamId = await service.createTeam(request.body, request.user);
      return reply.code(201).send(versionedResponse(teamId));
    }
  );

  routes.get(
    "/api/teams/:teamId",
    {
      schema: {
        params: teamReferenceSchema,
        response: {
          200: versionedResponseSchema(teamViewSchema),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const team = await service.getTeam(request.params.teamId, request.user);
      return reply.send(versionedResponse(team, toTeamView));
    }
  );

  routes.put(
    "/api/teams/:teamId",
    {
      preHandler: requireAdmin,
      schema: {
        params: teamReferenceSchema,
        body: updateTeamDtoSchema,
        response: {
          200: versionedResponseSchema(teamIdSchema),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const teamId = await service.updateTeam(request.params.teamId, request.body, request.user);
      return reply.send(versionedResponse(teamId));
    }
  );

  routes.delete(
    "/api/teams/:teamId",
    {
      preHandler: requireAdmin,
      schema: {
        params: teamReferenceSchema,
        response: {
          200: versionedResponseSchema(teamIdSchema),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const teamId = await service.deleteTeam(request.params.teamId, request.user);
      return reply.send(versionedResponse(teamId));
    }
  );
}
