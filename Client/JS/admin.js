let allUsers = [];
let allLoginCounts = {};
let currentLog = { date: "", lines: [] };

$(document).ready(function () {
    getUsers();
    getAdminStats();
    getDailyLoginCounts();
    getLogDates();

    $("#user-search").on("input", filterUsers);

    $("#login-range").on("change", function () {
        filterAndRenderLoginCounts();
    });

    $("#log-date").on("change", loadSelectedLog);
    $("#log-download-btn").on("click", downloadCurrentLog);
});


/* =========================
   Server requests
========================= */

function getUsers() {
    ajaxCall(
        "GET",
        API_ROUTES.userAPI,
        null,
        getUsersSuccess,
        requestFailed
    );
}


function getAdminStats() {
    ajaxCall(
        "GET",
        API_ROUTES.userAPI + "/admin/stats",
        null,
        getAdminStatsSuccess,
        requestFailed
    );
}


function getDailyLoginCounts() {
    ajaxCall(
        "GET",
        API_ROUTES.userAPI + "/admin/GetDailyLoginCounts",
        null,
        getDailyLoginCountsSuccess,
        requestFailed
    );
}


function getLogDates() {
    ajaxCall(
        "GET",
        API_ROUTES.logAPI + "/dates",
        null,
        getLogDatesSuccess,
        requestFailed
    );
}


