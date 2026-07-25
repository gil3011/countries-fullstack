
function getCreateShareElements() {
    return {
        modal: document.getElementById("share-modal"),
        form: document.getElementById("share-form"),
        countrySelect: document.getElementById("share-country"),
        titleInput: document.getElementById("share-title"),
        typeSelect: document.getElementById("share-type"),
        descriptionInput: document.getElementById("share-description"),
        messageElement: document.getElementById("share-form-message"),
        submitButton: document.getElementById("submit-share-btn"),
        openButton: document.getElementById("open-share-form-btn"),
        closeButton: document.getElementById("close-share-form-btn"),
        cancelButton: document.getElementById("cancel-share-btn"),
        overlay: document.querySelector(
            "#share-modal .share-modal-overlay"
        )
    };
}

function getLoggedInUserId() {
    const user = getUserLoggedIn();

    if (!user) {
        return null;
    }

    return user.id || user.Id || user.userId || null;
}

function getCountriesForShare() {
    if (typeof window.getCachedCountries !== "function") {
        console.error(
            "getCachedCountries is not available."
        );

        return Promise.resolve([]);
    }

    const cachedCountries =
        window.getCachedCountries();

    if (
        Array.isArray(cachedCountries) &&
        cachedCountries.length > 0
    ) {
        return Promise.resolve(cachedCountries);
    }

    return new Promise((resolve) => {
        ajaxCall(
            "GET",
            API_ROUTES.countryAPI,
            null,
            function (data) {
                const countries =
                    Array.isArray(data)
                        ? data
                        : [];

                resolve(countries);
            },
            function (error) {
                console.error(
                    "Failed to load countries:",
                    error
                );

                resolve([]);
            }
        );
    });
}

async function populateShareCountries(selectedCountryId = null) {
    const { countrySelect } =
        getCreateShareElements();

    if (!countrySelect) {
        return;
    }

    const countries =
        await getCountriesForShare();

    countrySelect.innerHTML = "";

    const defaultOption =
        document.createElement("option");

    defaultOption.value = "";
    defaultOption.textContent =
        "Select a country";

    countrySelect.appendChild(defaultOption);

    const sortedCountries =
        [...countries].sort((countryA, countryB) => {
            const nameA =
                countryA.commonName || "";

            const nameB =
                countryB.commonName || "";

            return nameA.localeCompare(nameB);
        });

    sortedCountries.forEach(country => {
        const option =
            document.createElement("option");

        option.value = country.id;
        option.textContent =
            country.commonName ||
            "Unknown country";

        if (
            selectedCountryId != null &&
            Number(country.id) ===
            Number(selectedCountryId)
        ) {
            option.selected = true;
        }

        countrySelect.appendChild(option);
    });
}

async function openShareModal(countryId = null) {
    const {
        modal,
        countrySelect,
        titleInput
    } = getCreateShareElements();

    if (!modal) {
        console.error("Share modal was not found.");
        return;
    }

    const userId = getLoggedInUserId();

    if (!userId) {
        alert("You must be logged in to create a share.");
        return;
    }

    resetShareForm();

    const selectGroup =
        document.getElementById(
            "share-country-select-group"
        );

    const displayGroup =
        document.getElementById(
            "share-country-display-group"
        );

    const displayElement =
        document.getElementById(
            "share-country-display"
        );

    await populateShareCountries(countryId);

    if (countryId != null) {
        countrySelect.value =
            String(countryId);

        const selectedOption =
            countrySelect.options[
            countrySelect.selectedIndex
            ];

        const countryName =
            selectedOption?.textContent ||
            "Selected country";

        selectGroup?.classList.add("hidden");
        displayGroup?.classList.remove("hidden");

        if (displayElement) {
            displayElement.textContent =
                "🌍 " + countryName;
        }

        titleInput?.focus();
    } else {
        selectGroup?.classList.remove("hidden");
        displayGroup?.classList.add("hidden");

        if (displayElement) {
            displayElement.textContent = "";
        }

        countrySelect?.focus();
    }

    modal.classList.remove("hidden");
    document.body.style.overflow = "hidden";
}
function closeShareModal() {
    const { modal } =
        getCreateShareElements();

    if (!modal) {
        return;
    }

    modal.classList.add("hidden");
    document.body.style.overflow = "";
}

function resetShareForm() {
    const {
        form,
        messageElement,
        submitButton
    } = getCreateShareElements();

    form?.reset();

    if (messageElement) {
        messageElement.className =
            "share-form-message hidden";

        messageElement.textContent = "";
    }

    if (submitButton) {
        submitButton.disabled = false;
        submitButton.textContent =
            "Publish Share";
    }
}

function showShareFormMessage(
    message,
    type
) {
    const { messageElement } =
        getCreateShareElements();

    if (!messageElement) {
        return;
    }

    messageElement.className =
        "share-form-message " + type;

    messageElement.textContent =
        message;
}

