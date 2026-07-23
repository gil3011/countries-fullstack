let countries = [];
let map;
let markers = [];

let loggedInUser = null;
let userId = null;
let visitedIds = new Set();
let wishlistIds = new Set();

function initMap() {
    map = L.map('map').setView([20, 0], 2);
    L.tileLayer('https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png').addTo(map);
}

function cssVar(name, fallback) {
    return getComputedStyle(document.documentElement).getPropertyValue(name).trim() || fallback;
}

function showLoading(isLoading) {
    const el = document.getElementById('loading');
    if (el) el.style.display = isLoading ? 'flex' : 'none';
    document.body.classList.toggle('is-loading', isLoading);
}

function statusColor(id) {
    if (visitedIds.has(id)) return cssVar('--visited', '#22c55e');
    if (wishlistIds.has(id)) return cssVar('--wishlist', '#f59e0b');
    return cssVar('--accent', '#60a5fa');
}

function pinIcon(color) {
    return L.divIcon({
        className: 'status-pin',
        html: `<span class="status-pin-dot" style="background:${color}"></span>`,
        iconSize: [16, 16],
        iconAnchor: [8, 8],
        popupAnchor: [0, -8]
    });
}

// Load the logged-in user's visited/wishlist membership, then re-render.
function loadUserLists() {
    if (!userId) return;
    ajaxCall("GET", `${API_ROUTES.usersApi}/${userId}/visited`, null,
        data => { visitedIds = new Set((data || []).map(c => c.id)); update(); }, () => { });
    ajaxCall("GET", `${API_ROUTES.usersApi}/${userId}/wishlist`, null,
        data => { wishlistIds = new Set((data || []).map(c => c.id)); update(); }, () => { });
}

function apiCall(method, url) {
    return new Promise((resolve, reject) => {
        ajaxCall(method, url, null, resolve, reject);
    });
}

// A country can be in at most one list. Setting a target list removes it from the other.
function setMembership(countryId, target) {
    if (!userId) return;
    const base = `${API_ROUTES.usersApi}/${userId}`;
    const inVisited = visitedIds.has(countryId);
    const inWishlist = wishlistIds.has(countryId);
    const ops = [];

    if (target === 'visited') {
        if (inWishlist) ops.push(apiCall("DELETE", `${base}/wishlist/${countryId}`));
        if (!inVisited) ops.push(apiCall("POST", `${base}/visited/${countryId}`));
    } else if (target === 'wishlist') {
        if (inVisited) ops.push(apiCall("DELETE", `${base}/visited/${countryId}`));
        if (!inWishlist) ops.push(apiCall("POST", `${base}/wishlist/${countryId}`));
    } else { // 'none'
        if (inVisited) ops.push(apiCall("DELETE", `${base}/visited/${countryId}`));
        if (inWishlist) ops.push(apiCall("DELETE", `${base}/wishlist/${countryId}`));
    }

    if (!ops.length) return;
    Promise.all(ops).then(() => {
        visitedIds.delete(countryId);
        wishlistIds.delete(countryId);
        if (target === 'visited') visitedIds.add(countryId);
        else if (target === 'wishlist') wishlistIds.add(countryId);
        update();
    }).catch(() => alert("Could not update your lists. Please try again."));
}

// Clicking a list button toggles that list off, or moves the country into it.
function toggleVisited(countryId) {
    setMembership(countryId, visitedIds.has(countryId) ? 'none' : 'visited');
}

function toggleWishlist(countryId) {
    setMembership(countryId, wishlistIds.has(countryId) ? 'none' : 'wishlist');
}

