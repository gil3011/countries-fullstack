/* ===================== My Profile page ===================== */

// authGuard.js (loaded in the page head) already redirects guests to login,
// so we only need to read the current user here.
const loggedInUser = JSON.parse(localStorage.getItem("loggedInUser"));
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

// --- Promise wrapper around the project's global ajaxCall ---
function apiRequest(method, url, data) {
    return new Promise((resolve, reject) => {
        ajaxCall(method, url, data ? JSON.stringify(data) : null, resolve, reject);
    });
}

// Escape text before injecting into innerHTML (quiz attempt cards use template
// strings). Mirrors the helper on the dashboard.
function escapeHtml(value) {
    return String(value == null ? '' : value)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
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

// Full profile load (countries + preferences) in a single request. Used on page load.
function loadProfile() {
    return apiRequest("GET", `${API_ROUTES.userAPI}/${userId}/profile`, null)
        .then(data => {
            data = data || {};

            visited = data.visited || [];
            wishlist = data.wishlist || [];
            renderLists();
            renderMap();

            originalContinents = data.continents || [];
            document.querySelectorAll("input[name='continent']").forEach(cb => {
                cb.checked = originalContinents.includes(cb.value);
            });

            originalLanguages = data.languages || {};
            stagedLanguages = Object.assign({}, originalLanguages);
            renderStagedLanguages();
        })
        .catch(() => setStatus("Failed to load your profile.", true));
}

// Reloads only the country lists (used after add/remove/move actions).
function loadCountries() {
    const base = `${API_ROUTES.userAPI}/${userId}`;
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
    apiRequest("DELETE", `${API_ROUTES.userAPI}/${userId}/visited/${countryId}`, null)
        .then(() => { setStatus("Removed from visited."); return loadCountries(); })
        .catch(() => setStatus("Could not remove country.", true));
}

function removeWishlist(countryId) {
    if (!confirm("Remove this country from your wishlist?")) return;
    apiRequest("DELETE", `${API_ROUTES.userAPI}/${userId}/wishlist/${countryId}`, null)
        .then(() => { setStatus("Removed from wishlist."); return loadCountries(); })
        .catch(() => setStatus("Could not remove country.", true));
}

function moveToVisited(countryId) {
    apiRequest("POST", `${API_ROUTES.userAPI}/${userId}/moveToVisited/${countryId}`, null)
        .then(() => { setStatus("Moved to visited."); return loadCountries(); })
        .catch(() => setStatus("Could not move country.", true));
}

/* ===================== Preferences: continents ===================== */
function getCheckedContinents() {
    return Array.from(document.querySelectorAll("input[name='continent']:checked")).map(cb => cb.value);
}

/* ===================== Preferences: languages ===================== */
function getServerLanguages() {
    ajaxCall("GET", `${API_ROUTES.countryAPI}/languages`, null,
        function (data) {
            const languagesFromServer = data || [];
            const select = document.getElementById("lang-select");
            select.innerHTML = "";
            languagesFromServer.forEach(lang => {
                const option = document.createElement("option");
                option.value = lang;
                option.textContent = lang;
                select.appendChild(option);
            });

        },
        function () { console.log("Failed to load languages from server."); }
    );
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
    const base = `${API_ROUTES.userAPI}/${userId}`;
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
            return loadProfile();
        })
        .catch(() => {
            errEl.textContent = "Some changes could not be saved. Reverting to your saved preferences.";
            return loadProfile();
        })
        .finally(() => { btn.disabled = false; });
}

/* ===================== AI Recommendations ===================== */
function getRecommendations() {
    const btn = document.getElementById("get-reco-btn");
    const loading = document.getElementById("reco-loading");
    const errEl = document.getElementById("reco-error");
    const emptyEl = document.getElementById("reco-empty");
    const list = document.getElementById("reco-list");

    errEl.classList.add("hidden");
    emptyEl.classList.add("hidden");
    list.innerHTML = "";
    loading.classList.remove("hidden");
    btn.disabled = true;

    apiRequest("POST", `${API_ROUTES.geminiAPI}/recommend`, { userId: userId, count: 3 })
        .then(res => {
            const recos = (res && res.data) || [];
            if (!recos.length) {
                emptyEl.textContent = "No recommendations available. Try adding some preferences first.";
                emptyEl.classList.remove("hidden");
                return;
            }
            renderRecommendations(recos);
        })
        .catch(xhr => {
            const serverError = xhr?.responseJSON?.error;
            errEl.textContent = serverError || "Could not get recommendations. Please try again.";
            errEl.classList.remove("hidden");
        })
        .finally(() => {
            loading.classList.add("hidden");
            btn.disabled = false;
        });
}

