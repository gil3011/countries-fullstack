// Client-side route guard: only logged-in users may view protected pages.
// Include this as the FIRST script in the <head> of every page except login.html.
// Runs immediately (before the rest of the page) and redirects guests to login.
(function () {
    var raw = localStorage.getItem("loggedInUser");
    var loggedIn = false;

    if (raw) {
        try {
            loggedIn = !!JSON.parse(raw);
        } catch (e) {
            // Corrupt value -- treat as logged out and clean it up.
            localStorage.removeItem("loggedInUser");
            loggedIn = false;
        }
    }

    if (!loggedIn) {
        // replace() so the protected page isn't left in the back-button history.
        window.location.replace("login.html");
    }
})();
