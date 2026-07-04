let countries = [];
let map;
let markers = [];

function initMap() {
    map = L.map('map').setView([20, 0], 2);
    L.tileLayer('https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png').addTo(map);
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
        const name = c.commonName || 'Unknown';
        const code = c.cca3 || '—';
        const capital = (c.capitals && c.capitals[0]?.name) || 'None';
        const pop = c.population || 0;
        const area = c.areaKm2 || 0;
        const flag = c.flagUrl || '';
        const lat = c.latitude || 0;
        const lng = c.longitude || 0;

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
    </div>
</div>
`;
    });

    // Update map markers
    markers.forEach(m => map.removeLayer(m));
    markers = [];

    filtered.forEach(c => {
        const name = c.commonName || c.CommonName || '';
        const lat = c.latitude || c.Latitude || 0;
        const lng = c.longitude || c.Longitude || 0;
        const capital = (c.capitals && c.capitals[0]?.name) || 'None';
        const cca3 = c.cca3 || '';

        if (lat !== 0 || lng !== 0) {
            const popupHtml = `<b>${name}</b><br>Capital: ${capital}<br><a href="country.html?cca3=${encodeURIComponent(cca3)}" target="_blank">View details</a>`;
            const m = L.marker([lat, lng]).addTo(map).bindPopup(popupHtml);
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
    update();
}

function handleError(error) {
    console.error("Error fetching data:", error);
    alert("Cannot load countries at this time.");
}

window.onload = () => {
    initMap();
    ajaxCall("GET", "https://localhost:7255/api/Country", null, handleSuccess, handleError);
};