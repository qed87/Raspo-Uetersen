import fp from 'fastify-plugin';
import { readFileSync } from 'node:fs';
import * as z from "zod";

const DEFAULT_CLUB_NAME = "rasensport-uetersen-1926";

export const appConfig = z.object({
  club: z.object({
    name: z.string().min(1).default(DEFAULT_CLUB_NAME),
  }).default({ name: DEFAULT_CLUB_NAME }),
  eventstore: z.object({
    url: z.url(),
  }),
  jwt: z.object({
    issuer: z.url(),
    audience: z.string().min(1),
    jwksUri: z.url(),
    adminRole: z.string().min(1),
    accountingRole: z.string().min(1),
  }),
});

export type AppConfig = z.infer<typeof appConfig>;

declare module 'fastify' {
  interface FastifyInstance {
    config: AppConfig;
  }
}

export function loadConfig(path: string): AppConfig {
  const raw = readFileSync(path, 'utf-8');
  const json = JSON.parse(raw);
  
  // parse() wirft bei ungültiger Config automatisch einen Fehler
  return appConfig.parse(json);
}

export default fp(async (fastify, options: { path: string }) => {
  const env = process.env.NODE_ENV ?? undefined;
  const config = loadConfig(`./${options.path}/appsettings${(env === undefined ? '' : '.' + env)}.json`);
  
  fastify.decorate('config', config);
}, { name: "config-loader" });