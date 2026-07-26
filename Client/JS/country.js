let currentCountry = null;

const ShareType = Object.freeze({
    Recommendation: 0,
    Thought: 1,
    Review: 2
});

/* ===================== Create Share Integration ===================== */

/*
    createShare.js calls this function when opening the modal.
    On the country page, the current country is selected automatically.
*/
window.getDefaultShareCountryId = function () {
    return currentCountry?.id || null;
};

/*
    createShare.js calls this function after a share
    is created successfully.
*/
window.onShareCreated = function () {
    if (!currentCountry) {
        return;
    }

    loadCountryShares(currentCountry.commonName);
};

/* ===================== Country Details ===================== */

function renderCountry(country) {
    document.getElementById("cca3-badge").textContent =
        country.cca3 || "";

    const flagElement =
        document.getElementById("country-flag");

    flagElement.src =
        country.flagUrl ||
        "https://via.placeholder.com/150?text=No+Flag";

    flagElement.alt =
        "Flag of " + (country.commonName || "country");

    document.getElementById("country-name").textContent =
        country.commonName || "Unknown country";

    document.getElementById("official-name").textContent =
        country.officialName ||
        country.commonName ||
        "Unknown";

    document.getElementById("region").textContent =
        country.region || "N/A";

    document.getElementById("subregion").textContent =
        country.subregion || "N/A";

    document.getElementById("population").textContent =
        country.population != null
            ? country.population.toLocaleString()
            : "N/A";

    document.getElementById("area").textContent =
        country.areaKm2 != null
            ? country.areaKm2.toLocaleString() + " km²"
            : "N/A";

    document.getElementById("capitals").textContent =
        (country.capitals || [])
            .map(capital => capital.name)
            .join(", ") || "None";

    document.getElementById("currency").textContent =
        (country.currencies || [])
            .map(currency => {
                const symbol =
                    currency.currencySymbol || "";

                const code =
                    currency.currencyCode || "";

                if (symbol && code) {
                    return `${symbol} (${code})`;
                }

                return symbol || code;
            })
            .filter(Boolean)
            .join(", ") || "N/A";

    renderLanguages(country.languages || []);

    document.getElementById("landlocked-status").textContent =
        country.isLandlocked ? "Yes" : "No";

    document.getElementById("coordinates").textContent =
        country.latitude != null &&
            country.longitude != null
            ? `${country.latitude}°, ${country.longitude}°`
            : "N/A";

    renderCountryMap(
        country.latitude,
        country.longitude
    );

    document.getElementById("timezones").textContent =
        (country.timezones || []).join(", ") || "N/A";

    document.getElementById("wiki-link").href =
        country.wikipediaUrl || "#";

    
    document.getElementById("country-quizzes-link").href = 
        `user_quiz.html?tab=explore-section&countryId=${currentCountry?.id}`;

    renderBorderingCountries(country.borders || []);

    document
        .getElementById("loading")
        .classList.add("hidden");

    document
        .getElementById("content")
        .classList.remove("hidden");
}

function renderLanguages(languages) {
    const languagesContainer =
        document.getElementById("languages-tags");

    languagesContainer.innerHTML = "";

    if (languages.length === 0) {
        languagesContainer.innerHTML =
            '<span class="muted">No languages available.</span>';

        return;
    }

    languages.forEach(language => {
        const languageElement =
            document.createElement("span");

        languageElement.className = "lang";

        const languageName =
            language.languageName || "Unknown";

        const languageCode =
            (language.iso639_1 || "").toUpperCase();

        languageElement.textContent =
            languageCode
                ? `${languageName} (${languageCode})`
                : languageName;

        languagesContainer.appendChild(
            languageElement
        );
    });
}

function renderCountryMap(latitude, longitude) {
    const mapElement =
        document.getElementById("country-map");

    if (
        latitude == null ||
        longitude == null
    ) {
        mapElement.style.display = "none";
        return;
    }

    mapElement.style.display = "block";

    const boundingBox =
        `${longitude - 5},` +
        `${latitude - 5},` +
        `${longitude + 5},` +
        `${latitude + 5}`;

    mapElement.src =
        "https://www.openstreetmap.org/export/embed.html" +
        `?bbox=${boundingBox}` +
        "&layer=mapnik" +
        `&marker=${latitude},${longitude}`;
}

function renderBorderingCountries(borders) {
    const bordersList =
        document.getElementById("borders-list");

    bordersList.innerHTML = "";

    if (borders.length === 0) {
        bordersList.innerHTML =
            '<li class="muted">No shared land borders.</li>';

        return;
    }

    borders.forEach(border => {
        const listItem =
            document.createElement("li");

        listItem.className = "border-item";

        const countryName =
            document.createElement("span");

        countryName.style.fontWeight = "600";
        countryName.textContent =
            border.commonName || "Unknown";

        const countryLink =
            document.createElement("a");

        countryLink.href =
            "?cca3=" +
            encodeURIComponent(border.cca3 || "");

        countryLink.style.textDecoration = "none";
        countryLink.style.color = "var(--accent)";
        countryLink.style.fontWeight = "700";

        countryLink.textContent =
            border.cca3 || "";

        listItem.appendChild(countryName);
        listItem.appendChild(countryLink);

        bordersList.appendChild(listItem);
    });
}

