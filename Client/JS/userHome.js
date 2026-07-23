/* ===================== My Profile page ===================== */

// --- Auth guard ---
const loggedInUser = JSON.parse(localStorage.getItem("loggedInUser"));
if (!loggedInUser || !loggedInUser.id) {
    window.location.href = "login.html";
}
const userId = loggedInUser ? loggedInUser.id : null;

// --- State ---
let map;
let markers = [];
let visited = [];
let wishlist = [];

// Preference snapshots (what the server currently has), staged locally until "Save".
let originalContinents = [];   // string[]
let originalLanguages = {};    // { language: level }
let stagedLanguages = {};      // { language: level } (edited locally)

const LEVELS = ["Beginner", "Intermediate", "Advanced"];

// --- Promise wrapper around the project's global ajaxCall ---
function apiRequest(method, url, data) {
    return new Promise((resolve, reject) => {
        ajaxCall(method, url, data ? JSON.stringify(data) : null, resolve, reject);
    });
}

// --- Status helper ---
function setStatus(msg, isError) {
    const el = document.getElementById("uh-status");
    el.textContent = msg || "";
    el.style.color = isError ? "var(--danger)" : "var(--accent)";
    if (msg) {
        setTimeout(() => { if (el.textContent === msg) el.textContent = ""; }, 3500);
    }
}

/* ===================== Map ===================== */
function initMap() {
    map = L.map("map").setView([20, 0], 2);
    L.tileLayer("https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png").addTo(map);
}

function pinIcon(color) {
    return L.divIcon({
        className: "uh-pin",
        html: `<span class="uh-pin-dot" style="background:${color}"></span>`,
        iconSize: [16, 16],
        iconAnchor: [8, 8],
        popupAnchor: [0, -8]
    });
}

function popupHtml(country, listType) {
    const name = country.commonName || "Unknown";
    const cca3 = country.cca3 || "";
    const flag = country.flagUrl ? `<img src="${country.flagUrl}" alt="${name} flag">` : "";
    const header = cca3
        ? `<a class="uh-popup-link" href="country.html?cca3=${encodeURIComponent(cca3)}">${flag}<b>${name}</b></a>`
        : `${flag}<b>${name}</b>`;
    let buttons;
    if (listType === "visited") {
        buttons = `<button class="uh-btn uh-btn-danger uh-btn-small" onclick="removeVisited(${country.id})">Remove</button>`;
    } else {
        buttons =
            `<button class="uh-btn uh-btn-success uh-btn-small" onclick="moveToVisited(${country.id})">Move to visited</button>` +
            `<button class="uh-btn uh-btn-danger uh-btn-small" onclick="removeWishlist(${country.id})">Remove</button>`;
    }
    return `<div class="uh-popup">${header}<div class="uh-row-actions">${buttons}</div></div>`;
}

function renderMap() {
    markers.forEach(m => map.removeLayer(m));
    markers = [];

    const plot = (list, color, listType) => {
        list.forEach(c => {
            const lat = c.latitude || 0;
            const lng = c.longitude || 0;
            if (lat === 0 && lng === 0) return;
            const m = L.marker([lat, lng], { icon: pinIcon(color) })
                .addTo(map)
                .bindPopup(popupHtml(c, listType));
            markers.push(m);
        });
    };

    plot(visited, getCss("--visited"), "visited");
    plot(wishlist, getCss("--wishlist"), "wishlist");
}

function getCss(varName) {
    return getComputedStyle(document.documentElement).getPropertyValue(varName).trim() || "#60a5fa";
}

function goToCountry(cca3) {
    if (!cca3) return;
    window.location.href = `country.html?cca3=${encodeURIComponent(cca3)}`;
}

/* ===================== Country lists ===================== */
function renderLists() {
    document.getElementById("visited-count").textContent = visited.length;
    document.getElementById("wishlist-count").textContent = wishlist.length;

    renderCountryList("visited-list", visited, "visited");
    renderCountryList("wishlist-list", wishlist, "wishlist");
}