function renderRecommendations(recos) {
    const list = document.getElementById("reco-list");
    list.innerHTML = "";

    recos.forEach(r => {
        const card = document.createElement("article");
        card.className = "uh-reco-card";

        const flag = r.flagUrl ? `<img src="${r.flagUrl}" alt="${r.commonName || ""} flag">` : "";
        const name = r.commonName || "Unknown";
        const region = r.region || "";

        card.innerHTML = `
            <div class="uh-reco-head">
                <span class="uh-reco-rank">#${r.rank}</span>
                ${flag}
                <div class="uh-reco-title">
                    <b>${name}</b>
                    <span class="uh-reco-region">${region}</span>
                </div>
            </div>
            <p class="uh-reco-reason"></p>
            <div class="uh-row-actions">
                <button type="button" class="uh-btn uh-btn-small uh-reco-view">View country</button>
                <button type="button" class="uh-btn uh-btn-success uh-btn-small uh-reco-wishlist">+ Wishlist</button>
            </div>
        `;

        card.querySelector(".uh-reco-reason").textContent = r.reason || "";
        card.querySelector(".uh-reco-view").addEventListener("click", () => goToCountry(r.cca3));

        const wishBtn = card.querySelector(".uh-reco-wishlist");
        wishBtn.addEventListener("click", () => addRecoToWishlist(r.id, wishBtn));

        list.appendChild(card);
    });
}

