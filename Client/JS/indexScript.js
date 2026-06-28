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

    // סינון המדינות (תיקון שמות השדות בהתאם למבנה ה-API הטיפוסי שלך)
    let filtered = countries.filter(c => {
        const countryName = (c.commonName ||"").toLowerCase();
        const countryRegion = c.region|| "";
        const countryCode = (c.cca3 || "").toLowerCase();
        const capitalName = (c.capitals && c.capitals[0]?.Name || "").toLowerCase();

        if (reg && countryRegion != reg) return false;
        if (txt && !countryName.includes(txt) && !countryCode.includes(txt) && !capitalName.includes(txt)) return false;
        return true;
    });

    // מיון
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

    // עדכון מונה המדינות
    document.getElementById('count').innerText = filtered.length;

    const grid = document.getElementById('grid');
    grid.innerHTML = '';

    // רינדור הכרטיסיות
    filtered.forEach(c => {
        const name = c.commonName || c.CommonName || 'לא ידוע';
        const code = c.cca3 || c.Code || '—';
        const capital = (c.capitals && c.capitals[0]?.Name) || c.capital || 'אין';
        const pop = c.population || c.Population || 0;
        const area = c.areaKm2 || c.AreaKm2 || 0;
        const flag = c.flagUrl || c.FlagUrl || '';
        const lat = c.latitude || c.Latitude || 0;
        const lng = c.longitude || c.Longitude || 0;

        grid.innerHTML += `
<div class="country-card">
    <div>
        <img src="${flag}" class="flag-img" alt="דגל ${name}">
            <h3>${name} (${code})</h3>
            <p>עיר בירה: ${capital}</p>
            <p>אוכלוסייה: ${pop.toLocaleString()}</p>
            <p>שטח: ${area.toLocaleString()} קמ"ר</p>
    </div>
    <button onclick="goTo(${lat}, ${lng})" class="map-btn">הצג במפה</button>
</div>
`;
    });

    // עדכון המרקרים במפה
    markers.forEach(m => map.removeLayer(m));
    markers = [];

    filtered.forEach(c => {
        const name = c.commonName || c.CommonName || '';
        const lat = c.latitude || c.Latitude || 0;
        const lng = c.longitude || c.Longitude || 0;
        const capital = (c.capitals && c.capitals[0]?.Name) || c.capital || 'אין';

        if (lat !== 0 || lng !== 0) {
            const m = L.marker([lat, lng]).addTo(map).bindPopup(`<b>${name}</b><br>בירה: ${capital}`);
            markers.push(m);
        }
    });
}

// האזנה לאירועים
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
    console.log("הנתונים הגיעו מהשרת:", data);
    countries = data;
    update();
}

function handleError(error) {
    console.error("שגיאה במשיכת הנתונים:", error);
    alert("לא ניתן לטעון את המדינות כרגע.");
}

window.onload = () => {
    initMap();
    ajaxCall("GET", "https://localhost:7255/api/Country", null, handleSuccess, handleError);
};
