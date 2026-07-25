import EventEmitter from 'node:events';
import { type FastifyInstance } from 'fastify'
import fastifyPlugin from 'fastify-plugin'

declare module 'fastify' {
  interface FastifyInstance {
    eventbus: EventEmitter;
  }
}

/**
 * @param {FastifyInstance} fastify
 * @param {Object} options
 */
async function fp(fastify: FastifyInstance, options :any) {
  fastify.decorate("eventbus", new EventEmitter());
}

// Wrapping a plugin function with fastify-plugin exposes the decorators
// and hooks, declared inside the plugin to the parent scope.
export default fastifyPlugin(fp, { name: "common" })