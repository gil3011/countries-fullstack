# 👤 User Profile Page — Feature Plan

The **My Profile** page lets a logged-in user manage their account and travel data:
change password, edit preferences (continents + languages, as in registration), and
view/manage their **Visited** and **Wishlist** countries on an interactive map.

Branch: `user-profile-page` (from `develop`).

---

## 1. Architecture fit

- **Frontend**: static jQuery pages in `Client/Pages`, scripts in `Client/JS`, styles in `Client/CSS`.
  All XHR go through the global `ajaxCall(method, url, JSON.stringify(data), successCB, errorCB)`
  in `Client/JS/ajaxCalls.js`; routes via `API_ROUTES` in `Client/JS/apiRoutes.js`.
- **Auth state**: `localStorage.loggedInUser` = `{ id, username, email, isAdmin, isAllowedToShare }`.
- **Map**: reuse **Leaflet 1.9.4** (already loaded and used in `Client/JS/indexScript.js`).
  `Country` objects already carry `id`, `cca3`, `commonName`, `latitude`, `longitude`, `flagUrl`.
- **Backend**: `UserController` → `BL.User` → `DBServiceUser` → SQL stored procedures.

---

## 2. Backend gap analysis

### Already exposed & usable as-is
| Purpose | Endpoint |
| :--- | :--- |
| Get visited (full `Country` list w/ lat/lng) | `GET /api/User/{userId}/visited` |
| Get wishlist | `GET /api/User/{userId}/wishlist` |
| Remove from visited | `DELETE /api/User/{userId}/visited/{countryId}` |
| Remove from wishlist | `DELETE /api/User/{userId}/wishlist/{countryId}` |
| Move wishlist → visited | `POST /api/User/{userId}/moveToVisited/{countryId}` |
| Get continent preferences | `GET /api/User/getContinentPreferences/{userId}` |
| Get languages | `GET /api/User/getUserLanguages/{userId}` |

### Gaps — BL/DAL methods exist but no controller endpoint (add thin wrappers, **no new SPs**)
- `AddContinentPreference` / `RemoveContinentPreference`
- `AddLanguageToUser` / `RemoveLanguageFromUser`

The underlying SPs (`FP_SP_Add_User_Continent`, `FP_SP_Delete_User_Continent`,
`FP_SP_Add_Language_To_User`, `FP_SP_Remove_Language_From_User`) already exist in the DB
(used by registration).

### Gap — needs new code: Change Password
`PUT /api/User/UpdateUser` takes a **whole `User` object and always re-hashes the password**, and
`IsBlocked` is not in localStorage (would default to `false` — a blocked user could unblock
themselves). So change-password gets a **dedicated endpoint** that verifies the current password and
updates only the hash.

---

## 3. User Stories

**Epic:** As a logged-in user, I want a profile page to manage my account, preferences, and travel lists.

- **US-1 — Change password.** Enter current + new + confirm. *Accept:* current verified server-side;
  new validated (≥8 chars, 1 uppercase, 1 number — same regex as `login.js`); confirmation must match;
  wrong current → clear error; success → confirmation message.
- **US-2 — Edit continent preferences.** Checkbox grid like registration; changes staged locally and
  persisted on **Save**. *Accept:* current prefs pre-checked; Save diffs and persists; failure reverts.
- **US-3 — Edit spoken languages.** Language + level select + Add, list with Remove; staged locally,
  persisted on **Save**. *Accept:* duplicates blocked; changes persist on Save.
- **US-4 — See my countries on a map.** One Leaflet map: **🟢 green = visited, 🟠 orange = wishlist**,
  with a legend. Pin popup shows flag + name + list actions. *Accept:* both lists plotted with distinct
  colors; empty state handled.
- **US-5 — Manage visited.** Remove from visited (map popup + list). *Accept:* updates map + list without
  full reload; confirm before removing.
- **US-6 — Manage wishlist.** Remove **or** move to visited. *Accept:* move turns pin green and updates
  both lists; remove drops it.

---

## 4. Page layout & design

Single page `Client/Pages/userHome.html`, reusing the navbar and site CSS variables.

```
┌───────────────────────────── Navbar ─────────────────────────────┐
│  My Profile — Welcome, {username}                                 │
├─────────────────────────── Travel Map ────────────────────────────┤
│   [ Leaflet map, full width ]   Legend: 🟢 Visited  🟠 Wishlist   │
│   Two columns below the map:                                      │
│     Visited (N)              |     Wishlist (N)                   │
│     • Flag Name  [Remove]    |     • Flag Name [Move✓][Remove]    │
├──────────── Preferences ────────────┬──────── Account ────────────┤
│  Continents (checkbox grid)         │  Change Password            │
│  Languages (select+level+Add, list) │   current / new / confirm   │
│  [ Save Preferences ]               │   [ Change Password ]       │
└─────────────────────────────────────┴─────────────────────────────┘
```

