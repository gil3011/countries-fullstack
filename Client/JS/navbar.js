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
    const currentPage = window.location.pathname.split("/").pop().split("?")[0];
    const currentSearch = window.location.search;
    const urlParams = new URLSearchParams(currentSearch);
    let currentTab = urlParams.get('tab');
    
    if (currentPage === "user_quiz.html" && !currentTab) {
        currentTab = "explore-section";
    }

    const navLinks = document.querySelectorAll(".nav-links a");

    navLinks.forEach(link => {
        let href = link.getAttribute("href");
        if (!href) return;
        
        const hrefParts = href.split("/");
        const fullPage = hrefParts.pop();
        const linkPage = fullPage.split("?")[0];
        const linkParams = new URLSearchParams(fullPage.split("?")[1] || "");
        const linkTab = linkParams.get('tab');

        let isActive = false;

        // If it's the exact same page
        if (linkPage === currentPage) {
            // If the link has a tab parameter, only activate if it matches the current tab
            if (linkTab) {
                if (linkTab === currentTab) {
                    isActive = true;
                }
            } else {
                // If it doesn't have a tab param, it's just a normal page link
                isActive = true;
            }
        }

        if (isActive) {
            // Also add active to parent dropdown button if it's in a dropdown
            if (link.closest('.nav-dropdown-content')) {
                const dropBtn = link.closest('.nav-dropdown').querySelector('.dropbtn');
                if (dropBtn) dropBtn.classList.add('active');
            } else {
                link.classList.add("active");
            }
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
    const admin = getUserLoggedIn()?.isAdmin;
    return admin === true || admin === "true";
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