function addRecoToWishlist(countryId, btn) {
    if (!countryId) return;
    btn.disabled = true;
    apiRequest("POST", `${API_ROUTES.userAPI}/${userId}/wishlist/${countryId}`, null)
        .then(() => {
            setStatus("Added to wishlist.");
            btn.textContent = "★ Wishlisted";
            // Refresh the map and country lists so the new wishlist entry shows up.
            return loadCountries();
        })
        .catch(() => {
            setStatus("Could not add to wishlist.", true);
            btn.disabled = false;
        });
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
    apiRequest("POST", `${API_ROUTES.userAPI}/changePassword`, {
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
    loadProfile();
    getServerLanguages();

    document.getElementById("add-language-btn").addEventListener("click", addStagedLanguage);
    document.getElementById("save-prefs-btn").addEventListener("click", savePreferences);
    document.getElementById("password-form").addEventListener("submit", changePassword);
    document.getElementById("get-reco-btn").addEventListener("click", getRecommendations);
    document.getElementById("reco-empty").classList.remove("hidden");
});

let currentUserShares = [];

const UserShareType = Object.freeze({
    Recommendation: 0,
    Thought: 1,
    Review: 2
});

$(document).ready(function () {
    bindMySharesEvents();
    loadMyShares();
    loadUserQuizAttempts();
});

function bindMySharesEvents() {
    document
        .getElementById("edit-share-form")
        ?.addEventListener("submit", submitEditShareForm);

    document
        .getElementById("close-edit-share-modal-btn")
        ?.addEventListener("click", closeEditShareModal);

    document
        .getElementById("cancel-edit-share-btn")
        ?.addEventListener("click", closeEditShareModal);

    document
        .querySelector("#edit-share-modal .uh-modal-overlay")
        ?.addEventListener("click", closeEditShareModal);

    document.addEventListener("keydown", function (event) {
        if (event.key === "Escape") {
            closeEditShareModal();
        }
    });
}

function loadMyShares() {
    const user = getUserLoggedIn();

    if (!user) {
        showMySharesError("Could not identify the logged-in user.");
        return;
    }

    const userId =
        user.id ||
        user.Id ||
        user.userId;

    if (!userId) {
        showMySharesError("Could not identify the logged-in user.");
        return;
    }

    showMySharesLoading();

    const url =
        API_ROUTES.shareAPI +
        "/GetUserShares/" +
        encodeURIComponent(userId);

    ajaxCall(
        "GET",
        url,
        null,
        handleMySharesSuccess,
        handleMySharesError
    );
}

function handleMySharesSuccess(shares) {
    currentUserShares =
        Array.isArray(shares)
            ? shares
            : [];

    renderMyShares();
}

function handleMySharesError(error) {
    console.error("Failed to load user shares:", error);

    currentUserShares = [];

    showMySharesError("Could not load your shares.");
}

function renderMyShares() {
    const listElement =
        document.getElementById("my-shares-list");

    const loadingElement =
        document.getElementById("my-shares-loading");

    const emptyElement =
        document.getElementById("my-shares-empty");

    const errorElement =
        document.getElementById("my-shares-error");

    loadingElement?.classList.add("hidden");
    emptyElement?.classList.add("hidden");
    errorElement?.classList.add("hidden");

    if (!listElement) {
        return;
    }

    listElement.innerHTML = "";

    updateMySharesCount(currentUserShares.length);

    if (currentUserShares.length === 0) {
        emptyElement?.classList.remove("hidden");
        return;
    }

    currentUserShares.forEach(share => {
        listElement.appendChild(
            createMyShareCard(share)
        );
    });
}

function createMyShareCard(share) {
    const card =
        document.createElement("article");

    card.className = "uh-share-card";
    card.dataset.shareId = share.id;

    const shareType =
        Number(share.type);

    const typeName =
        getUserShareTypeName(shareType);

    const typeClass =
        getUserShareTypeClass(shareType);

    const countryName =
        share.countryName ||
        share.commonName ||
        "Unknown Country";

    const formattedDate =
        formatUserShareDate(share.createdAt);

    card.innerHTML = `
        <div class="uh-share-card-head">
            <div>
                <h3 class="uh-share-title"></h3>
                <a class="uh-share-country"></a>
            </div>

            <span class="uh-share-type"></span>
        </div>

        <p class="uh-share-description"></p>

        <div class="uh-share-footer">
            <span class="uh-share-date"></span>

            <div class="uh-share-actions">
                <button
                    type="button"
                    class="uh-share-edit-btn">
                    Edit
                </button>

                <button
                    type="button"
                    class="uh-share-delete-btn">
                    Delete
                </button>
            </div>
        </div>
    `;

    card
        .querySelector(".uh-share-title")
        .textContent =
        share.title || "Country Share";

    card
        .querySelector(".uh-share-description")
        .textContent =
        share.description || "No description was provided.";

    card
        .querySelector(".uh-share-date")
        .textContent = formattedDate;

    const typeElement =
        card.querySelector(".uh-share-type");

    typeElement.textContent = typeName;
    typeElement.classList.add(typeClass);

    const countryLink =
        card.querySelector(".uh-share-country");

    countryLink.textContent = countryName;

    setUserShareCountryLink(
        countryLink,
        share
    );

    card
        .querySelector(".uh-share-edit-btn")
        .addEventListener("click", function () {
            openEditShareModal(share.id);
        });

    card
        .querySelector(".uh-share-delete-btn")
        .addEventListener("click", function () {
            deleteUserShare(share.id);
        });

    return card;
}

function setUserShareCountryLink(countryLink, share) {
    const cca3 =
        share.cca3 ||
        share.countryCca3 ||
        share.countryCode;

    if (!cca3) {
        countryLink.href = "#";

        countryLink.addEventListener("click", function (event) {
            event.preventDefault();
        });

        return;
    }

    countryLink.href =
        "country.html?cca3=" +
        encodeURIComponent(cca3);
}

function openEditShareModal(shareId) {
    const share =
        currentUserShares.find(item =>
            Number(item.id) === Number(shareId)
        );

    if (!share) {
        return;
    }

    document
        .getElementById("edit-share-id")
        .value = share.id;

    document
        .getElementById("edit-share-title")
        .value = share.title || "";

    document
        .getElementById("edit-share-type")
        .value = Number(share.type);

    document
        .getElementById("edit-share-description")
        .value = share.description || "";

    clearEditShareError();

    document
        .getElementById("edit-share-modal")
        .classList.remove("hidden");

    document.body.style.overflow = "hidden";

    document
        .getElementById("edit-share-title")
        .focus();
}

function closeEditShareModal() {
    const modal =
        document.getElementById("edit-share-modal");

    if (!modal) {
        return;
    }

    modal.classList.add("hidden");
    document.body.style.overflow = "";

    clearEditShareError();
}

function submitEditShareForm(event) {
    event.preventDefault();

    const shareId =
        Number(
            document
                .getElementById("edit-share-id")
                .value
        );

    const title =
        document
            .getElementById("edit-share-title")
            .value
            .trim();

    const description =
        document
            .getElementById("edit-share-description")
            .value
            .trim();

    const type =
        Number(
            document
                .getElementById("edit-share-type")
                .value
        );

    if (!shareId || !title || !description) {
        showEditShareError("Please complete all fields.");
        return;
    }

    const originalShare =
        currentUserShares.find(item =>
            Number(item.id) === shareId
        );

    if (!originalShare) {
        showEditShareError("Share could not be found.");
        return;
    }

    const updatedShare = {
        id: shareId,
        userId: originalShare.userId,
        title: title,
        description: description,
        type: type,
        countryId: originalShare.countryId,
        createdAt: originalShare.createdAt
    };

    const saveButton =
        document.getElementById("save-edit-share-btn");

    saveButton.disabled = true;
    saveButton.textContent = "Saving...";

    ajaxCall(
        "PUT",
        API_ROUTES.shareAPI + "/UpdateShare",
        JSON.stringify(updatedShare),
        handleUpdateShareSuccess,
        handleUpdateShareError
    );
}

function handleUpdateShareSuccess() {
    resetEditShareButton();
    closeEditShareModal();
    loadMyShares();

    setProfileStatus(
        "Share updated successfully.",
        "success"
    );
}

function handleUpdateShareError(error) {
    console.error("Failed to update share:", error);

    resetEditShareButton();

    const status = error?.status;

    const serverMessage =
        error?.responseJSON?.message;

    let message;

    if (status === 403) {
        // Sharing privilege revoked or the account is blocked.
        message =
            serverMessage ||
            "You are not allowed to share.";
    } else if (status === 401) {
        message =
            serverMessage ||
            "You must be logged in to update a share.";
    } else if (status === 404) {
        message =
            serverMessage ||
            "Share could not be found.";
    } else {
        message =
            serverMessage ||
            "Could not update the share.";
    }

    showEditShareError(message);
}

function deleteUserShare(shareId) {
    const share =
        currentUserShares.find(item =>
            Number(item.id) === Number(shareId)
        );

    if (!share) {
        return;
    }

    const confirmed =
        confirm(
            `Are you sure you want to delete "${share.title}"?`
        );

    if (!confirmed) {
        return;
    }

    const url =
        API_ROUTES.shareAPI +
        "/DeleteShare?" + "shareID=" + share.id + "&userID=" + share.userId;

    ajaxCall(
        "DELETE",
        url,
        null,
        function () {
            handleDeleteShareSuccess(shareId);
        },
        handleDeleteShareError
    );
}

function handleDeleteShareSuccess(shareId) {
    currentUserShares =
        currentUserShares.filter(share =>
            Number(share.id) !== Number(shareId)
        );

    renderMyShares();

    setProfileStatus(
        "Share deleted successfully.",
        "success"
    );
}

function handleDeleteShareError(error) {
    console.error("Failed to delete share:", error);

    setProfileStatus(
        "Could not delete the share.",
        "error"
    );
}

function getUserShareTypeName(type) {
    switch (type) {
        case UserShareType.Recommendation:
            return "Recommendation";

        case UserShareType.Thought:
            return "Thought";

        case UserShareType.Review:
            return "Review";

        default:
            return "Unknown";
    }
}

function getUserShareTypeClass(type) {
    switch (type) {
        case UserShareType.Recommendation:
            return "uh-share-type-recommendation";

        case UserShareType.Thought:
            return "uh-share-type-thought";

        case UserShareType.Review:
            return "uh-share-type-review";

        default:
            return "uh-share-type-unknown";
    }
}

function formatUserShareDate(createdAt) {
    if (!createdAt) {
        return "Unknown date";
    }

    const date =
        new Date(createdAt);

    if (Number.isNaN(date.getTime())) {
        return "Unknown date";
    }

    return date.toLocaleDateString(
        "en-GB",
        {
            day: "2-digit",
            month: "short",
            year: "numeric"
        }
    );
}

function updateMySharesCount(count) {
    const countElement =
        document.getElementById("my-shares-count");

    if (countElement) {
        countElement.textContent = count;
    }
}

function showMySharesLoading() {
    document
        .getElementById("my-shares-loading")
        ?.classList.remove("hidden");

    document
        .getElementById("my-shares-empty")
        ?.classList.add("hidden");

    document
        .getElementById("my-shares-error")
        ?.classList.add("hidden");

    const listElement =
        document.getElementById("my-shares-list");

    if (listElement) {
        listElement.innerHTML = "";
    }

    updateMySharesCount(0);
}

function showMySharesError(message) {
    document
        .getElementById("my-shares-loading")
        ?.classList.add("hidden");

    document
        .getElementById("my-shares-empty")
        ?.classList.add("hidden");

    const errorElement =
        document.getElementById("my-shares-error");

    if (errorElement) {
        errorElement.textContent = message;
        errorElement.classList.remove("hidden");
    }

    updateMySharesCount(0);
}

function showEditShareError(message) {
    const errorElement =
        document.getElementById("edit-share-error");

    if (errorElement) {
        errorElement.textContent = message;
    }
}

function clearEditShareError() {
    const errorElement =
        document.getElementById("edit-share-error");

    if (errorElement) {
        errorElement.textContent = "";
    }
}

function resetEditShareButton() {
    const saveButton =
        document.getElementById("save-edit-share-btn");

    if (!saveButton) {
        return;
    }

    saveButton.disabled = false;
    saveButton.textContent = "Save Changes";
}

function setProfileStatus(message, type) {
    const statusElement =
        document.getElementById("uh-status");

    if (!statusElement) {
        return;
    }

    statusElement.textContent = message;
    statusElement.className =
        "uh-status " + type;

    setTimeout(function () {
        statusElement.textContent = "";
        statusElement.className = "uh-status";
    }, 3000);
}

window.onShareCreated = function () {
    loadMyShares();
};

// Quizzez
function loadUserQuizAttempts() {
    const loggedInUserJson =
        localStorage.getItem("loggedInUser") ||
        sessionStorage.getItem("loggedInUser");

    const loadingElement = $("#my-attempts-loading");
    const emptyElement = $("#my-attempts-empty");
    const errorElement = $("#my-attempts-error");
    const attemptsList = $("#my-attempts-list");

    loadingElement.removeClass("hidden");
    emptyElement.addClass("hidden");
    errorElement.addClass("hidden");
    attemptsList.empty();

    if (!loggedInUserJson) {
        loadingElement.addClass("hidden");
        errorElement
            .text("No logged-in user was found.")
            .removeClass("hidden");

        return;
    }

    let loggedInUser;

    try {
        loggedInUser = JSON.parse(loggedInUserJson);
    } catch (error) {
        console.error("Invalid logged-in user data:", error);

        loadingElement.addClass("hidden");
        errorElement
            .text("Could not read the logged-in user.")
            .removeClass("hidden");

        return;
    }

    const userId = loggedInUser.id;

    if (!userId) {
        loadingElement.addClass("hidden");
        errorElement
            .text("User ID was not found.")
            .removeClass("hidden");

        return;
    }

    ajaxCall(
        "GET",
        `${API_ROUTES.quizAttemptAPI}/User/${userId}`,
        "",
        renderUserQuizAttempts,
        function (error) {
            console.error("Failed to load quiz attempts:", error);

            loadingElement.addClass("hidden");
            errorElement
                .text("Could not load your quiz attempts.")
                .removeClass("hidden");
        }
    );
}

function renderUserQuizAttempts(attempts) {
    const loadingElement = $("#my-attempts-loading");
    const emptyElement = $("#my-attempts-empty");
    const errorElement = $("#my-attempts-error");
    const attemptsList = $("#my-attempts-list");

    loadingElement.addClass("hidden");
    errorElement.addClass("hidden");
    attemptsList.empty();

    if (!attempts || attempts.length === 0) {
        emptyElement.removeClass("hidden");
        return;
    }

    emptyElement.addClass("hidden");

    const sortedAttempts = [...attempts]
        .sort((a, b) => new Date(b.dateTaken) - new Date(a.dateTaken))
        .slice(0, 5);

    sortedAttempts.forEach(attempt => {
        const quizTitle =
            attempt.quizTitle || `Quiz ${attempt.quizId}`;

        const formattedDate = attempt.dateTaken
            ? new Date(attempt.dateTaken).toLocaleString([], {
                year: "numeric",
                month: "numeric",
                day: "numeric",
                hour: "2-digit",
                minute: "2-digit"
            })
            : "Unknown date";

        const score = attempt.score ?? 0;

        attemptsList.append(`
            <article class="uh-attempt-card">
                <div class="uh-attempt-details">
                    <h3 class="uh-attempt-title">
                        ${escapeHtml(quizTitle)}
                    </h3>

                    <span class="uh-attempt-date">
                        ${formattedDate}
                    </span>
                </div>

                <div class="uh-attempt-score">
                    ${score}%
                </div>
            </article>
        `);
    });
}