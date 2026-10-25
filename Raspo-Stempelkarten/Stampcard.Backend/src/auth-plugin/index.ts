import { type FastifyInstance, type FastifyRequest } from 'fastify'
import fastifyPlugin from 'fastify-plugin'
import { createRemoteJWKSet, jwtVerify, type JWTPayload } from 'jose';
import z from 'zod';
import type { AuthenticatedUser } from '../authenticated-user.js';

declare module 'fastify' {
  interface FastifyRequest {
    user: AuthenticatedUser;
  }
  interface FastifyContextConfig {
    /** Public routes are not authenticated by a JWT, request.user is not set for them. */
    public?: boolean;
  }
}

export class AuthenticationError extends Error {
  constructor(reason: string) {
    super(`Authentication failed: ${reason}`);
    this.name = "AuthenticationError";
  }
}

export class ForbiddenError extends Error {
  constructor(reason: string) {
    super(reason);
    this.name = "ForbiddenError";
  }
}

const BEARER_PREFIX = "Bearer ";

// Keycloak puts realm roles into realm_access.roles, other providers use a top-level roles claim
const claimsSchema = z.object({
  email: z.email(),
  realm_access: z.object({ roles: z.array(z.string()) }).optional(),
  role: z.array(z.string()).optional()
});

function bearerTokenOf(request: FastifyRequest): string {
  const authorization = request.headers.authorization;
  if (authorization === undefined || !authorization.startsWith(BEARER_PREFIX)) {
    throw new AuthenticationError("missing bearer token.");
  }
  return authorization.slice(BEARER_PREFIX.length);
}

type RoleNames = { adminRole: string, accountingRole: string };

function toAuthenticatedUser(payload: JWTPayload, roleNames: RoleNames): AuthenticatedUser {
  const claims = claimsSchema.safeParse(payload);
  if (!claims.success) {
    throw new AuthenticationError("token does not contain a valid email claim.");
  }
  const roles = [...claims.data.realm_access?.roles ?? [], ...claims.data.role ?? []];
  return {
    email: claims.data.email.toLowerCase(),
    isAdmin: roles.includes(roleNames.adminRole),
    isAccountant: roles.includes(roleNames.accountingRole)
  };
}

/**
 * Authenticates every request by the JWT in its Authorization header
 * and provides the authenticated user as request.user.
 */
async function authPlugin(fastify: FastifyInstance) {
  const { issuer, audience, jwksUri, ...roleNames } = fastify.config.jwt;
  const signingKeys = createRemoteJWKSet(new URL(jwksUri));

  fastify.decorateRequest("user", null as unknown as AuthenticatedUser);

  fastify.addHook("onRequest", async request => {
    if (request.routeOptions.config.public) return;
    const token = bearerTokenOf(request);
    try {
      const { payload } = await jwtVerify(token, signingKeys, { issuer, audience });
      request.user = toAuthenticatedUser(payload, roleNames);
    } catch (error) {
      if (error instanceof AuthenticationError) throw error;
      throw new AuthenticationError("invalid token.");
    }
  });
}

function requireUser(isAllowed: (user: AuthenticatedUser) => boolean, deniedMessage: string) {
  return async (request: FastifyRequest): Promise<void> => {
    if (!isAllowed(request.user)) {
      throw new ForbiddenError(deniedMessage);
    }
  };
}

export const requireAdmin = requireUser(user => user.isAdmin, "Only admins may perform this action.");

export const requireAccountant = requireUser(user => user.isAccountant, "Only accounting may perform this action.");

export default fastifyPlugin(authPlugin, { name: "auth", dependencies: ["config-loader"] });
