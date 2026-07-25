let allShares = [];
let filteredShares = [];

const ShareType = Object.freeze({
    Recommendation: 0,
    Thought: 1,
    Review: 2
});

$(document).ready(function () {
    bindShareEvents();
    loadAllShares();
});

/*
    createShare.js calls this function
    after a new share is created successfully.
*/
window.onShareCreated = function () {
    loadAllShares();
};

function bindShareEvents() {
    document
        .getElementById("share-search")
        .addEventListener("input", applyFilters);

    document
        .getElementById("share-type-filter")
        .addEventListener("change", applyFilters);

    document
        .getElementById("share-sort")
        .addEventListener("change", applyFilters);

    document
        .getElementById("clear-share-filters")
        .addEventListener("click", clearFilters);

    document
        .getElementById("retry-shares-btn")
        .addEventListener("click", loadAllShares);
}

function loadAllShares() {
    showLoadingState();

    ajaxCall(
        "GET",
        API_ROUTES.shareAPI + "/ReadAllShares",
        null,
        handleSharesSuccess,
        handleSharesError
    );
}

function handleSharesSuccess(shares) {
    allShares = Array.isArray(shares)
        ? shares
        : [];

    hideAllStates();

    applyFilters();
}

function handleSharesError(error) {
    console.error("Failed to load shares:", error);

    allShares = [];
    filteredShares = [];

    hideAllStates();

    document
        .getElementById("shares-error")
        .classList.remove("hidden");

    updateTotalSharesCount(0);
}

function applyFilters() {
    const searchValue =
        document
            .getElementById("share-search")
            .value
            .trim()
            .toLowerCase();

    const selectedType =
        document
            .getElementById("share-type-filter")
            .value;

    const selectedSort =
        document
            .getElementById("share-sort")
            .value;

    filteredShares = allShares.filter(share => {
        const matchesSearch =
            matchesShareSearch(share, searchValue);

        const matchesType =
            selectedType === "all" ||
            Number(share.type) === Number(selectedType);

        return matchesSearch && matchesType;
    });

    sortShares(filteredShares, selectedSort);

    renderShares(filteredShares);
}

function matchesShareSearch(share, searchValue) {
    if (!searchValue) {
        return true;
    }

    const searchableValues = [
        share.title,
        share.description,
        share.userName,
        share.username,
        share.countryName,
        getShareTypeName(Number(share.type))
    ];

    return searchableValues.some(value =>
        String(value || "")
            .toLowerCase()
            .includes(searchValue)
    );
}

function sortShares(shares, sortOption) {
    switch (sortOption) {
        case "oldest":
            shares.sort((firstShare, secondShare) =>
                getShareDateValue(firstShare.createdAt) -
                getShareDateValue(secondShare.createdAt)
            );
            break;

        case "country":
            shares.sort((firstShare, secondShare) =>
                getCountryName(firstShare).localeCompare(
                    getCountryName(secondShare),
                    "en",
                    {
                        sensitivity: "base"
                    }
                )
            );
            break;

        case "newest":
        default:
            shares.sort((firstShare, secondShare) =>
                getShareDateValue(secondShare.createdAt) -
                getShareDateValue(firstShare.createdAt)
            );
            break;
    }
}

function renderShares(shares) {
    const sharesList =
        document.getElementById("shares-list");

    sharesList.innerHTML = "";

    document
        .getElementById("shares-error")
        .classList.add("hidden");

    document
        .getElementById("shares-loading")
        .classList.add("hidden");

    updateTotalSharesCount(shares.length);

    if (shares.length === 0) {
        document
            .getElementById("shares-empty")
            .classList.remove("hidden");

        return;
    }

    document
        .getElementById("shares-empty")
        .classList.add("hidden");

    shares.forEach(share => {
        sharesList.appendChild(
            createShareCard(share)
        );
    });
}

function createShareCard(share) {
    const card =
        document.createElement("article");

    card.className = "share-card";

    const shareType =
        Number(share.type);

    const typeName =
        getShareTypeName(shareType);

    const typeClass =
        getShareTypeClass(shareType);

    const countryName =
        getCountryName(share);

    const userName =
        getShareUserName(share);

    const formattedDate =
        formatShareDate(share.createdAt);

    card.innerHTML = `
    <div class="share-card-header">
            <h2 class="share-card-title"></h2>
            <span class="share-type"></span>
        </div >

        <a class="share-country"></a>

        <p class="share-description"></p>

        <div class="share-card-footer">
            <span class="share-user"></span>
            <span class="share-date"></span>
        </div>
`;

    card
        .querySelector(".share-card-title")
        .textContent =
        share.title ||
        getDefaultShareTitle(shareType);

    const typeElement =
        card.querySelector(".share-type");

    typeElement.textContent = typeName;
    typeElement.classList.add(typeClass);

    const countryLink =
        card.querySelector(".share-country");

    countryLink.textContent =
        "🌍 " + countryName;

    setCountryLink(
        countryLink,
        share
    );

    card
        .querySelector(".share-description")
        .textContent =
        share.description ||
        "No description was provided.";

    card
        .querySelector(".share-user")
        .textContent =
        "Shared by " + userName;

    card
        .querySelector(".share-date")
        .textContent = formattedDate;

    return card;
}

function setCountryLink(countryLink, share) {
    const cca3 =
        share.cca3 ||
        share.countryCca3 ||
        share.countryCode;

    if (cca3) {
        countryLink.href =
            "country.html?cca3=" +
            encodeURIComponent(cca3);

        return;
    }

    countryLink.href = "#";

    countryLink.addEventListener(
        "click",
        function (event) {
            event.preventDefault();
        }
    );
}

function getShareTypeName(type) {
    switch (type) {
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
    switch (type) {
        case ShareType.Recommendation:
            return "share-type-recommendation";

        case ShareType.Thought:
            return "share-type-thought";

        case ShareType.Review:
            return "share-type-review";

        default:
            return "";
    }
}

function getDefaultShareTitle(type) {
    switch (type) {
        case ShareType.Recommendation:
            return "Country Recommendation";

        case ShareType.Thought:
            return "Thought About a Country";

        case ShareType.Review:
            return "Country Review";

        default:
            return "Country Share";
    }
}

function getCountryName(share) {
    return (
        share.countryName ||
        share.commonName ||
        "Unknown Country"
    );
}

function getShareUserName(share) {
    return (
        share.userName ||
        share.username ||
        share.UserName ||
        "Anonymous User"
    );
}

function getShareDateValue(createdAt) {
    if (!createdAt) {
        return 0;
    }

    const date =
        new Date(createdAt);

    if (Number.isNaN(date.getTime())) {
        return 0;
    }

    return date.getTime();
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

    return date.toLocaleDateString(
        "en-GB",
        {
            day: "2-digit",
            month: "short",
            year: "numeric"
        }
    );
}

function clearFilters() {
    document
        .getElementById("share-search")
        .value = "";

    document
        .getElementById("share-type-filter")
        .value = "all";

    document
        .getElementById("share-sort")
        .value = "newest";

    applyFilters();
}

function updateTotalSharesCount(count) {
    document
        .getElementById("total-shares-count")
        .textContent = count;
}

function showLoadingState() {
    hideAllStates();

    document
        .getElementById("shares-loading")
        .classList.remove("hidden");

    document
        .getElementById("shares-list")
        .innerHTML = "";

    updateTotalSharesCount(0);
}

function hideAllStates() {
    document
        .getElementById("shares-loading")
        .classList.add("hidden");

    document
        .getElementById("shares-error")
        .classList.add("hidden");

    document
        .getElementById("shares-empty")
        .classList.add("hidden");
}
