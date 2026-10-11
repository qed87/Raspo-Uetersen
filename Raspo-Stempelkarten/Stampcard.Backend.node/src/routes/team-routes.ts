import type { FastifyInstance } from 'fastify';
import type { ZodTypeProvider } from 'fastify-type-provider-zod';
import { z } from 'zod';
import { requireAdmin } from '../auth-plugin/index.js';
import { createTeamDtoSchema, teamReferenceSchema } from '../stamp-card-dtos.js';
import { apiResponseSchema, errorResponseSchemas, successResponse } from './api-response.js';
import { teamViewSchema, toTeamView } from './views.js';

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
    "/api/teams/:teamName",
    {
      schema: {
        params: teamReferenceSchema,
        response: {
          200: apiResponseSchema(teamViewSchema),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const team = await service.getTeam(request.params.teamName, request.user);
      return reply.send(successResponse(toTeamView(team)));
    }
  );

  routes.delete(
    "/api/teams/:teamName",
    {
      preHandler: requireAdmin,
      schema: {
        params: z.object({
          teamName: z.string().startsWith("team-")
        }),
        response: {
          200: apiResponseSchema(z.string()),
          ...errorResponseSchemas
        }
      }
    },
    async function (request, reply) {
      const { teamName } = request.params;
      await service.deleteTeam(teamName, request.user);
      return reply.send(successResponse(teamName));
    }
  );

  routes.put(
    "/api/teams/:teamId",
    {
      schema: {
        body: z.object({
          name: z.string(),
          concurrencyToken: z.uint64()
        }),
        params: z.object({
          teamId: z.uuidv4()
        }),
        response: {
          200: z.object({
            Message: z.string(),
            HasError: z.boolean(),
            Data: z.uint64()
          })
        }
      }
    },
    async function (request, reply) {
      const { teamId: string } = request.params;
    }
  );
}