- **Pins**: Leaflet `L.divIcon` with CSS colors — green `#2e7d32` (visited), orange `#ef6c00`
  (wishlist). Small legend chip. Popups reuse the flag + name pattern from `indexScript.js`.
- **Auth guard**: redirect to `login.html` if `localStorage.loggedInUser` is null.
- **Preferences save = batch:** snapshot original on load; user edits local state freely; on **Save**,
  diff original vs. current and fire the minimal add/remove calls; re-snapshot on success.

---

## 5. API / DB call map

| Feature | Frontend call | Endpoint | Backend |
| :--- | :--- | :--- | :--- |
| Load map/lists | GET visited + GET wishlist | existing | existing |
| Remove visited | `DELETE …/{id}/visited/{cid}` | existing | existing |
| Remove wishlist | `DELETE …/{id}/wishlist/{cid}` | existing | existing |
| Move to visited | `POST …/{id}/moveToVisited/{cid}` | existing | existing |
| Load prefs | `GET …/getContinentPreferences/{id}` | existing | existing |
| Add/Remove continent | `POST`/`DELETE …/{id}/continent/{name}` | **new** | existing BL/SP |
| Load languages | `GET …/getUserLanguages/{id}` | existing | existing |
| Add language | `POST …/{id}/language` body `{language, level}` | **new** | existing BL/SP |
| Remove language | `DELETE …/{id}/language/{language}` | **new** | existing BL/SP |
| Change password | `POST …/changePassword` body `{userId, currentPassword, newPassword}` | **new** | new BL + SP |

---

## 6. Backend changes

1. **`UserController` — 5 new actions** (mirroring the wishlist/visited style):
   - `POST {userId}/continent/{continentName}` → `BL.User.AddContinentPreference`
   - `DELETE {userId}/continent/{continentName}` → `BL.User.RemoveContinentPreference`
   - `POST {userId}/language` (body `{language, level}`) → `BL.User.AddLanguageToUser`
   - `DELETE {userId}/language/{language}` → `BL.User.RemoveLanguageFromUser`
   - `POST changePassword` (body `{userId, currentPassword, newPassword}`) → new `BL.User.ChangePassword`
2. **Change password (new):**
   - `BL.User.ChangePassword(userId, current, new)`: look up user (by the email in localStorage via
     existing `GetUserByEmail`, or add `GetUserById`), verify current with
     `PasswordHasher.VerifyHashedPassword`, hash the new password, call a password-only DAL update.
   - `DBServiceUser.UpdatePassword(userId, hash)` → new SP `FP_sp_Users_UpdatePassword(@Id, @Password)`.
3. **No new SPs for preferences/languages** — they already exist. Backfill their `CREATE` scripts into
   `SQL/SP's User.sql` for repo completeness.

---

## 7. File plan

- `Client/Pages/userHome.html` *(new)* — Leaflet CSS/JS + jQuery + `ajaxCalls.js`, `apiRoutes.js`,
  `navbar.js`, `userHome.js`
- `Client/JS/userHome.js` *(new)*
- `Client/CSS/userHomeStyle.css` *(new)*
- `Client/Pages/navbar.html` — add "My Profile" link (and fix the broken `../HTML/` paths → `../Pages/`)
- `Server/Server/Controller/UserController.cs` — 5 new actions
- `Server/Server/BL/User.cs` + `Server/Server/DAL/DBServiceUser.cs` — `ChangePassword` / `UpdatePassword`
- `SQL/SP's User.sql` — add `FP_sp_Users_UpdatePassword` + backfill preference/language SP scripts
- Delete the untracked `Server/Client/` scaffold *(done)*

---

## 8. Build order

1. Backend endpoints (preferences, languages, change-password) + SP — testable in Swagger.
2. `userHome.html` shell + navbar link + auth guard.
3. Map (US-4) with two pin colors + legend, wired to visited/wishlist GETs.
4. List actions (US-5, US-6) with optimistic UI updates.
5. Preferences panel with batch Save (US-2, US-3).
6. Change password (US-1).

---

## 9. Locked decisions

- **Change password:** dedicated endpoint that **verifies the current password** and updates only the
  hash (new password-only SP).
- **Preferences saving:** **batch on a Save button** (diff original vs. current, fire minimal calls).
- **File location:** build in the real `Client/` tree; the `Server/Client/` scaffold is discarded.
