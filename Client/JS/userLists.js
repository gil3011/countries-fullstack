// Shared helpers for a user's Visited / Wishlist country lists.
// Used by the dashboard (index) and the country detail page so the membership
// rules live in one place. Requires apiRoutes.js and ajaxCalls.js to be loaded first.

// Wrap a jQuery ajax call in a promise.
function userListsApiCall(method, url) {
    return new Promise((resolve, reject) => {
        ajaxCall(method, url, null, resolve, reject);
    });
}

// Fetch the country arrays for a user.
function fetchVisitedCountries(userId) {
    return userListsApiCall("GET", `${API_ROUTES.userAPI}/${userId}/visited`);
}

function fetchWishlistCountries(userId) {
    return userListsApiCall("GET", `${API_ROUTES.userAPI}/${userId}/wishlist`);
}

// Move `countryId` into `target` ('visited' | 'wishlist' | 'none') given its
// current membership. A country can be in at most one list, so setting one
// removes it from the other. Returns a promise for all the mutations.
function persistCountryMembership(userId, countryId, target, inVisited, inWishlist) {
    const base = `${API_ROUTES.userAPI}/${userId}`;
    const ops = [];

    if (target === 'visited') {
        // moveToVisited removes from wishlist and adds to visited in one call.
        if (inWishlist) ops.push(userListsApiCall("POST", `${base}/moveToVisited/${countryId}`));
        else if (!inVisited) ops.push(userListsApiCall("POST", `${base}/visited/${countryId}`));
    } else if (target === 'wishlist') {
        if (inVisited) ops.push(userListsApiCall("DELETE", `${base}/visited/${countryId}`));
        if (!inWishlist) ops.push(userListsApiCall("POST", `${base}/wishlist/${countryId}`));
    } else { // 'none'
        if (inVisited) ops.push(userListsApiCall("DELETE", `${base}/visited/${countryId}`));
        if (inWishlist) ops.push(userListsApiCall("DELETE", `${base}/wishlist/${countryId}`));
    }

    return Promise.all(ops);
}
