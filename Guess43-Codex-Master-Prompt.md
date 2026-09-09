# Guess43 — Codex Master Implementation Prompt

You are the principal engineer responsible for designing, implementing, testing, documenting, and preparing for deployment an interview-grade full-stack application named **Guess43**. This is a hiring assessment with a two-day deadline, so deliver a polished, defensible solution without unnecessary enterprise complexity.

## Original assignment

Build a full-stack CRUD application that:

- Uses React and PostgreSQL.
- Supports registration, login, and logout.
- Stores user credentials securely in PostgreSQL.
- Lets an authenticated user play Guess the Number:
  - Generate a random integer from 1 through 43, inclusive.
  - Let the user submit guesses.
  - Respond with higher, lower, or correct.
  - Persist the user's lowest number of guesses as one nullable field.
  - Show that personal best whenever the user logs in again.
- Includes one thoughtful bonus feature.
- Is stored in GitHub.
- Is deployed to AWS or another cloud platform.

## Your operating instructions

Own the task end to end. First inspect the current working directory, Git state, installed SDKs, available credentials, and any existing files. Preserve legitimate existing work. Do not overwrite or delete unrelated changes.

If the repository is empty, initialize the solution described below. If it already contains code, compare it with this prompt, document the gaps, and adapt the plan without duplicating functioning components.

Before implementation:

1. Write `docs/architecture.md` containing the validated architecture, boundaries, main flows, ER diagram, security decisions, and trade-offs.
2. Write `docs/implementation-plan.md` with small, ordered, testable tasks and explicit acceptance criteria.
3. Self-review both documents for contradictions, placeholders, missing requirements, and over-engineering.
4. Then begin implementation immediately. Do not pause for approval unless a genuinely blocking choice, unavailable credential, destructive action, or external permission is required.

Use test-driven development for domain rules, application use cases, and bug fixes: write a failing meaningful test, observe the expected failure, implement the minimum correct behavior, and rerun the focused and relevant broader suites.

Make small conventional commits after independently verified slices. Never claim that a test, build, Sonar Quality Gate, deployment, push, or URL works without fresh evidence from the relevant command or service.

## Technology baseline

- Backend: ASP.NET Core 8 Web API, C#, nullable reference types enabled.
- Frontend: React, TypeScript, and Vite.
- Database: PostgreSQL with EF Core and Npgsql.
- API documentation: OpenAPI/Swagger.
- Backend tests: xUnit, FluentAssertions, and Testcontainers for PostgreSQL integration tests.
- Frontend tests: Vitest, React Testing Library, and MSW.
- End-to-end smoke tests: Playwright.
- Validation: FluentValidation.
- Logging: `ILogger<T>` abstractions with Serilog as provider.
- Quality: SonarQube or SonarCloud, ESLint, Prettier, TypeScript strict mode, build warnings treated as errors.
- Delivery: Docker, Docker Compose, GitHub Actions, and a reproducible Render deployment blueprint. AWS may be used instead only if working account access makes it safer within the deadline.

Use stable compatible package versions resolved at implementation time. Commit lockfiles and avoid unnecessary dependencies.

## Required architecture

Implement a pragmatic Clean Architecture. The backend projects are:

```text
src/backend/
├── Guess43.Domain
├── Guess43.Application
├── Guess43.Infrastructure
└── Guess43.Api
```

Tests are:

```text
tests/
├── Guess43.Domain.Tests
├── Guess43.Application.Tests
├── Guess43.ArchitectureTests
└── Guess43.IntegrationTests
```

The frontend lives in `src/frontend/guess43-web` and uses feature-based organization:

```text
src/
├── app
├── features/auth
├── features/game
├── features/history
├── features/dashboard
├── features/profile
└── shared
```

Dependency rules:

