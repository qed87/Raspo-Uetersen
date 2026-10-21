import { after, before, describe, test } from "node:test";
import assert from "node:assert/strict";
import { createServer, type Server } from "node:http";
import type { AddressInfo } from "node:net";
import Fastify, { type FastifyInstance } from "fastify";
import fastifyPlugin from "fastify-plugin";
import { exportJWK, generateKeyPair, SignJWT, type CryptoKey } from "jose";
import auth, { AuthenticationError, ForbiddenError, requireAccountant, requireAdmin } from "../src/auth-plugin/index.js";

const ISSUER = "https://idp.test/realms/raspo";
const AUDIENCE = "stampcard-dev";

let identityProvider: Server;
let app: FastifyInstance;
let signingKey: CryptoKey;
let foreignKey: CryptoKey;

function sign(claims: Record<string, unknown>, options: { key?: CryptoKey, audience?: string } = {}): Promise<string> {
    return new SignJWT(claims)
        .setProtectedHeader({ alg: "RS256", kid: "test-key" })
        .setIssuer(ISSUER)
        .setAudience(options.audience ?? AUDIENCE)
        .setExpirationTime("5m")
        .sign(options.key ?? signingKey);
}

async function get(url: string, token?: string) {
    const headers = token === undefined ? {} : { authorization: `Bearer ${token}` };
    return app.inject({ method: "GET", url, headers });
}

async function startIdentityProvider(): Promise<string> {
    const { publicKey, privateKey } = await generateKeyPair("RS256");
    signingKey = privateKey;
    foreignKey = (await generateKeyPair("RS256")).privateKey;
    const jwks = { keys: [{ ...(await exportJWK(publicKey)), kid: "test-key", alg: "RS256" }] };
    identityProvider = createServer((_, response) => response.end(JSON.stringify(jwks)));
    await new Promise<void>(resolve => identityProvider.listen(0, "127.0.0.1", resolve));
    return `http://127.0.0.1:${(identityProvider.address() as AddressInfo).port}/certs`;
}

before(async () => {
    const jwksUri = await startIdentityProvider();
    app = Fastify();
    app.register(fastifyPlugin(async instance => {
        instance.decorate("config", { jwt: { issuer: ISSUER, audience: AUDIENCE, jwksUri, adminRole: "manager", accountingRole: "accounting" } } as never);
    }, { name: "config-loader" }));
    app.register(auth);
    app.setErrorHandler((error, request, reply) => {
        const statusCode = error instanceof AuthenticationError ? 401 : error instanceof ForbiddenError ? 403 : 500;
        return reply.code(statusCode).send();
    });
    app.get("/me", async request => request.user);
    app.get("/admin", { preHandler: requireAdmin }, async () => "ok");
    app.get("/accounting", { preHandler: requireAccountant }, async () => "ok");
    app.get("/public", { config: { public: true } }, async () => "ok");
    await app.ready();
});

after(async () => {
    await app.close();
    identityProvider.close();
});

describe("authentication", () => {
    test("a valid token identifies the user by the lower-cased email", async () => {
        const response = await get("/me", await sign({ email: "Coach@Raspo.de" }));

        assert.deepEqual(response.json(), { email: "coach@raspo.de", isAdmin: false, isAccountant: false });
    });

    test("realm roles mark admins and accountants", async () => {
        const token = await sign({ email: "a@raspo.de", realm_access: { roles: ["manager", "accounting"] } });

        const response = await get("/me", token);

        assert.deepEqual(response.json(), { email: "a@raspo.de", isAdmin: true, isAccountant: true });
    });

    test("a request without token is rejected", async () => {
        assert.equal((await get("/me")).statusCode, 401);
    });

    test("a token for another audience is rejected", async () => {
        assert.equal((await get("/me", await sign({ email: "c@raspo.de" }, { audience: "other" }))).statusCode, 401);
    });

    test("a token signed by an unknown key is rejected", async () => {
        assert.equal((await get("/me", await sign({ email: "c@raspo.de" }, { key: foreignKey }))).statusCode, 401);
    });

    test("a token without email is rejected", async () => {
        assert.equal((await get("/me", await sign({ sub: "someone" }))).statusCode, 401);
    });

    test("public routes are reachable without token", async () => {
        assert.equal((await get("/public")).statusCode, 200);
    });
});

describe("authorization", () => {
    test("coaches may not use admin routes", async () => {
        assert.equal((await get("/admin", await sign({ email: "c@raspo.de" }))).statusCode, 403);
    });

    test("admins may use admin routes", async () => {
        const token = await sign({ email: "a@raspo.de", role: ["manager"] });

        assert.equal((await get("/admin", token)).statusCode, 200);
    });

    test("admins may not use accounting routes", async () => {
        const token = await sign({ email: "a@raspo.de", roles: ["manager"] });

        assert.equal((await get("/accounting", token)).statusCode, 403);
    });

    test("accountants may use accounting routes", async () => {
        const token = await sign({ email: "b@raspo.de", realm_access: { roles: ["accounting"] } });

        assert.equal((await get("/accounting", token)).statusCode, 200);
    });
});