function renderCountryList(containerId, list, listType) {
    const ul = document.getElementById(containerId);
    ul.innerHTML = "";

    if (!list.length) {
        const li = document.createElement("li");
        li.className = "uh-empty";
        li.style.border = "none";
        li.style.background = "none";
        li.textContent = listType === "visited"
            ? "No countries visited yet."
            : "Your wishlist is empty.";
        ul.appendChild(li);
        return;
    }

    list.forEach(c => {
        const li = document.createElement("li");

        const nameDiv = document.createElement("div");
        nameDiv.className = "uh-country-name";
        const flag = c.flagUrl ? `<img src="${c.flagUrl}" alt="">` : "";
        nameDiv.innerHTML = `${flag}<span>${c.commonName || "Unknown"}</span>`;
        if (c.cca3) {
            nameDiv.classList.add("clickable");
            nameDiv.title = "View country details";
            nameDiv.addEventListener("click", () => goToCountry(c.cca3));
        }

        const actions = document.createElement("div");
        actions.className = "uh-row-actions";

        if (listType === "wishlist") {
            const moveBtn = document.createElement("button");
            moveBtn.className = "uh-btn uh-btn-success uh-btn-small";
            moveBtn.textContent = "Move to visited";
            moveBtn.addEventListener("click", () => moveToVisited(c.id));
            actions.appendChild(moveBtn);
        }

        const removeBtn = document.createElement("button");
        removeBtn.className = "uh-btn uh-btn-danger uh-btn-small";
        removeBtn.textContent = "Remove";
        removeBtn.addEventListener("click", () =>
            listType === "visited" ? removeVisited(c.id) : removeWishlist(c.id));
        actions.appendChild(removeBtn);

        li.appendChild(nameDiv);
        li.appendChild(actions);
        ul.appendChild(li);
    });
}

function loadCountries() {
    const base = `${API_ROUTES.usersApi}/${userId}`;
    return Promise.all([
        apiRequest("GET", `${base}/visited`, null),
        apiRequest("GET", `${base}/wishlist`, null)
    ]).then(([v, w]) => {
        visited = v || [];
        wishlist = w || [];
        renderLists();
        renderMap();
    }).catch(() => setStatus("Failed to load your countries.", true));
}

/* ===================== Country actions ===================== */
function removeVisited(countryId) {
    if (!confirm("Remove this country from your visited list?")) return;
    apiRequest("DELETE", `${API_ROUTES.usersApi}/${userId}/visited/${countryId}`, null)
        .then(() => { setStatus("Removed from visited."); return loadCountries(); })
        .catch(() => setStatus("Could not remove country.", true));
}

function removeWishlist(countryId) {
    if (!confirm("Remove this country from your wishlist?")) return;
    apiRequest("DELETE", `${API_ROUTES.usersApi}/${userId}/wishlist/${countryId}`, null)
        .then(() => { setStatus("Removed from wishlist."); return loadCountries(); })
        .catch(() => setStatus("Could not remove country.", true));
}

function moveToVisited(countryId) {
    apiRequest("POST", `${API_ROUTES.usersApi}/${userId}/moveToVisited/${countryId}`, null)
        .then(() => { setStatus("Moved to visited."); return loadCountries(); })
        .catch(() => setStatus("Could not move country.", true));
}

/* ===================== Preferences: continents ===================== */
function loadContinents() {
    return apiRequest("GET", `${API_ROUTES.usersApi}/getContinentPreferences/${userId}`, null)
        .then(list => {
            originalContinents = list || [];
            document.querySelectorAll("input[name='continent']").forEach(cb => {
                cb.checked = originalContinents.includes(cb.value);
            });
        })
        .catch(() => setStatus("Failed to load continent preferences.", true));
}

function getCheckedContinents() {
    return Array.from(document.querySelectorAll("input[name='continent']:checked")).map(cb => cb.value);
}

/* ===================== Preferences: languages ===================== */
function loadLanguages() {
    return apiRequest("GET", `${API_ROUTES.usersApi}/getUserLanguages/${userId}`, null)
        .then(dict => {
            originalLanguages = dict || {};
            stagedLanguages = Object.assign({}, originalLanguages);
            renderStagedLanguages();
        })
        .catch(() => setStatus("Failed to load languages.", true));
}

function renderStagedLanguages() {
    const list = document.getElementById("languages-list");
    list.innerHTML = "";

    const entries = Object.entries(stagedLanguages);
    if (!entries.length) {
        const li = document.createElement("li");
        li.className = "uh-empty";
        li.style.border = "none";
        li.style.background = "none";
        li.textContent = "No languages added yet.";
        list.appendChild(li);
        return;
    }

    entries.forEach(([lang, level]) => {
        const li = document.createElement("li");
        const left = document.createElement("div");
        left.innerHTML = `<strong>${lang}</strong><span class="uh-lang-level">(${level})</span>`;

        const btn = document.createElement("button");
        btn.type = "button";
        btn.className = "uh-btn uh-btn-danger uh-btn-small";
        btn.textContent = "Remove";
        btn.addEventListener("click", () => {
            delete stagedLanguages[lang];
            renderStagedLanguages();
        });

        li.appendChild(left);
        li.appendChild(btn);
        list.appendChild(li);
    });
}