- Domain depends on no other project and contains pure business behavior.
- Application depends only on Domain and defines use cases and ports.
- Infrastructure implements Application ports using EF Core, PostgreSQL, hashing, tokens, and time.
- API is the composition root and contains thin HTTP controllers, authentication configuration, and error mapping.
- Frontend consumes HTTP contracts and contains no duplicated server business rules.
- Enforce backend dependency rules using architecture tests.

Do not add MediatR, a generic `IRepository<TEntity>`, AutoMapper, an event bus, microservices, Redis, or Kubernetes. They do not add sufficient value to this assessment.

## Repository and Unit of Work design

Use aggregate-specific repositories rather than a generic repository:

- `IUserRepository`
- `IGameSessionRepository`
- `IRefreshTokenRepository`
- `IUnitOfWork`

Place the interfaces under Application abstractions and EF implementations under Infrastructure. Repositories expose business-relevant operations, never `IQueryable`, EF entities, or provider details. Repository methods stage changes; they must not call `SaveChanges` independently.

`IUnitOfWork.SaveChangesAsync` defines the normal use-case transaction boundary. Provide an explicit transaction operation only where multiple persistence steps truly require it. Completing a game and updating the personal best must be atomic. Do not create nested or per-repository transactions.

## Domain model and persistence

### User aggregate

Persist:

- `Id`
- `Email`
- `NormalizedEmail`
- `DisplayName`
- `PasswordHash`
- `BestGuessCount` as a nullable integer; this is the assignment's one persisted personal-best field.
- `CreatedAtUtc`
- `UpdatedAtUtc`

Enforce a unique case-insensitive normalized-email index. Encapsulate personal-best updates so the value changes only after a completed game with a smaller positive guess count.

### GameSession aggregate

Persist:

- `Id`
- `UserId`
- `TargetNumber`
- `GuessCount`
- `Status` (`Active` or `Completed`)
- `StartedAtUtc`
- `CompletedAtUtc`
- a concurrency token/version

Generate the secret server-side using `RandomNumberGenerator.GetInt32(1, 44)`. Never use client randomness, `new Random()` for the secret, or expose `TargetNumber` through any DTO, log, error, Swagger example, or frontend state.

The aggregate method that submits a guess must:

- Accept only integers from 1 through 43.
- Reject guesses after completion.
- Increment the counter exactly once per accepted guess.
- Return `Higher`, `Lower`, or `Correct`.
- Mark the session complete and set its completion time when correct.

Allow at most one active session per user. Enforce this both in application behavior and with a PostgreSQL constraint/partial unique index so concurrent starts are safe. A repeated start while a session is active should return the existing active session rather than silently creating another. Handle optimistic concurrency deterministically.

### RefreshToken entity

Persist only a cryptographic hash of each refresh token with user id, expiry, creation, revocation, and replacement metadata required for rotation and reuse detection. Never store or log the plaintext refresh token.

Configure relationships, indexes, lengths, required fields, cascade behavior, and concurrency explicitly using EF Core configuration classes. Generate and commit an initial migration. Verify migration against a clean PostgreSQL database.

## Authentication and account CRUD

