# Backend Remediation Plan

Prioritized backlog of fixes from the backend review, organized into **epics → user stories**.
Each story lists the findings it covers, acceptance criteria, rough size, and dependencies.
Ordering is security-first, then correctness, then performance, then cleanup.

> Note: `appsettings.json` is gitignored and untracked — DB secrets are **not** committed. No action needed there.

---

## Workflow / Definition of Done

- **One commit per user story.** After a story's acceptance criteria are met and the solution builds, commit before starting the next story.
- Commit messages reference the story id, e.g. `US-3: stop leaking password hashes via UserDto`.
- Work proceeds in the recommended execution order (see bottom of file).

---

## Priority tiers at a glance

| Tier | Theme | Stories | Why now |
|------|-------|---------|---------|
| **P0 – Critical** | Auth & data exposure | US-1, US-2, US-3 | Real security holes; exploitable today |
| **P1 – High** | Correctness & info leaks | US-4, US-5, US-6, US-7 | Wrong behavior / leaks internals |
| **P2 – Medium** | Performance & config | US-8, US-9, US-10 | Scalability & wasted I/O |
| **P3 – Low** | Consistency & hygiene | US-11, US-12, US-13 | Maintainability |

Suggested sequencing: **P0 first (Sprint 1)** → P1 (Sprint 1–2) → P2/P3 (Sprint 2).

---

## EPIC A — Authentication & Authorization (P0)

### US-1 — Protect admin & destructive endpoints
**As** the system owner, **I want** admin and destructive routes to require an authenticated admin, **so that** ordinary users can't promote themselves, block, or delete accounts.

- **Covers findings:** #1 (no auth anywhere)
- **Acceptance criteria:**
  - An unauthenticated/non-admin caller to `AdminController.*`, `DELETE /api/User/{id}`, and `GET /api/User` receives `401/403`.
  - Admin identity is derived from a verified token/session, **not** from a client-supplied `userId`.
  - Quiz ownership checks (`UpdateQuiz`, `DeleteQuiz`, `PublishQuiz`, question edits) use the authenticated identity instead of the `?userId=` query param.
- **Size:** L (design decision required — pick JWT vs. cookie/session)
- **Dependencies:** none, but is the biggest design choice. **Discuss approach before building.**

### US-2 — Enforce blocked-user login
**As** an admin, **I want** blocked users to be denied login, **so that** blocking actually restricts access.