function addStagedLanguage() {
    const lang = document.getElementById("lang-select").value;
    const level = document.getElementById("lang-level-select").value;
    if (stagedLanguages.hasOwnProperty(lang)) {
        setStatus(`${lang} is already in your list.`, true);
        return;
    }
    stagedLanguages[lang] = level;
    renderStagedLanguages();
}

/* ===================== Preferences: batched Save ===================== */
function savePreferences() {
    const errEl = document.getElementById("prefs-error");
    errEl.textContent = "";
    const base = `${API_ROUTES.usersApi}/${userId}`;
    const calls = [];

    // Continents diff
    const current = getCheckedContinents();
    current.filter(c => !originalContinents.includes(c))
        .forEach(c => calls.push(apiRequest("POST", `${base}/continent/${encodeURIComponent(c)}`, null)));
    originalContinents.filter(c => !current.includes(c))
        .forEach(c => calls.push(apiRequest("DELETE", `${base}/continent/${encodeURIComponent(c)}`, null)));

    // Languages diff (a level change = remove + add)
    Object.keys(originalLanguages).forEach(lang => {
        if (!stagedLanguages.hasOwnProperty(lang) || stagedLanguages[lang] !== originalLanguages[lang]) {
            calls.push(apiRequest("DELETE", `${base}/language/${encodeURIComponent(lang)}`, null));
        }
    });
    Object.keys(stagedLanguages).forEach(lang => {
        if (!originalLanguages.hasOwnProperty(lang) || originalLanguages[lang] !== stagedLanguages[lang]) {
            calls.push(apiRequest("POST", `${base}/language`, { language: lang, level: stagedLanguages[lang] }));
        }
    });

    if (!calls.length) {
        setStatus("No preference changes to save.");
        return;
    }

    const btn = document.getElementById("save-prefs-btn");
    btn.disabled = true;
    Promise.all(calls)
        .then(() => {
            setStatus("Preferences saved.");
            return Promise.all([loadContinents(), loadLanguages()]);
        })
        .catch(() => {
            errEl.textContent = "Some changes could not be saved. Reverting to your saved preferences.";
            return Promise.all([loadContinents(), loadLanguages()]);
        })
        .finally(() => { btn.disabled = false; });
}

/* ===================== Change password ===================== */
function isValidPassword(pw) {
    return /^(?=.*[A-Z])(?=.*\d).{8,}$/.test(pw);
}

function changePassword(e) {
    e.preventDefault();
    const errEl = document.getElementById("password-error");
    const okEl = document.getElementById("password-success");
    errEl.textContent = "";
    okEl.textContent = "";

    const current = document.getElementById("current-password").value;
    const next = document.getElementById("new-password").value;
    const confirm = document.getElementById("confirm-password").value;

    if (!current) { errEl.textContent = "Enter your current password."; return; }
    if (!isValidPassword(next)) {
        errEl.textContent = "New password must be at least 8 characters, include 1 uppercase letter and 1 number.";
        return;
    }
    if (next !== confirm) { errEl.textContent = "New passwords do not match."; return; }
    if (next === current) { errEl.textContent = "New password must be different from the current one."; return; }

    const btn = document.getElementById("change-password-btn");
    btn.disabled = true;
    apiRequest("POST", `${API_ROUTES.usersApi}/changePassword`, {
        userId: userId,
        currentPassword: current,
        newPassword: next
    })
        .then(() => {
            okEl.textContent = "Password changed successfully.";
            document.getElementById("password-form").reset();
        })
        .catch(xhr => {
            errEl.textContent = (xhr && xhr.status === 401)
                ? "Current password is incorrect."
                : "Could not change password. Please try again.";
        })
        .finally(() => { btn.disabled = false; });
}

/* ===================== Init ===================== */
document.addEventListener("DOMContentLoaded", () => {
    if (!userId) return;
    initMap();
    loadCountries();
    loadContinents();
    loadLanguages();

    document.getElementById("add-language-btn").addEventListener("click", addStagedLanguage);
    document.getElementById("save-prefs-btn").addEventListener("click", savePreferences);
    document.getElementById("password-form").addEventListener("submit", changePassword);
});
