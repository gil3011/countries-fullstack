let allUsers = [];

$(document).ready(function () {
    getUsers();
    getAdminStats();
    getDailyLoginCounts();

    $("#user-search").on("input", filterUsers);

    $("#login-range").on("change", function () {
        getDailyLoginCounts();
    });
});


/* =========================
   Server requests
========================= */

function getUsers() {
    ajaxCall(
        "GET",
        userAPI,
        null,
        getUsersSuccess,
        requestFailed
    );
}


function getAdminStats() {
    ajaxCall(
        "GET",
        userAPI + "/admin/stats",
        null,
        getAdminStatsSuccess,
        requestFailed
    );
}


function getDailyLoginCounts() {
    const days = Number($("#login-range").val());

    ajaxCall(
        "GET",
        userAPI + "/admin/GetDailyLoginCounts?days=" + days,
        null,
        getDailyLoginCountsSuccess,
        requestFailed
    );
}


/* =========================
   Success callbacks
========================= */

function getUsersSuccess(users) {
    allUsers = users;

    renderUsers(users);
}


function getAdminStatsSuccess(data) {
    console.log("Admin stats:", data);

    const stats = Array.isArray(data)
        ? data[0]
        : data;

    renderAdminStats(stats);
}


function getDailyLoginCountsSuccess(loginData) {
    console.log("Daily login counts:", loginData);

    renderDailyLoginCounts(loginData);
}


/* =========================
   Render admin statistics
========================= */

function renderAdminStats(stats) {
    if (!stats) {
        console.error("Admin statistics were not received");
        return;
    }

    $("#daily-logins").text(
        stats.dailyLogins ??
        stats.DailyLogins ??
        0
    );

    $("#imported-countries").text(
        stats.importedCountries ??
        stats.ImportedCountries ??
        0
    );

    $("#wishlist-countries").text(
        stats.wishlistCountries ??
        stats.WishlistCountries ??
        0
    );

    $("#visited-countries").text(
        stats.visitedCountries ??
        stats.VisitedCountries ??
        0
    );

    $("#shares-count").text(
        stats.shares ??
        stats.Shares ??
        stats.sharesCount ??
        stats.SharesCount ??
        0
    );
}


/* =========================
   Render daily login counts
========================= */

function renderDailyLoginCounts(loginData) {
    const tableBody = $("#login-counts-body");

    tableBody.empty();

    console.log("Login data received:", loginData);

    if (!loginData) {
        tableBody.html(`
            <tr>
                <td colspan="2" class="empty-table-message">
                    No login activity was found
                </td>
            </tr>
        `);

        return;
    }

    let loginItems = [];

    if (Array.isArray(loginData)) {
        loginItems = loginData;
    } else if (typeof loginData === "object") {
        loginItems = Object.entries(loginData).map(
            function ([date, count]) {
                return {
                    date: date,
                    count: count
                };
            }
        );
    }

    if (loginItems.length === 0) {
        tableBody.html(`
            <tr>
                <td colspan="2" class="empty-table-message">
                    No login activity was found
                </td>
            </tr>
        `);

        return;
    }

    loginItems.forEach(function (item) {
        const date =
            item.date ??
            item.Date ??
            item.loginDate ??
            item.LoginDate ??
            "";

        const count =
            item.loginCount ??
            item.LoginCount ??
            item.dailyLogins ??
            item.DailyLogins ??
            item.count ??
            item.Count ??
            0;

        const row = `
            <tr>
                <td>${formatDate(date)}</td>
                <td>${count}</td>
            </tr>
        `;

        tableBody.append(row);
    });
}


/* =========================
   Render users
========================= */

function renderUsers(users) {
    const tableBody = $("#users-table-body");

    tableBody.empty();

    if (!users || users.length === 0) {
        tableBody.html(`
            <tr>
                <td colspan="7" class="empty-table-message">
                    No users were found
                </td>
            </tr>
        `);

        return;
    }

    users.forEach(function (user) {
        const roleBadge = getRoleBadge(user);
        const statusBadge = getStatusBadge(user);
        const sharingBadge = getSharingBadge(user);
        const actionButtons = getUserActionButtons(user);

        const row = `
            <tr data-user-id="${user.id}">
                <td>${user.id}</td>

                <td>
                    ${escapeHtml(user.username)}
                </td>

                <td>
                    ${escapeHtml(user.email)}
                </td>

                <td>
                    ${roleBadge}
                </td>

                <td>
                    ${statusBadge}
                </td>

                <td>
                    ${sharingBadge}
                </td>

                <td>
                    ${actionButtons}
                </td>
            </tr>
        `;

        tableBody.append(row);
    });
}


/* =========================
   User badges
========================= */

function getRoleBadge(user) {
    if (user.isAdmin) {
        return `
            <span class="badge admin-badge">
                Admin
            </span>
        `;
    }

    return `
        <span class="badge user-badge">
            User
        </span>
    `;
}


function getStatusBadge(user) {
    if (user.isBlocked) {
        return `
            <span class="badge blocked-badge">
                Blocked
            </span>
        `;
    }

    return `
        <span class="badge active-badge">
            Active
        </span>
    `;
}


function getSharingBadge(user) {
    if (user.isAllowedToShare) {
        return `
            <span class="badge allowed-badge">
                Allowed
            </span>
        `;
    }

    return `
        <span class="badge disabled-badge">
            Disabled
        </span>
    `;
}


/* =========================
   User action buttons
========================= */

function getUserActionButtons(user) {
    const blockButtonText = user.isBlocked
        ? "Unblock"
        : "Block";

    const sharingButtonText = user.isAllowedToShare
        ? "Disable Sharing"
        : "Enable Sharing";

    return `
        <div class="action-buttons">

            <button
                type="button"
                class="action-btn block-btn"
                data-user-id="${user.id}">
                ${blockButtonText}
            </button>

            <button
                type="button"
                class="action-btn permission-btn"
                data-user-id="${user.id}">
                ${sharingButtonText}
            </button>

        </div>
    `;
}


/* =========================
   Search users
========================= */

function filterUsers() {
    const searchText = String(
        $("#user-search").val() ?? ""
    )
        .trim()
        .toLowerCase();

    if (searchText === "") {
        renderUsers(allUsers);
        return;
    }

    const filteredUsers = allUsers.filter(function (user) {
        const username = String(
            user.username ?? ""
        ).toLowerCase();

        const email = String(
            user.email ?? ""
        ).toLowerCase();

        return (
            username.includes(searchText) ||
            email.includes(searchText)
        );
    });

    renderUsers(filteredUsers);
}


/* =========================
   Helper functions
========================= */

function formatDate(dateValue) {
    if (!dateValue) {
        return "";
    }

    const date = new Date(dateValue);

    if (Number.isNaN(date.getTime())) {
        return escapeHtml(dateValue);
    }

    return date.toLocaleDateString("en-GB", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric"
    });
}


function escapeHtml(value) {
    return $("<div>")
        .text(value ?? "")
        .html();
}


/* =========================
   Error handling
========================= */

function requestFailed(error) {
    console.error(
        "Admin dashboard request failed:",
        error
    );

    alert("Failed to load admin dashboard data");
}