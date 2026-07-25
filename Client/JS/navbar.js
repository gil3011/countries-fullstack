document.addEventListener("DOMContentLoaded", loadNavbar);

async function loadNavbar() {
    try {
        const response = await fetch("../Pages/navbar.html");

        if (!response.ok) {
            throw new Error("Failed to load navbar");
        }

        const navbarHtml = await response.text();

        document.getElementById("navbar-container").innerHTML = navbarHtml;

        initializeNavbar();
    } catch (error) {
        console.error("Navbar error:", error);
    }
    updateWelcomeMessage();
    updateNavbar();
}
function initializeNavbar() {
    const logoutButton = document.getElementById("logout-btn");
    const usernameElement = document.getElementById("nav-username");

    const loggedInUser = JSON.parse(
        localStorage.getItem("loggedInUser")
    );

    if (loggedInUser && usernameElement) {
        usernameElement.textContent = loggedInUser.username;
    }

    if (logoutButton) {
        logoutButton.addEventListener("click", logout);
    }

    markActivePage();
}

function updateNavbar() {
    const loginLink = document.getElementById("login-link");
    const logoutButton = document.getElementById("logout-btn");
    const adminLink = document.getElementById("admin-link");

    const loggedIn = getUserLoggedIn();
    const admin = isAdmin();

    if (loginLink) {
        loginLink.style.display = loggedIn ? "none" : "inline-flex";
    }

    if (logoutButton) {
        logoutButton.style.display = loggedIn ? "inline-flex" : "none";
    }

    if (adminLink) {
        adminLink.style.display = admin ? "inline-flex" : "none";
    }
}

function markActivePage() {
    const currentPage = window.location.pathname
        .split("/")
        .pop();

    const navLinks = document.querySelectorAll(".nav-links a");

    navLinks.forEach(link => {
        const linkPage = link.getAttribute("href")
            .split("/")
            .pop();

        if (linkPage === currentPage) {
            link.classList.add("active");
        }
    });
}

function updateWelcomeMessage() {
    const welcomeElement = document.getElementById("welcome-user");

    const loggedInUser = getUserLoggedIn();

    if (loggedInUser && loggedInUser.username) {
        welcomeElement.textContent = `Welcome ${loggedInUser.username}!`;
    } else {
        welcomeElement.textContent = "Welcome Guest!";
    }
}

function getUserLoggedIn() {
    const userJson = localStorage.getItem("loggedInUser");

    if (!userJson) {
        return null;
    }

    try {
        return JSON.parse(userJson);
    } catch (error) {
        console.error("Invalid user data in localStorage:", error);
        localStorage.removeItem("loggedInUser");
        return null;
    }
}

function isAdmin() {
    return getUserLoggedIn()?.isAdmin === "true";
}

function logout() {
    localStorage.removeItem("loggedInUser");
    window.location.href = "../Pages/login.html";
}


// caching countries
const COUNTRIES_CACHE_KEY = "countriesSummaryV2";
const COUNTRIES_CACHE_TTL = 15 * 60 * 1000; // 15 minutes

window.getCachedCountries = function () {

    try {
        const raw = sessionStorage.getItem(COUNTRIES_CACHE_KEY);
        if (!raw) return null;
        const cached = JSON.parse(raw);
        if (!cached || !Array.isArray(cached.data)) return null;
        if (Date.now() - cached.ts > COUNTRIES_CACHE_TTL) return null;
        return cached.data;
    } catch {
        return null;
    }
}



//window.getCachedCountries = function () {
//    try {
//        console.log("COUNTRIES_CACHE_KEY:", COUNTRIES_CACHE_KEY);
//        console.log("COUNTRIES_CACHE_TTL:", COUNTRIES_CACHE_TTL);

//        const raw = sessionStorage.getItem(COUNTRIES_CACHE_KEY);
//        console.log("Raw value:", raw);

//        if (!raw) return null;

//        const cached = JSON.parse(raw);
//        console.log("Parsed cache:", cached);

//        if (!cached || !Array.isArray(cached.data)) {
//            console.warn("Cache structure is invalid:", cached);
//            return null;
//        }

//        if (Date.now() - cached.ts > COUNTRIES_CACHE_TTL) {
//            console.warn("Cache expired");
//            return null;
//        }

//        return cached.data;
//    } catch (error) {
//        console.error("Error inside getCachedCountries:", error);
//        return null;
//    }
//};