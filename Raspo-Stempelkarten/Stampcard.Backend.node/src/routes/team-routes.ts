import type { FastifyInstance } from 'fastify';
import type { ZodTypeProvider } from 'fastify-type-provider-zod';
import { z } from 'zod';
import { requireAdmin } from '../auth-plugin/index.js';
import { createTeamDtoSchema, teamReferenceSchema, updateTeamDtoSchema } from '../stamp-card-dtos.js';
import { apiResponseSchema, errorResponseSchemas, successResponse } from './api-response.js';
import { teamDetailsViewSchema, teamViewSchema, toTeamDetailsView, toTeamView } from './views.js';

export default async function teamRoutes(fastify: FastifyInstance) {
  const routes = fastify.withTypeProvider<ZodTypeProvider>();
  const service = fastify.stampCardService;

  routes.get(
    "/api/teams",
    {
      schema: {
        response: {
          200: apiResponseSchema(z.array(teamViewSchema))
        }
      }
    },
    async function (request, reply) {
      const teams = await service.listTeams(request.user);
      return reply.send(successResponse(teams.map(toTeamView)));
    }
  );

  routes.post(
    "/api/teams",
    {
      preHandler: requireAdmin,
      schema: {
        body: createTeamDtoSchema,
        response: {
          201: apiResponseSchema(z.uuid()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const teamId = await service.createTeam(request.body, request.user);
      return reply.code(201).send(successResponse(teamId));
    }
  );

  routes.get(
    "/api/teams/:teamId",
    {
      schema: {
        params: teamReferenceSchema,
        response: {
          200: apiResponseSchema(teamDetailsViewSchema),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const team = await service.getTeam(request.params.teamId, request.user);
      return reply.send(successResponse(toTeamDetailsView(team)));
    }
  );

  routes.delete(
    "/api/teams/:teamId",
    {
      preHandler: requireAdmin,
      schema: {
        params: teamReferenceSchema,
        response: {
          200: apiResponseSchema(z.string()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const { teamId } = request.params;
      await service.deleteTeam(teamId, request.user);
      return reply.send(successResponse(teamId));
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
          200: apiResponseSchema(z.int()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const concurrencyToken = await service.updateTeam(request.params.teamId, request.body, request.user);
      return reply.send(successResponse(concurrencyToken));
    }
  );
}