function goTo(lat, lng) {
    map.flyTo([lat, lng], 5);
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

function update() {
    const txt = document.getElementById('search').value.toLowerCase();
    const reg = document.getElementById('region').value;
    const order = document.getElementById('sort').value;

    let filtered = countries.filter(c => {
        const countryName = (c.commonName || "").toLowerCase();
        const countryRegion = c.region || "";
        const countryCode = (c.cca3 || "").toLowerCase();
        const capitalName = (c.capitals && c.capitals[0]?.Name || "").toLowerCase();

        if (reg && countryRegion != reg) return false;
        if (txt && !countryName.includes(txt) && !countryCode.includes(txt) && !capitalName.includes(txt)) return false;
        return true;
    });

    // Sorting
    filtered.sort((a, b) => {
        if (order === 'name') {
            const nameA = a.commonName || a.CommonName || "";
            const nameB = b.commonName || b.CommonName || "";
            return nameA.localeCompare(nameB);
        }

        const valA = order === 'pop' ? (a.population || a.Population || 0) : (a.areaKm2 || a.AreaKm2 || 0);
        const valB = order === 'pop' ? (b.population || b.Population || 0) : (b.areaKm2 || b.AreaKm2 || 0);
        return valB - valA;
    });

    // Update country counter
    document.getElementById('count').innerText = filtered.length;

    const grid = document.getElementById('grid');
    grid.innerHTML = '';

    // Render cards
    filtered.forEach(c => {
        const id = c.id;
        const name = c.commonName || 'Unknown';
        const code = c.cca3 || '—';
        const capital = (c.capitals && c.capitals[0]?.name) || 'None';
        const pop = c.population || 0;
        const area = c.areaKm2 || 0;
        const flag = c.flagUrl || '';
        const lat = c.latitude || 0;
        const lng = c.longitude || 0;

        const userActions = userId ? `
    <div class="card-actions">
        <button onclick="toggleVisited(${id})" title="${visitedIds.has(id) ? 'Remove from visited' : 'Add to visited'}" class="card-action-btn ${visitedIds.has(id) ? 'active-visited' : ''}">${visitedIds.has(id) ? '✓ Visited' : '+ Visited'}</button>
        <button onclick="toggleWishlist(${id})" title="${wishlistIds.has(id) ? 'Remove from wishlist' : 'Add to wishlist'}" class="card-action-btn ${wishlistIds.has(id) ? 'active-wishlist' : ''}">${wishlistIds.has(id) ? '★ Wishlist' : '+ Wishlist'}</button>
    </div>` : '';

        grid.innerHTML += `
<div class="country-card">
    <div>
        <img src="${flag}" class="flag-img" alt="${name} flag">
            <h3>${name} (${code})</h3>
            <p>Capital: ${capital}</p>
            <p>Population: ${pop.toLocaleString()}</p>
            <p>Area: ${area.toLocaleString()} km²</p>
    </div>
    <div style="display:flex;gap:.5rem;flex-direction:column">
        <button onclick="goTo(${lat}, ${lng})" class="map-btn">Show on Map</button>
        <button onclick="window.location.href='country.html?cca3=${encodeURIComponent(code)}'" class="map-btn">View Details</button>
        ${userActions}
    </div>
</div>
`;
    });

    // Update map markers
    markers.forEach(m => map.removeLayer(m));
    markers = [];

    filtered.forEach(c => {
        const id = c.id;
        const name = c.commonName || c.CommonName || '';
        const lat = c.latitude || c.Latitude || 0;
        const lng = c.longitude || c.Longitude || 0;
        const capital = (c.capitals && c.capitals[0]?.name) || 'None';
        const cca3 = c.cca3 || '';

        if (lat !== 0 || lng !== 0) {
            const flag = c.flagUrl || '';
            const flagImg = flag ? `<img src="${flag}" class="popup-flag" alt="${name} flag">` : '';
            const popupActions = userId ? `
                <div class="popup-actions">
                    <button onclick="toggleVisited(${id})" title="${visitedIds.has(id) ? 'Remove from visited' : 'Add to visited'}" class="popup-btn ${visitedIds.has(id) ? 'active-visited' : ''}">${visitedIds.has(id) ? '✓ Visited' : '+ Visited'}</button>
                    <button onclick="toggleWishlist(${id})" title="${wishlistIds.has(id) ? 'Remove from wishlist' : 'Add to wishlist'}" class="popup-btn ${wishlistIds.has(id) ? 'active-wishlist' : ''}">${wishlistIds.has(id) ? '★ Wishlist' : '+ Wishlist'}</button>
                </div>` : '';
            const popupHtml = `
                <div class="map-popup">
                    ${flagImg}
                    <div class="popup-title">${name}</div>
                    <div class="popup-sub">Capital: ${capital}</div>
                    <a class="popup-link" href="country.html?cca3=${encodeURIComponent(cca3)}" target="_blank">View details</a>
                    ${popupActions}
                </div>`;
            const m = L.marker([lat, lng], { icon: pinIcon(statusColor(id)) })
                .addTo(map)
                .bindPopup(popupHtml, { minWidth: 200, maxWidth: 240 });
            markers.push(m);
        }
    });
}

// Event Listeners
document.getElementById('search').addEventListener('input', update);
document.getElementById('region').addEventListener('change', update);
document.getElementById('sort').addEventListener('change', update);

document.getElementById('reset').addEventListener('click', () => {
    document.getElementById('search').value = '';
    document.getElementById('region').value = '';
    document.getElementById('sort').value = 'name';
    update();
    map.setView([20, 0], 2);
});

function handleSuccess(data) {
    console.log("Data received from server:", data);
    countries = data;
    showLoading(false);
    update();
}

function handleError(error) {
    console.error("Error fetching data:", error);
    showLoading(false);
    alert("Cannot load countries at this time.");
}

window.onload = () => {
    initMap();
    showLoading(true);

    loggedInUser = JSON.parse(localStorage.getItem("loggedInUser"));
    userId = loggedInUser ? loggedInUser.id : null;

    if (userId) {
        const legend = document.getElementById("map-legend");
        if (legend) legend.style.display = "flex";
        loadUserLists();
    }

    ajaxCall("GET", API_ROUTES.countryApi, null, handleSuccess, handleError);
};