- **Covers findings:** #3
- **Acceptance criteria:**
  - `POST /api/User/login` returns `403` (distinct from bad-credentials `401`) when `IsBlocked` is true.
  - No login-log row is written for a blocked attempt (or it's logged as "blocked").
- **Size:** S
- **Dependencies:** none (independent of US-1).

---

## EPIC B — Sensitive Data Exposure (P0)

### US-3 — Stop leaking password hashes
**As** a user, **I want** my password hash never returned by the API, **so that** it can't be harvested for offline cracking.

- **Covers findings:** #2
- **Acceptance criteria:**
  - Introduce a `UserDto` (no `Password`) — or `[JsonIgnore]` on `User.Password`.
  - `GET /api/User`, `Register`, and any other user-returning endpoint exclude the hash.
  - `Register` returns the newly created `Id`.
  - Response shape verified via Swagger/manual check.
- **Size:** M
- **Dependencies:** none.

---

## EPIC C — Correctness & Error Handling (P1)

### US-4 — Don't leak internal exceptions to clients
**As** the system owner, **I want** API errors to return generic messages, **so that** SQL/schema details aren't disclosed.

- **Covers findings:** #4
- **Acceptance criteria:** `AdminController` (GetDailyLoginCounts, Promote, Demote) no longer returns `ex.Message`; returns a generic 500 string like the rest. Details still logged server-side.
- **Size:** S

### US-5 — Fix copy-paste error messages
**As** a developer, **I want** each AdminController catch block to describe its own action, **so that** logs/responses are accurate.

- **Covers findings:** #5
- **Acceptance criteria:** block/unblock/sharing/stats no longer say "…retrieving wishlist."
- **Size:** XS
- **Note:** Good candidate to bundle with US-4 (same file).

### US-6 — Correct `UpdateShare` result detection
**As** a user, **I want** share updates to report success accurately, **so that** a successful edit isn't reported as "not found."

- **Covers findings:** #6
- **Acceptance criteria:**
  - `UpdateShare` uses the same `ReturnValue` mechanism as `CreateShare`/`DeleteShare`, **or** the SP is confirmed to `SELECT` a row count.
  - Verified against `FP_SP_Shares_Update` behavior.
- **Size:** S
- **Dependencies:** requires checking the stored procedure definition.

### US-7 — Make `MoveToVisited` safe & consistent
**As** a user, **I want** moving a country wishlist→visited to be atomic and error-handled, **so that** a failure doesn't leave it removed-but-not-added.

- **Covers findings:** #7
- **Acceptance criteria:**
  - Endpoint has try/catch like its peers.
  - Remove-from-wishlist + add-to-visited run in a single transaction (or a single SP).
- **Size:** M

---

## EPIC D — Performance & Configuration (P2)

### US-8 — Load configuration once
**As** the system, **I want** the connection string read once, **so that** we stop re-reading `appsettings.json` from disk on every query.

- **Covers findings:** #8
- **Acceptance criteria:** `Connect()` no longer builds a `ConfigurationBuilder` per call; config cached (static) or injected. Behavior unchanged.
- **Size:** S

### US-9 — Clean up CORS & startup
**As** a developer, **I want** a single, correct CORS registration, **so that** startup is clean and policy is intentional.

- **Covers findings:** #9
- **Acceptance criteria:** duplicate `UseCors` line removed; named policy defined; (optional) origin list tightened if deployment target is known.
- **Size:** XS

### US-10 — Reduce N+1 in wishlist/visited loads
**As** a user with many saved countries, **I want** the list to load efficiently, **so that** it doesn't fire 5 queries per country.

- **Covers findings:** #10
- **Acceptance criteria:** `getWishlist`/`getVisited` use an aggregated pattern like `ReadAllCountries`, or batch child loads. Result equivalence verified.
- **Size:** M (may need SP work)
- **Dependencies:** possibly new/updated stored procedures.

---

## EPIC E — Consistency & Hygiene (P3)

### US-11 — Adopt `using` for DB resources
**As** a developer, **I want** connections/commands wrapped in `using`, **so that** they're disposed deterministically.

- **Covers findings:** Low — manual `finally { con.Close() }` instead of `using`
- **Acceptance criteria:** DAL methods use `using` for `SqlConnection`/`SqlCommand`; the canonical DAL method skeleton is updated to reflect the standard. **Do one method as a reference, get sign-off, then roll out.**
- **Size:** L (touches every DAL method — mechanical but broad)

### US-12 — Input validation on Register
**As** a user, **I want** clear validation errors, **so that** malformed registrations fail fast with 400s instead of relying on the SP.

- **Covers findings:** Low — no input validation
- **Acceptance criteria:** email format + non-empty username/password validated (DataAnnotations or explicit checks); returns `400` with field messages.
- **Size:** S

### US-13 — Remove unused `catch` vars & dead inconsistencies
**As** a developer, **I want** warning-free, consistent catch blocks, **so that** the code is clean.

- **Covers findings:** Low — unused `ex`; `Language.GetAll` vs `ReadAllLanguages` confirmation
- **Acceptance criteria:** no unused `ex` warnings; confirm `Language.GetAll` and `ReadAllLanguages` target the intended SP.
- **Size:** XS

---

## Recommended execution order

1. **Sprint 1 (security):** US-3 → US-2 → US-4 + US-5 (bundle) → US-9 → US-8. Mostly small, independent quick wins that close real holes.
2. **US-1 (auth):** parallel design discussion, then build — largest story; shapes ownership checks.
3. **Sprint 2 (correctness/perf):** US-6 → US-7 → US-10 → US-12.
4. **Sprint 2 / backlog (hygiene):** US-11 (staged rollout) → US-13.

**Dependencies to note:** US-6 and US-10 may require inspecting/changing stored procedures; US-1 needs an auth-strategy decision before implementation.

---

## Findings → Story traceability

| # | Finding | Story |
|---|---------|-------|
| 1 | No authentication/authorization anywhere | US-1 |
| 2 | Password hashes leaked (`GET /api/User`, `Register`) | US-3 |
| 3 | Blocked users can still log in | US-2 |
| 4 | Internal `ex.Message` leaked to clients | US-4 |
| 5 | Copy-paste error messages in AdminController | US-5 |
| 6 | `UpdateShare` uses `ExecuteScalar` inconsistently | US-6 |
| 7 | `MoveToVisited` no try/catch, non-atomic | US-7 |
| 8 | Configuration rebuilt on every DB call | US-8 |
| 9 | Duplicate CORS registration; wide-open policy | US-9 |
| 10 | N+1 queries in `getWishlist`/`getVisited` | US-10 |
| L | DB resources not wrapped in `using` | US-11 |
| L | No input validation on Register | US-12 |
| L | Unused `catch` vars; `Language.GetAll` check | US-13 |
