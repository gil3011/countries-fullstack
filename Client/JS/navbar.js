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
    updateWelcomeMessage()
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

function logout() {
    localStorage.removeItem("loggedInUser");
    window.location.href = "../HTML/login.html";
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

    const loggedInUser = JSON.parse(
        localStorage.getItem("loggedInUser")
    );

    if (loggedInUser && loggedInUser.username) {
        welcomeElement.textContent = `Welcome ${loggedInUser.username}!`;
    } else {
        welcomeElement.textContent = "Welcome Guest!";
    }
}