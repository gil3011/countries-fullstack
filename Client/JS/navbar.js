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