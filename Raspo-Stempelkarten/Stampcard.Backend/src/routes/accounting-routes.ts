import type { FastifyInstance } from 'fastify';
import type { ZodTypeProvider } from 'fastify-type-provider-zod';
import { z } from 'zod';
import { requireAccountant } from '../auth-plugin/index.js';
import { incompleteStampCardQuerySchema } from '../stamp-card-dtos.js';
import { apiResponseSchema, successResponse } from './api-response.js';
import { incompleteStampCardViewSchema, toIncompleteStampCardView } from './views.js';

export default async function accountingRoutes(fastify: FastifyInstance) {
  const routes = fastify.withTypeProvider<ZodTypeProvider>();
  const service = fastify.stampCardService;

  routes.get(
    "/api/accounting/stampcard/incomplete",
    {
      preHandler: requireAccountant,
      schema: {
        querystring: incompleteStampCardQuerySchema,
        response: {
          200: apiResponseSchema(z.array(incompleteStampCardViewSchema))
        }
      }
    },
    async function (request, reply) {
      const incompleteCards = await service.listIncompleteStampCards(request.query);
      return reply.send(successResponse(incompleteCards.map(toIncompleteStampCardView)));
    }
  );
}