Implement:

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`
- `GET /api/users/me`
- `PUT /api/users/me`
- `DELETE /api/users/me`

Requirements:

- Hash passwords using ASP.NET Core's proven `PasswordHasher<TUser>` or an equivalent framework implementation. Never implement cryptography manually.
- Enforce a reasonable password policy and validate email/display name.
- Return the same generic authentication failure for an unknown email and a wrong password.
- Use a short-lived JWT access token and rotating refresh tokens.
- Return the access token to the SPA and keep it in memory, not local storage.
- Put the refresh token in a `Secure`, `HttpOnly`, appropriately `SameSite` cookie.
- Revoke the current refresh-token family on logout and clear the cookie.
- Rate-limit registration, login, refresh, and guess submission.
- Never accept a user id from the client for an authenticated resource; derive it from validated claims.
- `GET /api/users/me` returns the user profile and nullable personal best so it is displayed immediately after login or session restoration.
- Account deletion revokes sessions and removes dependent game data in a documented transaction.

In production, serve the compiled React SPA and API from the same ASP.NET Core origin. Build React in the production Dockerfile and copy the output into the API's static files. Keep separate Vite and API development servers locally with narrowly configured development CORS. This avoids weakening refresh-cookie security for cross-site deployment.

## Game and CRUD API

Implement these authorized endpoints:

- `POST /api/games` — start a game or return the existing active game.
- `GET /api/games/active` — restore the current active game.
- `POST /api/games/{gameId}/guesses` — submit a guess and return result, accepted guess count, completion state, and current personal best.
- `GET /api/games?page=1&pageSize=10` — paginated history belonging to the current user.
- `GET /api/games/{gameId}` — retrieve an owned game summary.
- `DELETE /api/games/{gameId}` — delete an owned completed game; do not erase the user's persisted personal best.

This must demonstrate real CRUD through account/profile and game-history flows. Choose status codes consistently and document them in OpenAPI. Apply a maximum page size. Return `404`, not data leakage, when a resource is absent or belongs to another user.

## Error model

Use a typed `Result<T>` plus an `Error` containing stable code, description, and category for expected failures. Do not use exceptions for normal validation, not-found, conflict, or unauthorized business outcomes.

Map expected errors centrally to RFC-compatible `ProblemDetails`, including a stable `errorCode` and `traceId`. Required codes include at least:

- `VALIDATION_ERROR`
- `EMAIL_ALREADY_EXISTS`
- `INVALID_CREDENTIALS`
- `GAME_NOT_FOUND`
- `GAME_ALREADY_COMPLETED`
- `GUESS_OUT_OF_RANGE`
- `CONCURRENCY_CONFLICT`

Use ASP.NET Core 8 `IExceptionHandler` and `ProblemDetails` for unexpected exceptions. Log the full internal exception once, return a sanitized 500 response, and never expose stack traces, SQL, connection details, or secrets. Keep controller actions thin and keep mapping behavior covered by tests.

## Logging and observability

Use structured logging through `ILogger<T>` and Serilog:

- API: request method/path template, status, duration, trace/correlation id.
- Application: meaningful events such as account creation, game start/completion, and new personal best.
- Infrastructure: actionable database or token-operation failures.
- Domain: no logging dependency.

Never log passwords, password hashes, tokens, cookies, authorization headers, game target numbers, or unnecessary personal data. Avoid logging the same exception in multiple layers. Add correlation-id propagation and JSON console logs in production.

Add `/health/live` and `/health/ready`; readiness must verify PostgreSQL connectivity without revealing sensitive details.

## Frontend requirements

Create a polished, responsive, accessible UI with:

- Registration and login pages with client validation consistent with server contracts.
- Protected and guest-only routes.
- Dashboard showing display name, personal best, statistics, recent games, and primary play action.
- Game screen with range guidance, guess count, higher/lower feedback, loading protection, and keyboard-friendly input.
- History page with pagination, detail view, and confirmed deletion of completed records.
- Profile page supporting update and confirmed account deletion.
- Clear loading, empty, error, offline, and success states.
- Visible logout.
- Accessible labels, focus behavior, contrast, responsive layout, and reduced-motion support.

Use a centralized typed API client. Implement access-token refresh once per failed request, prevent refresh loops and refresh stampedes, and retry the original request only once. Use stable server `errorCode` values rather than matching human-readable messages. Add a React error boundary for unexpected rendering failures.

Do not create a generic admin template or excessive visual effects. The interface should look intentional and interview-ready.

## Bonus feature

After all required behavior is complete and verified, implement a lightweight **Performance Dashboard and Achievements** bonus:

- Completed game count.
- Best score.
- Average accepted guesses.
- Recent performance.
- Computed achievements: `First Win`, `Sharp Shooter` for a win in six guesses or fewer, and `Personal Best` when a record is broken.
- A tasteful confetti effect for winning or setting a new personal best, respecting reduced-motion preferences.

Compute statistics efficiently from owned records. Avoid adding a separate achievements table unless persistence has a demonstrated requirement. The bonus must never delay or destabilize the core assignment.

## Required tests

Write meaningful tests that cover at least:

### Domain

- Minimum and maximum accepted guesses.
- Out-of-range guesses do not increment the counter.
- Higher, lower, and correct outcomes.
- Each accepted guess increments exactly once.
- Completed games reject further guesses.
- Personal best initializes, improves, and never worsens.

### Application

- Register rejects normalized duplicate email and saves once on success.
- Login has indistinguishable invalid-credential behavior.
- Logout revokes the correct token family.
- Start game is idempotent when an active session exists.
- Guess completion and personal-best update share one commit/transaction.
- Owned-resource access and history deletion rules.

### Integration with real PostgreSQL through Testcontainers

- Initial migration applies to an empty database.
- Register, login, refresh rotation, logout, and post-logout refresh rejection.
- Duplicate email constraint under concurrency.
- Start, guess, complete, persist, log in again, and retrieve personal best.
- Cross-user read/delete attempts do not leak data.
- Target number never appears in serialized responses.
- Active-game database constraint and optimistic concurrency behavior.
- ProblemDetails shape and stable error codes.

### Frontend

- Registration and login validation and server-error display.
- Protected-route behavior.
- Higher/lower/correct game states.
- Personal-best and history rendering.
- Central API error and refresh behavior.

### Playwright smoke journeys

- Register → login/session → play → win → see personal best.
- Logout → protected page redirects to login.

Tests must be deterministic. Use an injectable random-number generator and clock in tests while production implementations remain secure and UTC-based.

## Code-quality rules

- Apply SOLID where it produces real boundaries; do not create interfaces for pure data or one-off helpers without a seam.
- Prefer small cohesive classes and intention-revealing names.
- Keep domain invariants inside domain objects.
- Use constants/value objects for game limits and avoid magic numbers.
- Use asynchronous I/O and propagate `CancellationToken`.
- Enable analyzers, nullable reference types, and warnings as errors.
- Do not expose EF Core types outside Infrastructure.
- Do not return persistence entities directly from the API.
- Avoid static mutable state, service locators, hidden dependencies, and catch-all exception swallowing.
- Remove dead code, commented-out code, debug output, placeholders, and unresolved TODOs.
- Format C# and TypeScript consistently.

## Sonar quality requirements

Configure backend and frontend analysis. The target before completion is:

- zero blocker or critical issues;
- zero vulnerabilities;
- security hotspots reviewed;
- at least 80% coverage of testable code, with strong coverage of Domain and Application rules;
- less than 3% duplication;
- no unexplained new-code smells;
- generated files, migrations, and third-party assets excluded from coverage only where justified.

If Sonar credentials or an organization/project key are unavailable, prepare complete configuration and CI secret names, run every available local analyzer and coverage command, and report the external Quality Gate as blocked—not passed.

## CI/CD and containers

Provide:

- A multi-stage production Dockerfile that builds React, publishes ASP.NET Core, and serves both from one same-origin container.
- `docker-compose.yml` for the app and PostgreSQL with health checks.
- `.env.example` containing names and safe examples only.
- `.dockerignore` and `.gitignore`.
- A GitHub Actions workflow that performs locked dependency restore, formatting checks, backend build, unit/architecture/integration tests, frontend lint/type-check/test/build, coverage publication, Sonar analysis, and Docker build.
- Dependency caching without caching secrets or build outputs incorrectly.

Use `dotnet restore --locked-mode` after generating and committing a NuGet lock file where practical, and `npm ci` with the committed npm lockfile.

## Deployment

Prepare a reproducible Render blueprint using one Docker web service and managed PostgreSQL. Configure environment variables, same-origin SPA fallback, HTTPS-aware forwarded headers, health checks, production logging, and safe database migration execution. Do not embed credentials in `render.yaml`.

Deployment is an external action. If authenticated deployment access is already configured and the environment authorizes it, deploy and verify the live UI, API, health endpoint, registration, and a complete game flow. Otherwise, finish all deployment artifacts and exact instructions, then report precisely which credential or dashboard action is required. Never invent a live URL.

## Documentation

Create an excellent root `README.md` containing:

- project purpose and assignment coverage;
- live application and Swagger links only when verified;
- screenshots or a short demo GIF if feasible;
- feature list and bonus feature;
- architecture and dependency diagram;
- ER diagram;
- explanation of Clean Architecture, focused repositories, Unit of Work, Result/ProblemDetails, and security decisions;
- prerequisites and one-command Docker Compose startup;
- local development instructions;
- migrations, tests, coverage, formatting, Sonar, and build commands;
- API endpoint table;
- environment-variable reference with no secrets;
- assumptions, trade-offs, known limitations, and sensible next steps.

Add an OpenAPI file or generated contract artifact only if it stays synchronized with the actual API.

## Git and delivery discipline

Use conventional commits grouped by independently working slices, for example:

1. `docs: add Guess43 architecture and implementation plan`
2. `chore: scaffold clean architecture and quality tooling`
3. `feat(auth): add secure account lifecycle`
4. `feat(game): add server-authoritative guessing flow`
5. `feat(history): add profile and game CRUD`
6. `feat(ui): add responsive authenticated game experience`
7. `test: add integration and end-to-end coverage`
8. `feat(dashboard): add performance achievements`
9. `ci: add quality gate and container pipeline`
10. `docs: finalize deployment and assessment guide`

Use the repository's existing branch convention if present; otherwise create a descriptive feature branch. Do not rewrite shared history. Before pushing, scan tracked files and Git history created during this task for secrets. If GitHub authentication and the target repository are configured, push and open a pull request with a concise summary, test evidence, screenshots, deployment link, and Sonar result. If not configured, stop at a clean local branch and report the exact missing setup rather than requesting or exposing raw tokens.

## Two-day priority order

Protect delivery using these gates:

### Gate 1 — mandatory functional core

- Clean solution builds.
- PostgreSQL migration works.
- Register/login/refresh/logout work securely.
- Server-authoritative game works.
- Personal best persists and appears after login.

### Gate 2 — CRUD and production behavior

- Profile read/update/delete.
- Game history read/detail/delete.
- Error model, logging, health checks, ownership, concurrency.
- Responsive core UI.

### Gate 3 — verification and presentation

- Unit, integration, architecture, frontend, and smoke tests.
- Sonar configuration and quality remediation.
- Docker, CI, README, diagrams, deployment.

### Gate 4 — bonus polish

- Performance statistics, achievements, and tasteful animation.

Never sacrifice a verified mandatory requirement for bonus polish.

## Final verification checklist

Before declaring completion, run and capture fresh evidence for:

- clean backend restore and build with warnings as errors;
- all backend test projects;
- frontend install, formatting, lint, strict type-check, tests, and production build;
- coverage generation;
- Docker image build;
- Docker Compose startup against a clean database;
- migration application;
- health checks;
- core manual or automated user journey;
- secret scan;
- Sonar Quality Gate if credentials exist;
- deployed URLs if deployment access exists.

Inspect `git status` and the final diff. Ensure documentation matches the actual implementation. Report:

1. What was built.
2. Architecture and important trade-offs.
3. Exact verification commands and their results.
4. Test and coverage totals.
5. Sonar outcome, distinguishing passed from unavailable.
6. Local run instructions.
7. Git branch, commits, remote, and PR status.
8. Deployment status and verified URLs.
9. Any remaining blocker or limitation.

Start now by inspecting the workspace and writing the architecture and implementation plan. Then execute the plan in priority order.
