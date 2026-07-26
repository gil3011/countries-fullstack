let countries = [];
let map;
let markers = [];
let markerById = {};

let loggedInUser = null;
let userId = null;
let visitedIds = new Set();
let wishlistIds = new Set();

let filteredCountries = [];
let currentPage = 1;
const PAGE_SIZE = 24;
let activeListFilter = ''; // '', 'visited', or 'wishlist' (logged-in only)
let sortAsc = true;         // sort direction; defaults per field via onSortFieldChange

function initMap() {
    map = L.map('map').setView([20, 0], 2);
    L.tileLayer('https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png').addTo(map);
}

function cssVar(name, fallback) {
    return getComputedStyle(document.documentElement).getPropertyValue(name).trim() || fallback;
}

// Escape text before injecting into innerHTML so a name with <, >, & or quotes
// can't break the markup (defensive -- country data is admin-controlled).
function escapeHtml(value) {
    return String(value == null ? '' : value)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

function showLoading(isLoading) {
    const el = document.getElementById('loading');
    if (el) el.style.display = isLoading ? 'flex' : 'none';
    document.body.classList.toggle('is-loading', isLoading);
}
function cacheCountries(data) {
    try {
        sessionStorage.setItem(COUNTRIES_CACHE_KEY, JSON.stringify({ ts: Date.now(), data }));
    } catch {
        // storage full / disabled -- caching is best-effort
    }
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
    ajaxCall("GET", `${API_ROUTES.userAPI}/${userId}/visited`, null,
        data => { visitedIds = new Set((data || []).map(c => c.id)); update(); }, () => { });
    ajaxCall("GET", `${API_ROUTES.userAPI}/${userId}/wishlist`, null,
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
    const base = `${API_ROUTES.userAPI}/${userId}`;
    const inVisited = visitedIds.has(countryId);
    const inWishlist = wishlistIds.has(countryId);
    const ops = [];

    if (target === 'visited') {
        // moveToVisited removes from wishlist and adds to visited atomically in one call.
        if (inWishlist) ops.push(apiCall("POST", `${base}/moveToVisited/${countryId}`));
        else if (!inVisited) ops.push(apiCall("POST", `${base}/visited/${countryId}`));
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

// Clicking a card flies the map to the country and opens its popup.
// Countries without coordinates fall back to the details page.
function focusCountry(id, lat, lng, cca3) {
    if (lat === 0 && lng === 0) {
        if (cca3) window.location.href = `country.html?cca3=${encodeURIComponent(cca3)}`;
        return;
    }
    map.flyTo([lat, lng], 5);
    const marker = markerById[id];
    if (marker) map.once('moveend', () => marker.openPopup());
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

// Fill the Language and Currency datalists (searchable) from the distinct
// values already present in the loaded /api/Country data.
function populateFilterOptions() {
    const langs = new Set();
    const currs = new Set();
    countries.forEach(c => {
        (c.languages || []).forEach(l => { if (l.languageName) langs.add(l.languageName); });
        (c.currencies || []).forEach(cu => { if (cu.currencyName) currs.add(cu.currencyName); });
    });
    fillDatalist('language-options', [...langs].sort());
    fillDatalist('currency-options', [...currs].sort());
}

function fillDatalist(id, values) {
    const dl = document.getElementById(id);
    if (!dl) return;
    dl.innerHTML = values.map(v => `<option value="${v}"></option>`).join('');
}

function computeFiltered() {
    const txt = document.getElementById('search').value.toLowerCase();
    const reg = document.getElementById('region').value;
    const lang = (document.getElementById('language-filter')?.value || '').toLowerCase().trim();
    const curr = (document.getElementById('currency-filter')?.value || '').toLowerCase().trim();
    const order = document.getElementById('sort').value;

    let filtered = countries.filter(c => {
        const countryName = (c.commonName || "").toLowerCase();
        const countryRegion = c.region || "";
        const countryCode = (c.cca3 || "").toLowerCase();
        const capitalName = (c.capitals && c.capitals[0]?.name || "").toLowerCase();

        if (reg && countryRegion != reg) return false;
        if (txt && !countryName.includes(txt) && !countryCode.includes(txt) && !capitalName.includes(txt)) return false;
        if (lang && !(c.languages || []).some(l => (l.languageName || '').toLowerCase().includes(lang))) return false;
        if (curr && !(c.currencies || []).some(cu => (cu.currencyName || '').toLowerCase().includes(curr))) return false;
        if (userId && activeListFilter === 'visited' && !visitedIds.has(c.id)) return false;
        if (userId && activeListFilter === 'wishlist' && !wishlistIds.has(c.id)) return false;
        return true;
    });

    const dir = sortAsc ? 1 : -1;
    filtered.sort((a, b) => {
        let cmp;
        if (order === 'name') {
            const nameA = a.commonName || a.CommonName || "";
            const nameB = b.commonName || b.CommonName || "";
            cmp = nameA.localeCompare(nameB);
        } else {
            const valA = order === 'pop' ? (a.population || a.Population || 0) : (a.areaKm2 || a.AreaKm2 || 0);
            const valB = order === 'pop' ? (b.population || b.Population || 0) : (b.areaKm2 || b.AreaKm2 || 0);
            cmp = valA - valB;
        }
        return cmp * dir;
    });

    return filtered;
}
// Update country counter

function totalPages() {
    return Math.max(1, Math.ceil(filteredCountries.length / PAGE_SIZE));
}

// Recompute the filtered set and re-render cards (current page) + all map markers.
// Does NOT reset the page, so list toggles keep the user in place; filter changes
// reset the page explicitly via onFilterChange().
function update() {
    filteredCountries = computeFiltered();
    if (currentPage > totalPages()) currentPage = totalPages();
    if (currentPage < 1) currentPage = 1;

    document.getElementById('count').innerText = filteredCountries.length;

    renderCards();
    renderMarkers();
}

function renderCards() {
    const grid = document.getElementById('grid');

    const start = (currentPage - 1) * PAGE_SIZE;
    const pageItems = filteredCountries.slice(start, start + PAGE_SIZE);

    if (!pageItems.length) {
        grid.innerHTML = `<p class="no-results">No countries match your filters.</p>`;
        renderPagination();
        return;
    }

    // Build the whole page of cards once, then assign innerHTML a single time
    // (assigning inside the loop forces a reflow per card).
    grid.innerHTML = pageItems.map(c => {
        const id = c.id;
        const name = escapeHtml(c.commonName || 'Unknown');
        const code = escapeHtml(c.cca3 || '—');
        const cca3 = c.cca3 || '';
        const capital = escapeHtml((c.capitals && c.capitals[0]?.name) || 'None');
        const pop = c.population || 0;
        const area = c.areaKm2 || 0;
        const flag = c.flagUrl || '';
        const lat = c.latitude || 0;
        const lng = c.longitude || 0;

        const userActions = userId ? `
    <div class="card-actions">
        <button onclick="event.stopPropagation(); toggleVisited(${id})" title="${visitedIds.has(id) ? 'Remove from visited' : 'Add to visited'}" class="card-action-btn ${visitedIds.has(id) ? 'active-visited' : ''}">${visitedIds.has(id) ? '✓ Visited' : '+ Visited'}</button>
        <button onclick="event.stopPropagation(); toggleWishlist(${id})" title="${wishlistIds.has(id) ? 'Remove from wishlist' : 'Add to wishlist'}" class="card-action-btn ${wishlistIds.has(id) ? 'active-wishlist' : ''}">${wishlistIds.has(id) ? '★ Wishlist' : '+ Wishlist'}</button>
    </div>` : '';

        const detailsLink = cca3 ? `
    <a href="country.html?cca3=${encodeURIComponent(cca3)}" class="card-details-link" onclick="event.stopPropagation()">View details →</a>` : '';

        return `
<div class="country-card" onclick="focusCountry(${id}, ${lat}, ${lng}, '${cca3}')" title="Show on map">
    <div>
        <img src="${flag}" class="flag-img" alt="${name} flag">
            <h3>${name} (${code})</h3>
            <p>Capital: ${capital}</p>
            <p>Population: ${pop.toLocaleString()}</p>
            <p>Area: ${area.toLocaleString()} km²</p>
    </div>
    ${detailsLink}
    ${userActions}
</div>
`;
    }).join('');

    renderPagination();
}

function renderPagination() {
    const container = document.getElementById('pagination');
    if (!container) return;

    const pages = totalPages();
    if (filteredCountries.length <= PAGE_SIZE) {
        container.innerHTML = '';
        return;
    }

    container.innerHTML =
        `<button class="page-btn" ${currentPage === 1 ? 'disabled' : ''} onclick="goToPage(${currentPage - 1})">‹ Prev</button>` +
        `<span class="page-info">Page ${currentPage} of ${pages}</span>` +
        `<button class="page-btn" ${currentPage === pages ? 'disabled' : ''} onclick="goToPage(${currentPage + 1})">Next ›</button>`;
}

function goToPage(page) {
    if (page < 1 || page > totalPages()) return;
    currentPage = page;
    renderCards();
    document.getElementById('grid').scrollIntoView({ behavior: 'smooth', block: 'start' });
}

function renderMarkers() {
    markers.forEach(m => map.removeLayer(m));
    markers = [];
    markerById = {};

    filteredCountries.forEach(c => {
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
            markerById[id] = m;
        }
    });
}

// Event Listeners

// Filter/sort changes reset back to the first page.
function onFilterChange() {
    currentPage = 1;
    update();
}

// Debounce the text inputs so typing doesn't trigger a full grid + map redraw
// on every keystroke. Dropdowns fire once, so they call onFilterChange directly.
function debounce(fn, wait) {
    let timer;
    return function (...args) {
        clearTimeout(timer);
        timer = setTimeout(() => fn.apply(this, args), wait);
    };
}
const debouncedFilterChange = debounce(onFilterChange, 200);

// Reflect the current direction on the toggle button.
function updateSortDirButton() {
    const btn = document.getElementById('sort-dir');
    if (!btn) return;
    btn.textContent = sortAsc ? '↑' : '↓';
    btn.title = sortAsc ? 'Ascending (click for descending)' : 'Descending (click for ascending)';
    btn.setAttribute('aria-label', btn.title);
}

// Changing the field picks a sensible default direction: names A→Z,
// population/area largest-first. The toggle can still flip it.
function onSortFieldChange() {
    sortAsc = document.getElementById('sort').value === 'name';
    updateSortDirButton();
    onFilterChange();
}

document.getElementById('search').addEventListener('input', debouncedFilterChange);
document.getElementById('region').addEventListener('change', onFilterChange);
document.getElementById('language-filter').addEventListener('input', debouncedFilterChange);
document.getElementById('currency-filter').addEventListener('input', debouncedFilterChange);
document.getElementById('sort').addEventListener('change', onSortFieldChange);

const sortDirBtn = document.getElementById('sort-dir');
if (sortDirBtn) {
    sortDirBtn.addEventListener('click', () => {
        sortAsc = !sortAsc;
        updateSortDirButton();
        onFilterChange();
    });
}

const listFilterEl = document.getElementById('list-filter');
if (listFilterEl) {
    listFilterEl.addEventListener('change', (e) => {
        activeListFilter = e.target.value;
        onFilterChange();
    });
}

document.getElementById('reset').addEventListener('click', () => {
    document.getElementById('search').value = '';
    document.getElementById('region').value = '';
    document.getElementById('language-filter').value = '';
    document.getElementById('currency-filter').value = '';
    document.getElementById('sort').value = 'name';
    sortAsc = true;
    updateSortDirButton();
    if (listFilterEl) listFilterEl.value = '';
    activeListFilter = '';
    currentPage = 1;
    update();
    map.setView([20, 0], 2);
});

function handleSuccess(data) {
    countries = data;
    cacheCountries(data);
    populateFilterOptions();
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

    loggedInUser = JSON.parse(localStorage.getItem("loggedInUser"));
    userId = loggedInUser ? loggedInUser.id : null;

    if (userId) {
        const legend = document.getElementById("map-legend");
        if (legend) legend.style.display = "flex";
        const listFilter = document.getElementById("list-filter");
        if (listFilter) listFilter.style.display = "";
        loadUserLists();
    }

    const cached = getCachedCountries();
    if (cached && cached.length) {
        countries = cached;
        populateFilterOptions();
        showLoading(false);
        update();
    } else {
        showLoading(true);
        ajaxCall("GET", API_ROUTES.countryAPI, null, handleSuccess, handleError);
    }
};