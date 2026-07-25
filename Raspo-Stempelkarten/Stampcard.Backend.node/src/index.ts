import Fastify from 'fastify'
import common from './common-plugin/index.js'
import configs from './config-loader-plugin/index.js'
import eventStore from './event-store-plugin/index.js'
import { z } from 'zod';
import type { ZodTypeProvider } from 'fastify-type-provider-zod';
import { validatorCompiler, serializerCompiler } from 'fastify-type-provider-zod';

const fastify = Fastify({
  logger: true
});

// register Zod validators und serializers
fastify.setValidatorCompiler(validatorCompiler);
fastify.setSerializerCompiler(serializerCompiler);

// register Plugins
fastify.register(common);
fastify.register(configs, { path: 'res' });
fastify.register(eventStore);

// register Routes
fastify.withTypeProvider<ZodTypeProvider>()
  .get(
    "/api/teams",
    {
      schema: {
        response: {
          200: z.object({
            Message: z.string(),
            HasError: z.boolean(),
            Data: z.array(
              z.object({
                Id: z.string(),
                Club: z.string(),
                Name: z.string(),
                Coaches: z.array(z.string())
              })
            )
          })
        }
      }
    },
    async function (request, reply) {
    }
  );

fastify.withTypeProvider<ZodTypeProvider>()
  .post(
    "/api/teams",
    {
      schema: {
        body: z.object({
          club: z.string(),
          name: z.string()
        }),
        response: {
          200: z.object({
            Message: z.string(),
            HasError: z.boolean(),
            Data: z.string()
          })
        }
      }
    },
    async function (request, reply) {
    }
  );

fastify.withTypeProvider<ZodTypeProvider>()
  .get(
    "/api/teams/:teamName",
    {
      schema: {
        params: z.object({
          teamName: z.string()
        }),
        response: {
          200: z.object({
            Message: z.string(),
            HasError: z.boolean(),
            Data: z.object({
              Id: z.string(),
              Club: z.string(),
              Name: z.string(),
              Issuer: z.string(),
              IssuedOn: z.date(),
              Deleted: z.boolean(),
              Coaches: z.array(z.string()),
              ConcurrencyToke: z.uint64()
            })
          })
        }
      }
    },
    async function (request, reply) {
      const { teamName: string } = request.params;

    }
  );

fastify.withTypeProvider<ZodTypeProvider>()
  .delete(
    "/api/teams/:teamId",
    {
      schema: {
        params: z.object({
          teamId: z.uuidv4()
        }),
        response: {
          200: z.object({
            Message: z.string(),
            HasError: z.boolean(),
            Data: z.string()
          })
        }
      }
    },
    async function (request, reply) {
      const { teamId: string } = request.params;
    }
  );

fastify.withTypeProvider<ZodTypeProvider>()
  .put(
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

// Run the server!
fastify.listen({ port: 3000 }, function (err, address) {
  if (err) {
    fastify.log.error(err)
    process.exit(1)
  }
  // Server is now listening on ${address}
  fastify.log.info(`Server is listening on ${address}`);
});