function submitShareForm(event) {
    event.preventDefault();

    const {
        countrySelect,
        titleInput,
        typeSelect,
        descriptionInput,
        submitButton
    } = getCreateShareElements();

    const userId =
        getLoggedInUserId();

    if (!userId) {
        showShareFormMessage(
            "Could not identify the logged-in user.",
            "error"
        );

        return;
    }

    const countryId =
        countrySelect?.value;

    const title =
        titleInput?.value.trim();

    const typeValue =
        typeSelect?.value;

    const description =
        descriptionInput?.value.trim();

    if (
        !countryId ||
        !title ||
        typeValue === "" ||
        !description
    ) {
        showShareFormMessage(
            "Please complete all fields.",
            "error"
        );

        return;
    }

    const share = {
        userId: Number(userId),
        id: 0,
        title,
        description,
        type: Number(typeValue),
        countryId: Number(countryId),
        createdAt:
            new Date().toISOString()
    };

    if (submitButton) {
        submitButton.disabled = true;
        submitButton.textContent =
            "Publishing...";
    }

    ajaxCall(
        "POST",
        API_ROUTES.shareAPI +
        "/CreateShare",
        JSON.stringify(share),
        handleCreateShareSuccess,
        handleCreateShareError
    );
}

function handleCreateShareSuccess() {
    showShareFormMessage(
        "The share was published successfully.",
        "success"
    );

    if (
        typeof window.onShareCreated ===
        "function"
    ) {
        window.onShareCreated();
    }

    setTimeout(function () {
        closeShareModal();
    }, 700);
}

function handleCreateShareError(error) {
    const { submitButton } =
        getCreateShareElements();

    if (submitButton) {
        submitButton.disabled = false;
        submitButton.textContent =
            "Publish Share";
    }

    showShareFormMessage(
        "Could not publish the share.",
        "error"
    );

    console.error(
        "Failed to create share:",
        error
    );
}

function initializeCreateShare() {
    const {
        form,
        openButton,
        closeButton,
        cancelButton,
        overlay
    } = getCreateShareElements();

    if (!form) {
        return;
    }

    openButton?.addEventListener(
        "click",
        function () {
            const defaultCountryId =
                typeof window.getDefaultShareCountryId === "function"
                    ? window.getDefaultShareCountryId()
                    : null;

            openShareModal(defaultCountryId);
        }
    );

    closeButton?.addEventListener(
        "click",
        closeShareModal
    );

    cancelButton?.addEventListener(
        "click",
        closeShareModal
    );

    overlay?.addEventListener(
        "click",
        closeShareModal
    );

    form.addEventListener(
        "submit",
        submitShareForm
    );

    document.addEventListener(
        "keydown",
        function (event) {
            if (
                event.key === "Escape" &&
                !document
                    .getElementById("share-modal")
                    ?.classList.contains("hidden")
            ) {
                closeShareModal();
            }
        }
    );
}

window.openShareModal =
    openShareModal;

window.closeShareModal =
    closeShareModal;

window.initializeCreateShare =
    initializeCreateShare;

$(document).ready(function () {
    createShareModalHtml();
    initializeCreateShare();
});

function createShareModalHtml() {
    if (document.getElementById("share-modal")) {
        return;
    }

    const modalHtml = `
        <div id="share-modal"
             class="share-modal hidden">

            <div class="share-modal-overlay"></div>

            <div class="share-modal-content"
                 role="dialog"
                 aria-modal="true"
                 aria-labelledby="share-modal-title">

                <div class="share-modal-header">
                    <div>
                        <span class="create-share-label">
                            Community
                        </span>

                        <h2 id="share-modal-title">
                            Add a Share
                        </h2>
                    </div>

                    <button id="close-share-form-btn"
                            class="share-modal-close"
                            type="button"
                            aria-label="Close">
                        ×
                    </button>
                </div>

                <form id="share-form">

                    <div id="share-country-select-group"
                         class="form-group">

                        <label for="share-country">
                            Country
                        </label>

                        <select id="share-country"
                                name="countryId"
                                required>
                            <option value="">
                                Select a country
                            </option>
                        </select>
                    </div>

                    <div id="share-country-display-group"
                         class="form-group hidden">

                        <label>
                            Country
                        </label>

                        <div id="share-country-display"
                             class="share-country-display">
                        </div>
                    </div>

                    <div class="form-group">
                        <label for="share-title">
                            Title
                        </label>

                        <input id="share-title"
                               name="title"
                               type="text"
                               maxlength="150"
                               placeholder="Enter a title"
                               required />
                    </div>

                    <div class="form-group">
                        <label for="share-type">
                            Share Type
                        </label>

                        <select id="share-type"
                                name="type"
                                required>

                            <option value="">
                                Select a share type
                            </option>

                            <option value="0">
                                Recommendation
                            </option>

                            <option value="1">
                                Thought
                            </option>

                            <option value="2">
                                Review
                            </option>
                        </select>
                    </div>

                    <div class="form-group">
                        <label for="share-description">
                            Description
                        </label>

                        <textarea id="share-description"
                                  name="description"
                                  rows="5"
                                  maxlength="1000"
                                  placeholder="Share your experience or thoughts"
                                  required></textarea>
                    </div>

                    <div id="share-form-message"
                         class="share-form-message hidden">
                    </div>

                    <div class="share-form-actions">

                        <button id="cancel-share-btn"
                                class="share-cancel-btn"
                                type="button">
                            Cancel
                        </button>

                        <button id="submit-share-btn"
                                class="share-submit-btn"
                                type="submit">
                            Publish Share
                        </button>
                    </div>
                </form>
            </div>
        </div>
    `;

    document.body.insertAdjacentHTML(
        "beforeend",
        modalHtml
    );
}