function loadSelectedLog() {
    const date = $("#log-date").val();

    if (!date) {
        renderLog({ date: "", lines: [] });
        return;
    }

    $("#log-status").text("Loading log for " + date + "...");

    ajaxCall(
        "GET",
        API_ROUTES.logAPI + "/" + date,
        null,
        renderLog,
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
    const stats = Array.isArray(data)
        ? data[0]
        : data;

    renderAdminStats(stats);
}


function getDailyLoginCountsSuccess(loginCounts) {
    allLoginCounts = loginCounts ?? {};

    filterAndRenderLoginCounts();
}


function getLogDatesSuccess(dates) {
    const select = $("#log-date");
    select.empty();

    const available = Array.isArray(dates) ? dates : [];

    if (available.length === 0) {
        select.append(
            $("<option></option>").val("").text("No logs available")
        );
        $("#log-status").text("No log files were found on the server.");
        return;
    }

    available.forEach(function (date) {
        select.append(
            $("<option></option>").val(date).text(date)
        );
    });

    // Auto-select and load the most recent day (first item).
    select.val(available[0]);
    loadSelectedLog();
}


/* =========================
   Render server logs
========================= */

function renderLog(data) {
    const date = data?.date ?? "";
    const lines = Array.isArray(data?.lines) ? data.lines : [];

    currentLog = { date: date, lines: lines };

    $("#log-download-btn").prop("disabled", lines.length === 0);

    if (!date) {
        $("#log-status").text("Select a date to see its entry count.");
    } else if (lines.length === 0) {
        $("#log-status").text("No log entries for " + date + ".");
    } else {
        $("#log-status").text(lines.length + " log entries for " + date + ". Use Download to save the file.");
    }
}


function downloadCurrentLog() {
    if (!currentLog.date || currentLog.lines.length === 0) {
        return;
    }

    const blob = new Blob(
        [currentLog.lines.join("\n")],
        { type: "text/plain;charset=utf-8" }
    );

    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = "log-" + currentLog.date + ".log";
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
}


/* =========================
   Filter daily login counts
========================= */

function filterAndRenderLoginCounts() {
    const days = Number($("#login-range").val());

    let loginItems = [];

    if (Array.isArray(allLoginCounts)) {
        loginItems = allLoginCounts.map(function (item) {
            return {
                date:
                    item.date ??
                    item.Date ??
                    item.loginDate ??
                    item.LoginDate ??
                    "",

                count:
                    item.loginCount ??
                    item.LoginCount ??
                    item.dailyLogins ??
                    item.DailyLogins ??
                    item.count ??
                    item.Count ??
                    0
            };
        });
    } else if (
        allLoginCounts &&
        typeof allLoginCounts === "object"
    ) {
        loginItems = Object.entries(allLoginCounts).map(
            function ([date, count]) {
                return {
                    date: date,
                    count: count
                };
            }
        );
    }

    loginItems.sort(function (a, b) {
        return new Date(a.date) - new Date(b.date);
    });

    const filteredLoginCounts = loginItems.slice(-days);

    renderDailyLoginCounts(filteredLoginCounts);
}


/* =========================
   Render admin statistics
========================= */

function renderAdminStats(stats) {
    if (!stats) {
        console.error(
            "Admin statistics were not received"
        );

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

    if (
        !Array.isArray(loginData) ||
        loginData.length === 0
    ) {
        tableBody.html(`
            <tr>
                <td colspan="2" class="empty-table-message">
                    No login activity was found
                </td>
            </tr>
        `);

        return;
    }

    loginData.forEach(function (item) {
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

    const adminButtonText = user.isAdmin
        ? "Demote from Admin"
        : "Promote to Admin";

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

            <button
                type="button"
                class="action-btn admin-btn"
                data-user-id="${user.id}">
                ${adminButtonText}
            </button>

        </div>
    `;
}

/* =========================
   User action events
========================= */

$(document).on("click", ".block-btn", function () {
    const userId = Number($(this).data("user-id"));

    changeUserBlockStatus(userId);
});


$(document).on("click", ".permission-btn", function () {
    const userId = Number($(this).data("user-id"));

    changeUserSharingPermission(userId);
});


$(document).on("click", ".admin-btn", function () {
    const userId = Number($(this).data("user-id"));

    changeUserAdminRole(userId);
});

/* =========================
   User action events
========================= */

function changeUserBlockStatus(userId) {
    const user = allUsers.find(function (item) {
        return item.id === userId;
    });

    if (!user) {
        console.error("User was not found:", userId);
        return;
    }

    const endpoint = user.isBlocked
        ? `${API_ROUTES.userAPI}/admin/unblockUser/${userId}`
        : `${API_ROUTES.userAPI}/admin/blockUser/${userId}`;

    const actionText = user.isBlocked
        ? "unblock"
        : "block";

    const confirmed = confirm(
        `Are you sure you want to ${actionText} ${user.username}?`
    );

    if (!confirmed) {
        return;
    }

    ajaxCall(
        "PUT",
        endpoint,
        null,
        function () {
            user.isBlocked = !user.isBlocked;
            renderUsers(allUsers);
        },
        requestFailed
    );
}

function changeUserSharingPermission(userId) {
    const user = allUsers.find(function (item) {
        return item.id === userId;
    });

    if (!user) {
        console.error("User was not found:", userId);
        return;
    }

    const endpoint = user.isAllowedToShare
        ? `${API_ROUTES.userAPI}/admin/preventSharing/${userId}`
        : `${API_ROUTES.userAPI}/admin/allowSharing/${userId}`;

    const actionText = user.isAllowedToShare
        ? "disable sharing for"
        : "enable sharing for";

    const confirmed = confirm(
        `Are you sure you want to ${actionText} ${user.username}?`
    );

    if (!confirmed) {
        return;
    }

    ajaxCall(
        "PUT",
        endpoint,
        null,
        function () {
            user.isAllowedToShare = !user.isAllowedToShare;
            renderUsers(allUsers);
        },
        requestFailed
    );
}

function changeUserAdminRole(userId) {
    const user = allUsers.find(function (item) {
        return item.id === userId;
    });

    if (!user) {
        console.error("User was not found:", userId);
        return;
    }

    const endpoint = user.isAdmin
        ? `${API_ROUTES.userAPI}/admin/demote/${userId}`
        : `${API_ROUTES.userAPI}/admin/promote/${userId}`;

    const actionText = user.isAdmin
        ? "demote from admin"
        : "promote to admin";

    const confirmed = confirm(
        `Are you sure you want to ${actionText}: ${user.username}?`
    );

    if (!confirmed) {
        return;
    }

    ajaxCall(
        "PUT",
        endpoint,
        null,
        function () {
            user.isAdmin = !user.isAdmin;
            renderUsers(allUsers);
        },
        requestFailed
    );
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

    const filteredUsers = allUsers.filter(
        function (user) {
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
        }
    );

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

    alert(
        "Failed to load admin dashboard data"
    );
}