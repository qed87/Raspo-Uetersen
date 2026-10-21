import Fastify from 'fastify'
import rateLimit from '@fastify/rate-limit'
import configs from './config-loader-plugin/index.js'
import eventStore from './event-store-plugin/index.js'
import auth, { AuthenticationError, ForbiddenError } from './auth-plugin/index.js'
import { validatorCompiler, serializerCompiler } from 'fastify-type-provider-zod';
import { AccessDeniedError, ConflictError, NotFoundError } from './stamp-card-errors.js';
import teamRoutes from './routes/team-routes.js';
import playerRoutes from './routes/player-routes.js';
import stampCardRoutes from './routes/stamp-card-routes.js';
import sharedRoutes from './routes/shared-routes.js';
import accountingRoutes from './routes/accounting-routes.js';

function statusCodeOf(error: Error): number | undefined {
  if (error instanceof AuthenticationError) return 401;
  if (error instanceof ForbiddenError || error instanceof AccessDeniedError) return 403;
  if (error instanceof NotFoundError) return 404;
  if (error instanceof ConflictError) return 409;
  return undefined;
}

const fastify = Fastify({
  logger: true
});

// register Zod validators und serializers
fastify.setValidatorCompiler(validatorCompiler);
fastify.setSerializerCompiler(serializerCompiler);

// map auth and domain errors to HTTP status codes, everything else is handled by Fastify
fastify.setErrorHandler(function (error, request, reply) {
  const statusCode = error instanceof Error ? statusCodeOf(error) : undefined;
  if (!(error instanceof Error) || statusCode === undefined) {
    throw error;
  }
  return reply.code(statusCode).send({ Message: error.message, HasError: true, Data: null });
});

// register Plugins
fastify.register(configs, { path: 'res' });
fastify.register(auth);
fastify.register(eventStore);

// only routes with a rateLimit config are limited
fastify.register(rateLimit, { global: false });

// register Routes
fastify.register(teamRoutes);
fastify.register(playerRoutes);
fastify.register(stampCardRoutes);
fastify.register(sharedRoutes);
fastify.register(accountingRoutes);

// Run the server!
fastify.listen({ port: 3000 }, function (err, address) {
  if (err) {
    fastify.log.error(err)
    process.exit(1)
  }
  // Server is now listening on ${address}
  fastify.log.info(`Server is listening on ${address}`);
});
