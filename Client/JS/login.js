let currentForm = 'login';
if (localStorage.getItem("loggedInUser") !== null)
    window.location.href = "index.html";

let addedLanguages = {};

const LEVEL_NAMES = {
    0: "Beginner",
    1: "Intermediate",
    2: "Advanced"
};

$(document).ready(function () {
    $(".register-link a").click(toggleForms);
    $("#login-form .login-btn").click(authenticate);

    $("#register-next-btn").click(registerNext);
    $("#add-language-btn").click(addLanguage);
    $("#back-to-step1-btn").click(backToStep1);
    $("#final-register-btn").click(registerUser);
});

function registerUser() {
    // Final submission: collect basic info + languages + continents
    const name = $("#reg-name").val();
    const email = $("#reg-email").val();
    const password = $("#reg-password").val();

    if (!isValidName(name)) {
        $("#register-step2-error").text("Name must contain only letters and at least 2 characters").show();
        return
    }
    else if (!isValidEmail(email)) {
        $("#register-step2-error").text("Email format is incorrect!").show();
        return
    }
    else if (!isValidPassword(password)) {
        $("#register-step2-error").text("Password must be at least 8 characters, include 1 uppercase letter and 1 number!").show();
        return
    }
    else {
        $("#register-step2-error").text("").hide();
    }

    const selectedContinents = [];
    $("input[name='continent']:checked").each(function () {
        selectedContinents.push($(this).val());
    });

    // Ensure the language-level dictionary contains numbers (defensive)
    const numericLanguegeLevels = Object.fromEntries(
        Object.entries(addedLanguages).map(([k, v]) => [k, Number(v)])
    );

    const user = {
        username: name,
        email: email,
        password: password,
        languegeLevels: numericLanguegeLevels,
        preferdContinents: selectedContinents
    };

    ajaxCall("POST", API_ROUTES.usersApi, JSON.stringify(user),
        function (data) {
            console.log(data);
            window.location.href = "login.html";
        },
        function (err) {
            if (err.status === 409) {
                $("#register-step2-error").text(err.responseText).show();
                return;
            }
            const message = "An error occurred";
            $("#register-step2-error").text(message).show();
        }
    );
}

function authenticate() {
    const email = $("#email").val();
    const password = $("#password").val();

    if (!isValidEmail(email)) {
        $("#login-error").text("Email format is incorrect!").show();
        return
    }
    else if (!isValidPassword(password)) {
        $("#login-error").text("Password must be at least 8 characters, include 1 uppercase letter and 1 number!").show();
        return
    }
    else {
        $("#login-error").hide();
    }

    const LoginInfo = {
        email: email,
        password: password
    };

    ajaxCall("POST", API_ROUTES.usersApi + '/login', JSON.stringify(LoginInfo),
        function (user) {
            localStorage.setItem("loggedInUser", JSON.stringify(user));
            window.location.href = "index.html";
        },
        function () {
            $("#login-error").text("Invalid email or password. Please try again.").show();
        }
    );
}

function toggleForms(event) {
    event.preventDefault();
    const loginSection = document.getElementById('login-section');
    const registerSection = document.getElementById('register-section');

    if (currentForm === 'login') {
        loginSection.classList.add('hidden');
        registerSection.classList.remove('hidden');
        currentForm = 'register';
    } else {
        registerSection.classList.add('hidden');
        loginSection.classList.remove('hidden');
        currentForm = 'login';
    }
}
function isValidName(name) {
    const regex = /^[A-Za-z]{2,}$/;
    return regex.test(name);
}
function isValidEmail(email) {
    const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return regex.test(email);
}
function isValidPassword(password) {
    const regex = /^(?=.*[A-Z])(?=.*\d).{8,}$/;
    return regex.test(password);
}

// Registration step handlers and language management
function registerNext() {
    const name = $("#reg-name").val();
    const email = $("#reg-email").val();
    const password = $("#reg-password").val();

    if (!isValidName(name)) {
        $("#register-error").text("Name must contain only letters and at least 2 characters").show();
        return;
    }
    else if (!isValidEmail(email)) {
        $("#register-error").text("Email format is incorrect!").show();
        return;
    }
    else if (!isValidPassword(password)) {
        $("#register-error").text("Password must be at least 8 characters, include 1 uppercase letter and 1 number!").show();
        return;
    }
    else {
        $("#register-error").text("").hide();
    }

    // Show step 2
    $("#register-step-1").addClass('hidden');
    $("#register-step-2").removeClass('hidden');
}

function backToStep1() {
    $("#register-step-2").addClass('hidden');
    $("#register-step-1").removeClass('hidden');
}

function addLanguage() {
    const lang = $("#lang-select").val();
    const level = parseInt($("#lang-level-select").val(), 10);

    if (Number.isNaN(level) || level < 0) {
        alert('Please select a valid language level');
        return;
    }

    if (addedLanguages.hasOwnProperty(lang)) {
        alert('Language already added');
        return;
    }

    // store numeric level
    addedLanguages[lang] = level;
    renderLanguages();
}

function renderLanguages() {
    const list = $("#languages-list");
    list.empty();
    Object.entries(addedLanguages).forEach(([language, level]) => {
        const li = $("<li></li>");
        li.css({
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            padding: '6px 0',
            borderBottom: '1px solid rgba(255,255,255,0.03)'
        });

        // show readable label while keeping numeric value in the dict
        const levelText = LEVEL_NAMES[level] ?? String(level);
        const left = $(`<div><span>${language}</span> <small style="color:rgba(255,255,255,0.6);margin-left:8px;">(${levelText})</small></div>`);
        const btn = $(`<button type="button" style="background:transparent;border:none;color:var(--accent);font-weight:700;cursor:pointer;">Remove</button>`);
        btn.on('click', () => removeLanguage(language));

        li.append(left);
        li.append(btn);
        list.append(li);
    });
}

function removeLanguage(language) {
    if (!addedLanguages.hasOwnProperty(language)) {
        return;
    }

    delete addedLanguages[language];
    renderLanguages();
}

export function getUserLoggedIn() {
    const userJson = localStorage.getItem("user");

    if (!userJson) {
        return null;
    }

    try {
        return JSON.parse(userJson);
    } catch (error) {
        console.error("Invalid user data in localStorage:", error);
        localStorage.removeItem("user");
        return null;
    }
}

export function isAdmin() {
    return getUserLoggedIn()?.isAdmin === "true";
}

export function logout() {
    localStorage.removeItem("user");
    window.location.href = "../HTML/login.html";
}

