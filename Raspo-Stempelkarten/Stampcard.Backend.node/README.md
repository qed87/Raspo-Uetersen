# Stampcard.Backend.node

Node.js backend of the Raspo stamp cards. It provides the REST API for teams, players, stamp cards, stamps and sharing stamp cards with external users, and replaces the former .NET backend ([Stampcard.Backend](../Stampcard.Backend)).

The business introduction (teams, coaches, stamp cards, accounting) is in the [repository README](../../README.md).

## Contents

- [Tech stack](#tech-stack)
- [Getting started](#getting-started)
- [Configuration](#configuration)
- [Project structure](#project-structure)
- [Architecture](#architecture)
- [Authentication and permissions](#authentication-and-permissions)
- [Optimistic concurrency](#optimistic-concurrency)
- [API](#api)
- [Events](#events)
- [Tests](#tests)
- [Migration from the .NET backend](#migration-from-the-net-backend)

## Tech stack

| Area | Library |
| --- | --- |
| Runtime | Node.js 24, TypeScript (ES modules, `nodenext`) |
| Web framework | [Fastify 5](https://fastify.dev/) with [`fastify-type-provider-zod`](https://github.com/turkerdev/fastify-type-provider-zod) |
| Validation | [Zod 4](https://zod.dev/) for requests, responses, configuration and stored events |
| Persistence | [KurrentDB](https://www.kurrent.io/) (event store) via `@kurrent/kurrentdb-client` |
| Authentication | JWT verification with [`jose`](https://github.com/panva/jose) against the JWKS endpoint of the identity provider (cloud-iam / Keycloak) |
| Error handling | [`neverthrow`](https://github.com/supermacro/neverthrow) in the repository, exceptions at the service boundary |
| Rate limiting | `@fastify/rate-limit` (public routes only) |

## Getting started

Prerequisites:

- Node.js 24
- a reachable KurrentDB with projections enabled (`KURRENTDB_RUN_PROJECTIONS=All`, `KURRENTDB_START_STANDARD_PROJECTIONS=true`), e.g. locally via Docker:

```bash
docker run -d --name kurrentdb -p 2113:2113 -e KURRENTDB_INSECURE=true -e KURRENTDB_RUN_PROJECTIONS=All -e KURRENTDB_START_STANDARD_PROJECTIONS=true docker.kurrent.io/kurrent-latest/kurrentdb:latest
```

Install the dependencies and start in watch mode:

```bash
npm install
```

```bash
npm run dev
```

The server listens on port `3000`. The working directory must be the project directory, because the configuration is loaded from `res/` relative to it.

### npm scripts

| Script | Purpose |
| --- | --- |
| `npm run dev` | Starts `src/index.ts` with `tsx watch` and reloads on changes |
| `npm run build` | Compiles to `dist/` |
| `npm start` | Compiles and starts `dist/index.js` |
| `npm test` | Runs all tests in `test/` with the Node test runner |
| `npm run typecheck` | Type-checks the sources **and** the tests (`tsconfig.test.json`) |
| `npm run migrate:legacy` | Migrates the streams of the .NET backend, see [Migration](#migration-from-the-net-backend) |

VS Code has the launch configurations **Backend** (debugging `src/index.ts`) and **Tests (current file)** in [.vscode/launch.json](.vscode/launch.json).

## Configuration

The configuration is read from `res/appsettings.json` on startup. If `NODE_ENV` is set, `res/appsettings.<NODE_ENV>.json` is used instead (e.g. `appsettings.production.json`). The files are **not** merged, so the environment-specific file must be complete.

The file is validated with Zod ([config-loader-plugin/index.ts](src/config-loader-plugin/index.ts)); an invalid configuration aborts the startup.

```json
{
    "club": {
        "name": "rasensport-uetersen-1926"
    },
    "eventstore": {
        "url": "kurrentdb://localhost:2113?tls=false"
    },
    "jwt": {
        "issuer": "https://lemur-10.cloud-iam.com/auth/realms/raspo1926",
        "audience": "stampcard-dev",
        "jwksUri": "https://lemur-10.cloud-iam.com/auth/realms/raspo1926/protocol/openid-connect/certs",
        "adminRole": "manager",
        "accountingRole": "accounting"
    }
}
```

| Key | Meaning |
| --- | --- |
| `club.name` | Club that new teams are assigned to. Optional, defaults to `rasensport-uetersen-1926`. Team names are unique within the club. |
| `eventstore.url` | Connection string of the KurrentDB |
| `jwt.issuer` | Expected `iss` claim |
| `jwt.audience` | Expected `aud` claim |
| `jwt.jwksUri` | JWKS endpoint the signing keys are loaded from |
| `jwt.adminRole` | Role name of the club management (admin) |
| `jwt.accountingRole` | Role name of the club office (accounting) |

## Project structure

```
src/
├── index.ts                     Entry point: Fastify, plugins, routes, error mapping
├── config-loader-plugin/        Loads and validates appsettings*.json → fastify.config
├── auth-plugin/                 JWT verification → request.user, requireAdmin / requireAccountant
├── event-store-plugin/          KurrentDB client, projection all-club-teams → fastify.stampCardService
├── routes/
│   ├── team-routes.ts           /api/teams
│   ├── player-routes.ts         /api/teams/:teamId/player
│   ├── stamp-card-routes.ts     Stamp cards, stamps, approvals, share codes
│   ├── shared-routes.ts         Public routes for external users (/api/shared)
│   ├── accounting-routes.ts     Evaluation for the club office
│   ├── api-response.ts          Response envelope { Message, HasError, Data, ConcurrencyToken }
│   └── views.ts                 Mapping of the domain model to the API representation
├── stamp-card-service.ts        Use cases: load, authorize, change, save
├── stamp-card-repository.ts     TeamRepository: read/replay streams, append events
├── stamp-card-model.ts          TeamAggregate (team, coaches, players, stamp cards)
├── stamp-card.ts                StampCard (stamps, share codes)
├── stamp-card-events.ts         Event types and their Zod schemas
├── stamp-card-dtos.ts           Request schemas (body, params, query, headers)
├── stamp-card-errors.ts         Domain errors
└── authenticated-user.ts        Type of the authenticated user
scripts/migrate-legacy-streams.ts  Migration of the .NET backend's streams
test/                              Unit tests (Node test runner) with an in-memory event store
bruno/                             Bruno collection for end-to-end tests against a running server
res/appsettings.json               Configuration
```

## Architecture

```
HTTP ─► Route (Zod validation) ─► StampCardService ─► TeamRepository ─► KurrentDB
                                        │                   │
                                        │                   └─ replay ─► TeamAggregate / StampCard
                                        └─ projection all-club-teams (team list, name check)
```

### Event sourcing per team

Every team is a stream `team-<teamId>` of its own in KurrentDB. The team id is a UUID **without hyphens** (32 hex characters, the format of the former .NET backend). Since the stream is named after the id only, a team can be renamed without its old name remaining blocked.

Everything belonging to a team – coaches, players, stamp cards, stamps and share codes – is stored as events in this one stream. A team is therefore the consistency boundary: every change affects exactly one stream and is appended atomically.

How a change is processed in the [`StampCardService`](src/stamp-card-service.ts):

1. **Load** – the [`TeamRepository`](src/stamp-card-repository.ts) reads all events of the stream, validates each one with its Zod schema ([stamp-card-events.ts](src/stamp-card-events.ts)) and replays it in the [`TeamAggregate`](src/stamp-card-model.ts). Unknown event types are skipped. If an event does not fit the state (e.g. it references an unknown player), all problems are collected and reported as `StreamReplayError`.
2. **Authorize** – `ensureVisible` checks whether the user may see the team; updates with a token are additionally checked by `ensureUnchanged`.
3. **Change** – the aggregate's method checks the business rules, applies the new event to the state and records it as a pending change.
4. **Save** – the repository appends exactly the recorded events, expecting the loaded revision as the stream state. If the stream has changed in the meantime, appending fails with `ConcurrentModificationError` (409).

### Projection `all-club-teams`

For queries across all teams (team list, unique team name check, accounting), the [event-store-plugin](src/event-store-plugin/index.ts) creates the KurrentDB projection `all-club-teams` on startup. It reads the category `team` (which requires the standard projection `$by_category`) and keeps id, club, name, stream, coaches and concurrency token (sequence number of the last event) of every team.

If the projection already exists, its query is updated and it is **reset**, i.e. it is fully rebuilt on every startup. The projection is eventually consistent: right after a change it may lag behind briefly. Since data and token come from the same projection step, an update with an outdated token is rejected instead of overwriting newer changes.

### Error handling

Domain errors are exceptions from [stamp-card-errors.ts](src/stamp-card-errors.ts); the error handler in [index.ts](src/index.ts) maps them to HTTP status codes:

| Error class | Status |
| --- | --- |
| `AuthenticationError` | 401 |
| `ForbiddenError`, `AccessDeniedError` (e.g. `SharedCodeRevokedError`) | 403 |
| `NotFoundError` (team, player, stamp card, stamp, share code, invalid code) | 404 |
| `ConflictError` (duplicates, stamp not pending, `ConcurrentModificationError`) | 409 |

The response then looks like `{ "Message": "...", "HasError": true, "Data": null }`. All other errors are handled by Fastify itself – validation errors of the Zod schemas as 400, exceeded rate limits as 429, everything else (e.g. `StreamReplayError`, `InconsistentStreamError`, database errors) as 500. These responses use Fastify's default format, not the envelope above.

## Authentication and permissions

Every request needs an `Authorization: Bearer <JWT>` header, except the routes under `/api/shared`, which are marked as `public`. The [auth-plugin](src/auth-plugin/index.ts) verifies the signature (via `jwksUri`), `issuer` and `audience` and sets `request.user`:

- `email` – from the `email` claim, lowercased. Tokens without a valid email are rejected.
- `isAdmin` / `isAccountant` – whether `adminRole` or `accountingRole` is contained in `realm_access.roles` (Keycloak) or in the `role` claim.

| Role | May |
| --- | --- |
| **Admin** | see and edit all teams; create, rename and delete teams |
| **Coach** (every authenticated user) | only teams listing their email as coach: players, stamp cards, stamps, approvals, share codes |
| **Accounting** | evaluate incomplete stamp cards across all teams |
| **External user** (not logged in) | read a shared stamp card and submit stamps for approval, authorized by the share code |

Teams a user may not see are answered with **404**, so that their existence is not revealed.

Coaches are assigned via `coaches` when a team is created and can be added or removed by admins afterwards. Coach emails are compared lower-cased.

## Optimistic concurrency

Every response that refers to a single team contains a `ConcurrencyToken` – the revision of the team stream (`-1` if the stream does not exist yet). In `GET /api/teams` every team carries its own token.

- `PUT /api/teams/:teamId` and `PUT /api/teams/:teamId/player/:playerId` require the token last read in the body (`concurrencyToken`). If it differs from the current revision, the server responds with **409** and the client has to reload.
- All other changes do not need a token. They are still only appended if the stream has not changed between loading and saving, otherwise they fail with 409 as well.
- After every change the response contains the new token, so that the client can continue without reloading.

## API

All responses (except Fastify's default errors) look like this:

```json
{
  "Message": "",
  "HasError": false,
  "Data": { },
  "ConcurrencyToken": 7
}
```

`ConcurrencyToken` is missing in responses that do not refer to exactly one team (team list, accounting, public routes). Request bodies use camelCase, responses PascalCase (compatible with the former .NET backend). Path parameters: `teamId` is a UUID without hyphens, all other ids are UUIDs with hyphens.

### Teams

| Method | Path | Permission | Body | `Data` |
| --- | --- | --- | --- | --- |
| GET | `/api/teams` | authenticated | – | list of `{ Id, Club, Name, Coaches, ConcurrencyToken }` – all teams for admins, otherwise only the user's own |
| POST | `/api/teams` | admin | `{ name, coaches?: string[] }` | team id (201) |
| GET | `/api/teams/:teamId` | team visible | – | `{ Id, Club, Name, Coaches }` |
| PUT | `/api/teams/:teamId` | admin | `{ name, concurrencyToken }` | team id |
| DELETE | `/api/teams/:teamId` | admin | – | team id |
| POST | `/api/teams/:teamId/coach` | admin | `{ email }` | lower-cased email (201); adding an existing coach changes nothing |
| DELETE | `/api/teams/:teamId/coach/:email` | admin | – | lower-cased email; 404 if the email does not coach the team |

The team name must be unique within the club (otherwise 409). A deleted team releases its name again.

### Players

| Method | Path | Body | `Data` |
| --- | --- | --- | --- |
| GET | `/api/teams/:teamId/player` | – | list of `{ Id, FirstName, LastName, Birthdate, Active }` |
| GET | `/api/teams/:teamId/player/:playerId` | – | player |
| POST | `/api/teams/:teamId/player` | `{ firstName, lastName, birthdate }` | player id (201) |
| PUT | `/api/teams/:teamId/player/:playerId` | `{ firstName, lastName, birthdate, concurrencyToken }` | player id |
| DELETE | `/api/teams/:teamId/player/:playerId` | – | player id |
| POST | `/api/teams/:teamId/player/:playerId/enable` | – | player id |
| POST | `/api/teams/:teamId/player/:playerId/disable` | – | player id |

First and last name have at least 2 characters, `birthdate` is an ISO date (`YYYY-MM-DD`), not in the future and at most 120 years ago. The same first and last name (case-insensitive) with the same birthdate counts as a duplicate (409). New players are active. Removing a player removes its stamp cards along with their stamps and share codes; to keep them, disable the player instead.

### Stamp cards and stamps

| Method | Path | Body / query | `Data` |
| --- | --- | --- | --- |
| GET | `/api/teams/:teamId/stampcard` | query `playerId?`, `accountingYear?` | list of `{ Id, PlayerId, AccountingYear, Stamps }` |
| GET | `/api/teams/:teamId/stampcard/:stampCardId` | – | stamp card |
| POST | `/api/teams/:teamId/stampcard` | `{ playerId, accountingYear }` | stamp card id (201) |
| DELETE | `/api/teams/:teamId/stampcard/:stampCardId` | – | stamp card id |
| POST | `/api/teams/:teamId/stampcard/:stampCardId/stamp` | `{ reason }` | stamp id (201) |
| DELETE | `/api/teams/:teamId/stampcard/:stampCardId/stamp/:stampId` | – | stamp id |

There is at most one stamp card per player and accounting year (otherwise 409); the year may be at most 25 years in the past. A stamp looks like `{ Id, Reason, Issuer, IssuedOn, Status }`. Stamps issued by a coach are `accepted` immediately.

### Approving submitted stamps

Stamps submitted by external users have the status `pending` and only count once a coach approves them.

| Method | Path | Body | `Data` |
| --- | --- | --- | --- |
| GET | `/api/teams/:teamId/stamp/pending` | – | list of pending stamps with `StampCardId` and `PlayerId` |
| POST | `/api/teams/:teamId/stamp/pending/approve` | `{ stampIds: string[] }` | approved ids |
| POST | `/api/teams/:teamId/stamp/pending/:stampId/approve` | – | stamp id |
| POST | `/api/teams/:teamId/stampcard/:stampCardId/stamp/:stampId/approve` | – | stamp id |
| POST | `/api/teams/:teamId/stampcard/:stampCardId/stamp/:stampId/reject` | – | stamp id |

The bulk approval is all or nothing: if one of the stamps is unknown or no longer pending, none is approved. A rejected stamp is removed. Approving or rejecting a stamp that is not `pending` results in 409.

### Share codes

A share code (24 random bytes, base64url) gives an external user – e.g. a parent – access to exactly one stamp card.

| Method | Path | `Data` |
| --- | --- | --- |
| GET | `/api/teams/:teamId/stampcard/:stampCardId/share` | list of `{ Id, Code, Revoked }` |
| POST | `/api/teams/:teamId/stampcard/:stampCardId/share` | new share code (201) |
| POST | `/api/teams/:teamId/stampcard/:stampCardId/share/:sharedCodeId/revoke` | share code id |
| DELETE | `/api/teams/:teamId/stampcard/:stampCardId/share/:sharedCodeId` | share code id |

- **Revoke**: the card stays readable with the code, but no more stamps can be submitted with it (403).
- **Delete**: the code becomes invalid, every access with it results in 404.

### Public routes for external users

No JWT, authorized by the header `x-share-code: <code>`. Rate limit: 30 requests per minute and client, so that codes cannot be guessed by trial and error.

| Method | Path | Body | `Data` |
| --- | --- | --- | --- |
| GET | `/api/shared/teams/:teamId/stampcard/:stampCardId` | – | stamp card, stamps **without** `Issuer` |
| POST | `/api/shared/teams/:teamId/stampcard/:stampCardId/stamp` | `{ reason, issuerName }` | stamp id (201), status `pending` |

The `Issuer` is hidden because for accepted stamps it is the coach's email address. `issuerName` (at least 2 characters) is the freely chosen name of the submitting person.

### Accounting

| Method | Path | Permission | Query | `Data` |
| --- | --- | --- | --- | --- |
| GET | `/api/accounting/stampcard/incomplete` | accounting | `accountingYear`, `requiredStamps` (≥ 1) | list of incomplete stamp cards |

Returns the stamp cards of the year across all teams that have fewer than `requiredStamps` **accepted** stamps, each with `TeamId`, `TeamName`, `ConcurrencyToken`, `StampCardId`, `AccountingYear`, `PlayerId`, `FirstName`, `LastName` and `AcceptedStamps`. Cards of players that have been removed are included, with `FirstName` and `LastName` set to `null`. If a team stream cannot be replayed, the whole evaluation fails.

## Events

All events carry `issuer` (email of the user or name of the external user) and `issuedOn`. The schemas are in [stamp-card-events.ts](src/stamp-card-events.ts).

| Area | Events |
| --- | --- |
| Team | `TeamAdded`, `TeamUpdated`, `TeamDeleted`, `CoachAdded`, `CoachRemoved` |
| Player | `PlayerAdded`, `PlayerUpdated`, `PlayerRemoved`, `PlayerEnabled`, `PlayerDisabled` |
| Stamp card | `StampCardAdded`, `StampCardRemoved` |
| Stamp | `StampAdded` (accepted immediately), `StampSubmitted` (pending, with `sharedCodeId`), `StampApproved`, `StampRejected`, `StampRemoved` |
| Share code | `SharedCodeAdded`, `SharedCodeRevoked`, `SharedCodeRemoved` |

**Adding a new event type:** add its schema to `stamp-card-events.ts` and register it in `teamEventSchemas`, handle it in `apply` of the `TeamAggregate` and – if the team list is affected – extend the query of the projection in the event-store-plugin. Stored events are immutable; if a schema changes, old events must remain valid.

## Tests

### Unit tests

```bash
npm test
```

The tests in [test/](test) run with the built-in Node test runner via `tsx` and need neither KurrentDB nor the identity provider. [`InMemoryEventStore`](test/helpers/in-memory-event-store.ts) replaces the KurrentDB client and serializes events to JSON like the real database, so that the same validation is exercised. Shared test data is in [test/helpers/fixtures.ts](test/helpers/fixtures.ts).

`npm run typecheck` additionally checks the types of the tests, which `npm run build` does not cover.

### End-to-end with Bruno

The [Bruno](https://www.usebruno.com/) collection in [bruno/](bruno) covers all endpoints against a running server:

1. Start the server and KurrentDB.
2. Select the environment `Local` and fill in the secret variables (client secret, passwords) as well as the usernames of an admin, a coach and an accounting user. `coachEmail` must match the `email` claim of the coach user. Secrets belong in `bruno/.env`, which is not checked in.
3. Run the collection with the runner. The numbered folders build on each other; ids and the latest `ConcurrencyToken` are passed on as runtime variables.

The folder `12 Cleanup` deletes the test team again.

## Migration from the .NET backend

[scripts/migrate-legacy-streams.ts](scripts/migrate-legacy-streams.ts) converts the team streams of the .NET backend (PascalCase events, e.g. `{ "Club": ..., "Name": ..., "Issuer": ..., "IssuedOn": ... }`) into the event format of this backend:

- Field names become camelCase, the club is taken from `club.name` of the configuration (or `--club`).
- `StampErased` becomes `StampRemoved`; the `Active` flag of `PlayerUpdated` becomes `PlayerEnabled` / `PlayerDisabled` when it changes.
- Every migrated stream is replayed with the `TeamAggregate` before writing. If any of them is invalid, nothing is written.
- The metadata of every event refers to the original (`migratedFrom`, `legacyRevision`, `legacyEventType`, `legacyCreated`).

Dry run, nothing is written:

```bash
npm run migrate:legacy -- --source "kurrentdb://localhost:2113?tls=false"
```

Write to a **different** database:

```bash
npm run migrate:legacy -- --source "<source-url>" --target "<target-url>" --write
```

The target must be a different database, because the legacy streams belong to the category `team` as well and would otherwise show up as broken teams in the projection. Streams that already exist in the target are skipped, so the migration can be run again.