function showError(message) {
    document
        .getElementById("loading")
        .classList.add("hidden");

    const errorElement =
        document.getElementById("error");

    errorElement.classList.remove("hidden");

    if (message) {
        document
            .getElementById("error-message")
            .textContent = message;
    }
}

function handleCountrySuccess(country) {
    currentCountry = country;

    document
        .querySelector(".shares-section")
        .classList.remove("hidden");

    renderCountry(country);
    loadCountryShares(country.commonName);
}

function handleCountryError(error) {
    showError("Internal database error.");

    console.error(
        "Failed to load country:",
        error
    );
}

/* ===================== Country Shares ===================== */

function loadCountryShares(countryName) {
    resetSharesSection();

    if (!countryName) {
        handleSharesError(
            new Error("Country name is missing.")
        );

        return;
    }

    const url =
        API_ROUTES.shareAPI +
        "/GetCountryShares/" +
        encodeURIComponent(countryName);

    ajaxCall(
        "GET",
        url,
        null,
        handleSharesSuccess,
        handleSharesError
    );
}

function resetSharesSection() {
    document
        .getElementById("shares-loading")
        .classList.remove("hidden");

    document
        .getElementById("shares-empty")
        .classList.add("hidden");

    document
        .getElementById("shares-error")
        .classList.add("hidden");

    document
        .getElementById("shares-list")
        .innerHTML = "";

    updateSharesCount(0);
}

function handleSharesSuccess(shares) {
    const sharesLoading =
        document.getElementById("shares-loading");

    const sharesEmpty =
        document.getElementById("shares-empty");

    const sharesError =
        document.getElementById("shares-error");

    const sharesList =
        document.getElementById("shares-list");

    sharesLoading.classList.add("hidden");
    sharesEmpty.classList.add("hidden");
    sharesError.classList.add("hidden");

    sharesList.innerHTML = "";

    const countryShares =
        Array.isArray(shares)
            ? shares
            : [];

    updateSharesCount(countryShares.length);

    if (countryShares.length === 0) {
        sharesEmpty.classList.remove("hidden");
        return;
    }

    countryShares.forEach(share => {
        sharesList.appendChild(
            createShareCard(share)
        );
    });
}

function getShareTypeName(type) {
    switch (Number(type)) {
        case ShareType.Recommendation:
            return "Recommendation";

        case ShareType.Thought:
            return "Thought";

        case ShareType.Review:
            return "Review";

        default:
            return "Unknown";
    }
}

function getShareTypeClass(type) {
    switch (Number(type)) {
        case ShareType.Recommendation:
            return "share-type-recommendation";

        case ShareType.Thought:
            return "share-type-thought";

        case ShareType.Review:
            return "share-type-review";

        default:
            return "share-type-unknown";
    }
}

function getDefaultShareTitle(type) {
    switch (Number(type)) {
        case ShareType.Recommendation:
            return "Country recommendation";

        case ShareType.Thought:
            return "Thought about this country";

        case ShareType.Review:
            return "Country review";

        default:
            return "Country share";
    }
}

function createShareCard(share) {
    const card =
        document.createElement("article");

    card.className = "share-card";

    const formattedDate =
        formatShareDate(share.createdAt);

    const username =
        share.userName ||
        share.username ||
        "Anonymous user";

    const typeName =
        getShareTypeName(share.type);

    const typeClass =
        getShareTypeClass(share.type);

    card.innerHTML = `
        <div class="share-top">
            <h3 class="share-title"></h3>
            <span class="share-type"></span>
        </div>

        <p class="share-description"></p>

        <div class="share-footer">
            <span class="share-user"></span>
            <span class="share-date"></span>
        </div>
    `;

    card
        .querySelector(".share-title")
        .textContent =
        share.title ||
        getDefaultShareTitle(share.type);

    card
        .querySelector(".share-description")
        .textContent =
        share.description ||
        "No description was provided.";

    const typeElement =
        card.querySelector(".share-type");

    typeElement.textContent = typeName;
    typeElement.classList.add(typeClass);

    card
        .querySelector(".share-user")
        .textContent =
        "Shared by " + username;

    card
        .querySelector(".share-date")
        .textContent = formattedDate;

    return card;
}

function formatShareDate(createdAt) {
    if (!createdAt) {
        return "Unknown date";
    }

    const date =
        new Date(createdAt);

    if (Number.isNaN(date.getTime())) {
        return "Unknown date";
    }

    return date.toLocaleDateString("en-GB");
}

function updateSharesCount(count) {
    const countElement =
        document.getElementById("shares-count");

    countElement.textContent =
        count === 1
            ? "1 share"
            : `${count} shares`;
}

function handleSharesError(error) {
    document
        .getElementById("shares-loading")
        .classList.add("hidden");

    document
        .getElementById("shares-empty")
        .classList.add("hidden");

    document
        .getElementById("shares-error")
        .classList.remove("hidden");

    document
        .getElementById("shares-list")
        .innerHTML = "";

    updateSharesCount(0);

    console.error(
        "Failed to load country shares:",
        error
    );
}

/* ===================== Page Initialization ===================== */

$(document).ready(function () {
    const cca3 =
        new URLSearchParams(
            window.location.search
        ).get("cca3");

    if (!cca3) {
        showError("Missing URL parameter.");
        return;
    }

    const url =
        API_ROUTES.countryAPI +
        "/GetByCca3/" + cca3;

    ajaxCall(
        "GET",
        url,
        null,
        handleCountrySuccess,
        handleCountryError
    );
});