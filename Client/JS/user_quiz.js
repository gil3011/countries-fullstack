// Base URL for API (assuming standard backend location)
const API_BASE_URL = 'https://localhost:7255/api';

let allCountries = [];
let currentAssociatedCountryIds = [];

function fetchCountries() {
    ajaxCall("GET", `${API_BASE_URL}/Countries`, "",
        (data) => {
            allCountries = data;
            // Sort alphabetically by common name
            allCountries.sort((a, b) => (a.commonName || "").localeCompare(b.commonName || ""));
            const select = $('#editor-quiz-country-select');
            select.empty();
            select.append(new Option("Select a country to add...", ""));
            allCountries.forEach(c => {
                select.append(new Option(c.commonName, c.id));
            });
        },
        (err) => console.error("Failed to load countries:", err)
    );
}

function renderSelectedCountries() {
    const container = $('#editor-selected-countries-list');
    container.empty();
    currentAssociatedCountryIds.forEach(id => {
        const country = allCountries.find(c => c.id === id);
        if (country) {
            container.append(`
                <span class="country-tag" style="background: rgba(96, 165, 250, 0.2); border: 1px solid var(--accent-primary); padding: 0.2rem 0.5rem; border-radius: 4px; display: inline-flex; align-items: center; gap: 0.5rem; font-size: 0.85rem;">
                    ${country.commonName}
                    <i class="fa-solid fa-xmark" style="cursor:pointer;" onclick="removeCountry(${id})"></i>
                </span>
            `);
        }
    });
}

function removeCountry(id) {
    currentAssociatedCountryIds = currentAssociatedCountryIds.filter(cid => cid !== id);
    renderSelectedCountries();
    if (currentEditingQuizId) updateQuizData(currentEditingQuizId);
}

$(document).ready(function () {
    // Navigation
    $('.nav-links li').on('click', function () {
        $('.nav-links li').removeClass('active');
        $(this).addClass('active');

        const target = $(this).data('target');
        $('.tab-section').removeClass('active');
        $('#' + target).addClass('active');

        // Update Title & Context Actions
        const text = $(this).text().trim();
        $('#section-title').text(text);

        if (target === 'manage-section') {
            $('#btn-create-quiz').show();
            fetchMyQuizzes();
        } else {
            $('#btn-create-quiz').hide();
        }

        if (target === 'explore-section') fetchPublicQuizzes();
        if (target === 'attempts-section') fetchMyAttempts();
    });

    // Initial Load
    fetchPublicQuizzes();
    fetchCountries();

    // Setup Modals
    $('#btn-create-quiz').on('click', () => {
        $('#editor-modal-title').text('Create New Quiz');
        $('#editor-quiz-id').val('');
        $('#editor-quiz-title').val('');
        $('#editor-quiz-country-select').val('');
        currentAssociatedCountryIds = [];
        renderSelectedCountries();
        $('#editor-questions-list').html('<p class="text-muted text-sm">No questions added yet. Add your first question on the right.</p>');
        currentEditingQuizId = null;
        currentQuestions = [];
        clearQuestionEditor();
        openModal('quiz-editor-modal');
    });

    $('#editor-quiz-title').on('blur', function () {
        if (currentEditingQuizId) updateQuizData(currentEditingQuizId);
    });
    
    $('#editor-quiz-country-select').on('change', function () {
        const val = $(this).val();
        if (val) {
            const id = parseInt(val);
            if (!currentAssociatedCountryIds.includes(id)) {
                currentAssociatedCountryIds.push(id);
                renderSelectedCountries();
                if (currentEditingQuizId) updateQuizData(currentEditingQuizId);
            }
            $(this).val('');
        }
    });

    $('#btn-save-question').on('click', saveQuestion);

    // Filter Search
    $('#search-quiz').on('input', function () {
        const term = $(this).val().toLowerCase();
        $('.quiz-card').each(function () {
            const title = $(this).find('.quiz-title').text().toLowerCase();
            $(this).toggle(title.includes(term));
        });
    });
});

function initUser() {
    const userStr = localStorage.getItem('loggedInUser') || sessionStorage.getItem('loggedInUser');
    if (userStr) {
        try {
            const user = JSON.parse(userStr);
            $('#logged-user-name').text(user.username || user.email || 'Logged In User');
        } catch (e) {
            $('#logged-user-name').text('Invalid User Data');
        }
    } else {
        showToast("No logged in user found. Please log in.", "error");
        // window.location.href = 'login.html';
    }
}

// Call initUser on load
$(document).ready(function () {
    initUser();
});

function getUserId() {
    const userStr = localStorage.getItem('loggedInUser') || sessionStorage.getItem('loggedInUser');
    if (userStr) {
        try {
            const user = JSON.parse(userStr);
            if (user.id) return parseInt(user.id);
        } catch (e) {
            return null;
        }
    }
    return null; // No user logged in
}

function logout() {
    localStorage.removeItem('loggedInUser');
    sessionStorage.removeItem('loggedInUser');
    window.location.href = 'login.html';
}

/* ==========================================================================
   UI Utility Functions
   ========================================================================== */
function openModal(id) {
    $('#' + id).addClass('show');
}

function closeModal(id) {
    $('#' + id).removeClass('show');
}

function showToast(message, type = 'info') {
    const icon = type === 'success' ? 'fa-check-circle' : type === 'error' ? 'fa-circle-xmark' : 'fa-info-circle';
    const toast = $(`
        <div class="toast ${type}">
            <i class="fa-solid ${icon} fa-lg"></i>
            <div>${message}</div>
        </div>
    `);
    $('#toast-container').append(toast);
    setTimeout(() => {
        toast.fadeOut(400, function () { $(this).remove(); });
    }, 4000);
}

function renderEmptyState(container, message) {
    $(container).html(`<div style="grid-column: 1/-1; text-align: center; padding: 3rem; color: var(--text-muted);"><i class="fa-solid fa-ghost fa-2x mb-2"></i><br>${message}</div>`);
}

function handleError(err, defaultMsg) {
    console.error(defaultMsg, err);
    let msg = defaultMsg;
    if (err.responseJSON && err.responseJSON.message) {
        msg = err.responseJSON.message;
    } else if (err.responseText && err.responseText.length < 150) {
        msg = err.responseText;
    }
    showToast(msg, "error");
}

/* ==========================================================================
   EXPLORE: Public Quizzes
   ========================================================================== */
function fetchPublicQuizzes() {
    $('#public-quizzes-grid').html('<p>Loading public quizzes...</p>');

    ajaxCall("GET", `${API_BASE_URL}/Quiz/Public`, "",
        (data) => {
            renderQuizzes(data, '#public-quizzes-grid', false);
        },
        (err) => {
            renderEmptyState('#public-quizzes-grid', 'Failed to load public quizzes.');
            handleError(err, "Failed to fetch public quizzes.");
        }
    );
}

/* ==========================================================================
   MANAGE: My Quizzes
   ========================================================================== */
function fetchMyQuizzes() {
    $('#my-quizzes-grid').html('<p>Loading your quizzes...</p>');
    const userId = getUserId();

    ajaxCall("GET", `${API_BASE_URL}/Quiz/User/${userId}`, "",
        (data) => {
            renderQuizzes(data, '#my-quizzes-grid', true);
        },
        (err) => {
            renderEmptyState('#my-quizzes-grid', 'Failed to load your quizzes.');
            handleError(err, "Failed to fetch user quizzes.");
        }
    );
}

function updateQuizData(id) {
    const title = $('#editor-quiz-title').val();
    if (!title) return;

    const quizData = {
        id: id,
        title: title,
        associatedCountryIds: currentAssociatedCountryIds,
        creatorId: getUserId()
    };

    ajaxCall("PUT", `${API_BASE_URL}/Quiz/${id}?userId=${getUserId()}`, JSON.stringify(quizData),
        () => console.log('Quiz data auto-saved'),
        (err) => console.error('Failed to auto-save quiz data', err)
    );
}

function openQuizEditor(quizId) {
    currentEditingQuizId = quizId;
    $('#editor-modal-title').text('Edit Quiz');
    clearQuestionEditor();
    $('#editor-questions-list').html('<p class="text-muted text-sm">Loading...</p>');

    // Fetch quiz details including questions
    ajaxCall("GET", `${API_BASE_URL}/Quiz/${quizId}`, "",
        (quiz) => {
            $('#editor-quiz-id').val(quiz.id);
            $('#editor-quiz-title').val(quiz.title);
            
            currentAssociatedCountryIds = quiz.associatedCountryIds || [];
            renderSelectedCountries();

            currentQuestions = quiz.questions || [];
            renderQuestionsList();
        },
        (err) => {
            handleError(err, "Failed to fetch quiz details.");
        }
    );

    openModal('quiz-editor-modal');
}

function renderQuestionsList() {
    let html = '';
    if (currentQuestions.length === 0) {
        html = '<p class="text-muted text-sm">No questions added yet. Add your first question on the right.</p>';
    } else {
        currentQuestions.forEach(q => {
            html += `
                <li>
                    <span>${q.text}</span>
                    <div>
                        <button class="btn btn-secondary btn-sm" style="padding: 0.3rem 0.6rem;" onclick="editQuestion(${q.id})"><i class="fa-solid fa-pen"></i></button>
                        <button class="btn btn-danger btn-sm" style="padding: 0.3rem 0.6rem;" onclick="deleteQuestion(${q.id})"><i class="fa-solid fa-trash"></i></button>
                    </div>
                </li>
            `;
        });
    }
    $('#editor-questions-list').html(html);
}

function publishQuiz(quizId) {
    if (!confirm("Are you sure you want to publish? You won't be able to edit it anymore.")) return;

    ajaxCall("POST", `${API_BASE_URL}/Quiz/${quizId}/Publish?userId=${getUserId()}`, "",
        (res) => {
            showToast("Quiz Published!", "success");
            fetchMyQuizzes();
        },
        (err) => {
            handleError(err, "Failed to publish quiz.");
        }
    );
}

function unpublishQuiz(quizId) {
    if (!confirm("Unpublish this quiz? It will become private again.")) return;

    ajaxCall("POST", `${API_BASE_URL}/Quiz/${quizId}/Unpublish?userId=${getUserId()}`, "",
        (res) => {
            showToast("Quiz is now Private.", "success");
            fetchMyQuizzes();
        },
        (err) => {
            handleError(err, "Failed to unpublish quiz.");
        }
    );
}

function deleteQuiz(quizId) {
    if (!confirm("Delete this quiz completely?")) return;

    ajaxCall("DELETE", `${API_BASE_URL}/Quiz/${quizId}?userId=${getUserId()}`, "",
        (res) => {
            showToast("Quiz Deleted", "success");
            fetchMyQuizzes();
        },
        (err) => {
            handleError(err, "Failed to delete quiz.");
        }
    );
}

/* ==========================================================================
   QUESTIONS MANAGEMENT
   ========================================================================== */
let currentEditingQuizId = null;
let currentQuestions = [];

function editQuestion(qId) {
    const q = currentQuestions.find(x => x.id === qId);
    if (!q) return;

    $('#q-id-input').val(q.id);
    $('#q-text-input').val(q.text);
    $('#q-opt1-input').val(q.optionA || '');
    $('#q-opt2-input').val(q.optionB || '');
    $('#q-opt3-input').val(q.optionC || '');
    $('#q-opt4-input').val(q.optionD || '');
    $('#btn-save-question').text('Update Question');
}

function clearQuestionEditor() {
    $('#q-id-input').val('');
    $('#q-text-input').val('');
    $('#q-opt1-input').val('');
    $('#q-opt2-input').val('');
    $('#q-opt3-input').val('');
    $('#q-opt4-input').val('');
    $('#btn-save-question').text('Save Question');
}

function saveQuestion() {
    const title = $('#editor-quiz-title').val();
    if (!title) return showToast("Please set a Quiz Title first", "error");

    if (!$('#q-text-input').val()) return showToast("Question text required", "error");
    if (!$('#q-opt1-input').val()) return showToast("At least the correct answer is required", "error");

    const newQuestion = {
        text: $('#q-text-input').val(),
        optionA: $('#q-opt1-input').val(),
        optionB: $('#q-opt2-input').val(),
        optionC: $('#q-opt3-input').val(),
        optionD: $('#q-opt4-input').val()
    };

    const qId = $('#q-id-input').val();

    if (currentEditingQuizId) {
        performSaveQuestion(newQuestion, qId);
    } else {
        // Create Quiz First
        const quizData = {
            title: title,
            associatedCountryIds: currentAssociatedCountryIds,
            creatorId: getUserId()
        };

        ajaxCall("POST", `${API_BASE_URL}/Quiz`, JSON.stringify(quizData),
            (res) => {
                // res should be the new quiz ID
                currentEditingQuizId = res;
                $('#editor-quiz-id').val(res);
                performSaveQuestion(newQuestion, qId);
            },
            (err) => {
                handleError(err, "Failed to create quiz before saving question.");
            }
        );
    }
}

function performSaveQuestion(newQuestion, qId) {
    if (qId) {
        newQuestion.id = parseInt(qId);
        ajaxCall("PUT", `${API_BASE_URL}/Quiz/Question/${qId}?userId=${getUserId()}`, JSON.stringify(newQuestion),
            (res) => {
                showToast("Question updated!", "success");
                clearQuestionEditor();
                refreshQuestionsList();
            },
            (err) => {
                handleError(err, "Failed to update question.");
            }
        );
    } else {
        ajaxCall("POST", `${API_BASE_URL}/Quiz/${currentEditingQuizId}/Question?userId=${getUserId()}`, JSON.stringify(newQuestion),
            (res) => {
                showToast("Question saved!", "success");
                clearQuestionEditor();
                refreshQuestionsList();
            },
            (err) => {
                handleError(err, "Failed to save question.");
            }
        );
    }
}

function refreshQuestionsList() {
    ajaxCall("GET", `${API_BASE_URL}/Quiz/${currentEditingQuizId}`, "",
        (quiz) => {
            currentQuestions = quiz.questions || [];
            renderQuestionsList();
        },
        (err) => {
            console.error("Failed to refresh questions", err);
        }
    );
}

function deleteQuestion(qId) {
    if (!confirm("Delete question?")) return;

    ajaxCall("DELETE", `${API_BASE_URL}/Quiz/Question/${qId}?userId=${getUserId()}`, "",
        (res) => {
            showToast("Question deleted", "success");
            refreshQuestionsList();
        },
        (err) => {
            handleError(err, "Failed to delete question.");
        }
    );
}

/* ==========================================================================
   ATTEMPTS & TAKING QUIZZES
   ========================================================================== */
function fetchMyAttempts() {
    $('#my-attempts-grid').html('<p>Loading attempts...</p>');
    const userId = getUserId();

    ajaxCall("GET", `${API_BASE_URL}/QuizAttempt/User/${userId}`, "",
        (attempts) => {
            let html = '';
            if (!attempts || attempts.length === 0) {
                html = '<p class="text-muted">No attempts found.</p>';
            } else {
                attempts.forEach(a => {
                    html += `
                        <div class="list-item">
                            <div>
                                <h3 style="margin-bottom: 0.25rem;">${a.quizTitle || 'Quiz ' + a.quizId}</h3>
                                <span class="text-muted text-sm"><i class="fa-regular fa-calendar"></i> ${a.dateTaken ? new Date(a.dateTaken).toLocaleString([], { year: 'numeric', month: 'numeric', day: 'numeric', hour: '2-digit', minute: '2-digit' }) : 'Unknown date'}</span>
                            </div>
                            <div style="display: flex; gap: 1rem; align-items: center;">
                                <div class="score-badge">${a.score}</div>
                                <button class="btn btn-secondary" onclick="viewAttempt(${a.id})"><i class="fa-solid fa-eye"></i> Details</button>
                                <button class="btn btn-primary btn-sm" onclick="takeQuiz(${a.quizId})" title="Reattempt"><i class="fa-solid fa-rotate-right"></i></button>
                                <button class="btn btn-danger btn-sm" onclick="deleteAttempt(${a.id})"><i class="fa-solid fa-trash"></i></button>
                            </div>
                        </div>
                    `;
                });
            }
            $('#my-attempts-grid').html(html);
        },
        (err) => {
            $('#my-attempts-grid').html('<p class="text-danger">Failed to load attempts.</p>');
            handleError(err, "Failed to fetch user attempts.");
        }
    );
}

function viewAttempt(id) {
    ajaxCall("GET", `${API_BASE_URL}/QuizAttempt/${id}`, "",
        (attemptDetails) => {
            ajaxCall("GET", `${API_BASE_URL}/Quiz/${attemptDetails.quizId}`, "",
                (quiz) => {
                    quiz.questions.forEach(q => {
                        const opts = [q.optionA, q.optionB, q.optionC, q.optionD].filter(o => o && o.trim() !== '');
                        opts.sort(() => Math.random() - 0.5);
                        q.shuffledOptions = opts;
                    });

                    tqCurrentQuiz = quiz;
                    tqCurrentQuestionIndex = 0;
                    tqUserAnswers = attemptDetails.answers;
                    tqMode = 'VIEW';
                    tqIsCurrentQuestionSubmitted = true;

                    $('#take-quiz-title').text(quiz.title + " (Review)");
                    $('#btn-tq-next').show();
                    renderCurrentQuestion();
                    openModal('take-quiz-modal');
                },
                (err) => handleError(err, "Failed to load quiz details.")
            );
        },
        (err) => handleError(err, "Failed to load attempt details.")
    );
}

let tqCurrentQuiz = null;
let tqCurrentQuestionIndex = 0;
let tqUserAnswers = {}; // map of questionId -> selectedOptionText
let tqUserAnswersIndex = {}; // map of questionId -> selectedIndex
let tqMode = 'TAKE';
let tqIsCurrentQuestionSubmitted = false;

function takeQuiz(quizId) {
    ajaxCall("GET", `${API_BASE_URL}/Quiz/${quizId}`, "",
        (quiz) => {
            if (!quiz.questions || quiz.questions.length === 0) {
                return showToast("This quiz has no questions.", "error");
            }

            // Pre-shuffle options for each question so they don't change on selection
            quiz.questions.forEach(q => {
                const opts = [q.optionA, q.optionB, q.optionC, q.optionD].filter(o => o && o.trim() !== '');
                opts.sort(() => Math.random() - 0.5);
                q.shuffledOptions = opts;
            });

            tqCurrentQuiz = quiz;
            tqCurrentQuestionIndex = 0;
            tqUserAnswers = {};
            tqUserAnswersIndex = {};
            tqMode = 'TAKE';
            tqIsCurrentQuestionSubmitted = false;

            $('#take-quiz-title').text(quiz.title);
            $('#btn-tq-next').show();
            renderCurrentQuestion();
            openModal('take-quiz-modal');
        },
        (err) => {
            handleError(err, "Failed to load quiz for taking.");
        }
    );
}

function renderCurrentQuestion() {
    $('#tq-error-msg').text('');
    const q = tqCurrentQuiz.questions[tqCurrentQuestionIndex];
    $('#take-quiz-progress').text(`Question ${tqCurrentQuestionIndex + 1} of ${tqCurrentQuiz.questions.length}`);
    $('#tq-question-text').text(q.text);

    const options = q.shuffledOptions;
    const isSubmitted = tqMode === 'VIEW' || tqIsCurrentQuestionSubmitted;

    let html = '';
    options.forEach((opt, index) => {
        let style = '';
        let icon = '';

        if (isSubmitted) {
            const isSelected = tqUserAnswers[q.id] === opt;
            const isCorrect = q.optionA === opt;

            if (isCorrect) {
                style = 'background: rgba(34, 197, 94, 0.2); border-color: var(--success);';
                icon = '<i class="fa-solid fa-check" style="color: var(--success); margin-left: auto;"></i>';
            } else if (isSelected && !isCorrect) {
                style = 'background: rgba(239, 68, 68, 0.2); border-color: var(--danger);';
                icon = '<i class="fa-solid fa-xmark" style="color: var(--danger); margin-left: auto;"></i>';
            }
        } else {
            if (tqUserAnswersIndex[q.id] === index) {
                style = 'background: rgba(96, 165, 250, 0.2); border-color: var(--accent-primary);';
            }
        }

        const cursor = isSubmitted ? 'default' : 'pointer';
        const clickHandler = isSubmitted ? '' : `onclick="selectAnswer(${q.id}, ${index}, '${opt.replace(/'/g, "\\'")}')"`;

        html += `
            <div class="glass-input" style="cursor: ${cursor}; ${style}; display: flex; align-items: center;" ${clickHandler}>
                ${opt} ${icon}
            </div>
        `;
    });

    $('#tq-options-container').html(html);

    if (tqMode === 'VIEW') {
        if (tqCurrentQuestionIndex === tqCurrentQuiz.questions.length - 1) {
            $('#btn-tq-next').html('Finish <i class="fa-solid fa-check"></i>').removeClass('btn-primary').addClass('btn-success');
        } else {
            $('#btn-tq-next').html('Next Question <i class="fa-solid fa-arrow-right"></i>').removeClass('btn-success').addClass('btn-primary');
        }
    } else {
        if (!tqIsCurrentQuestionSubmitted) {
            $('#btn-tq-next').html('Submit Answer <i class="fa-solid fa-paper-plane"></i>').removeClass('btn-success').addClass('btn-primary');
        } else {
            if (tqCurrentQuestionIndex === tqCurrentQuiz.questions.length - 1) {
                $('#btn-tq-next').html('Finish Quiz <i class="fa-solid fa-flag-checkered"></i>').removeClass('btn-primary').addClass('btn-success');
            } else {
                $('#btn-tq-next').html('Next Question <i class="fa-solid fa-arrow-right"></i>').removeClass('btn-success').addClass('btn-primary');
            }
        }
    }
}

function selectAnswer(qId, index, selectedOpt) {
    if (tqMode === 'VIEW' || tqIsCurrentQuestionSubmitted) return;
    tqUserAnswersIndex[qId] = index;
    tqUserAnswers[qId] = selectedOpt;
    renderCurrentQuestion();
}

function nextQuestion() {
    const q = tqCurrentQuiz.questions[tqCurrentQuestionIndex];

    if (tqMode === 'TAKE') {
        if (!tqUserAnswers[q.id]) {
            $('#tq-error-msg').text('Please select an answer.');
            return;
        }

        if (!tqIsCurrentQuestionSubmitted) {
            tqIsCurrentQuestionSubmitted = true;
            renderCurrentQuestion();
            return;
        }
    }

    if (tqCurrentQuestionIndex < tqCurrentQuiz.questions.length - 1) {
        tqCurrentQuestionIndex++;
        tqIsCurrentQuestionSubmitted = false;
        renderCurrentQuestion();
    } else {
        if (tqMode === 'TAKE') {
            submitQuiz();
        } else {
            showFinalViewScreen();
        }
    }
}

function showFinalViewScreen() {
    let score = 0;
    const total = tqCurrentQuiz.questions.length;

    tqCurrentQuiz.questions.forEach(q => {
        if (tqUserAnswers[q.id] === q.optionA) {
            score++;
        }
    });

    const percentage = Math.round((score / total) * 100);

    $('#take-quiz-body').html(`
        <div style="text-align: center; padding: 2rem;">
            <i class="fa-solid fa-clipboard-check fa-4x" style="color: var(--accent-primary); margin-bottom: 1rem;"></i>
            <h2>Attempt Review</h2>
            <p style="font-size: 1.2rem; color: var(--text-muted);">You scored</p>
            <div class="score-badge" style="font-size: 2rem; padding: 1rem 2rem; display: inline-block; margin-top: 1rem;">${percentage}%</div>
            <p style="margin-top: 1rem; font-size: 0.9rem;">(${score} out of ${total} correct)</p>
            <div style="margin-top: 2rem;">
                <button class="btn btn-primary" onclick="closeTakeQuiz(); setTimeout(() => takeQuiz(${tqCurrentQuiz.id}), 300)"><i class="fa-solid fa-rotate-right"></i> Reattempt Quiz</button>
            </div>
        </div>
    `);
    $('#tq-error-msg').text('');
    $('#btn-tq-next').hide();
}

function submitQuiz() {
    // Calculate score
    let score = 0;
    const total = tqCurrentQuiz.questions.length;

    tqCurrentQuiz.questions.forEach(q => {
        if (tqUserAnswers[q.id] === q.optionA) {
            score++;
        }
    });

    const percentage = Math.round((score / total) * 100);

    const attemptData = {
        quizId: tqCurrentQuiz.id,
        userId: getUserId(),
        score: percentage,
        dateTaken: new Date().toISOString(),
        answers: tqUserAnswers
    };

    ajaxCall("POST", `${API_BASE_URL}/QuizAttempt`, JSON.stringify(attemptData),
        (res) => {
            $('#take-quiz-body').html(`
                <div style="text-align: center; padding: 2rem;">
                    <i class="fa-solid fa-trophy fa-4x" style="color: #fbbf24; margin-bottom: 1rem;"></i>
                    <h2>Quiz Completed!</h2>
                    <p style="font-size: 1.2rem; color: var(--text-muted);">You scored</p>
                    <div class="score-badge" style="font-size: 2rem; padding: 1rem 2rem; display: inline-block; margin-top: 1rem;">${percentage}%</div>
                    <p style="margin-top: 1rem; font-size: 0.9rem;">(${score} out of ${total} correct)</p>
                </div>
            `);
            $('#tq-error-msg').text('');
            $('#btn-tq-next').hide();

            fetchMyAttempts();
        },
        (err) => {
            handleError(err, "Failed to submit attempt.");
        }
    );
}

function closeTakeQuiz() {
    closeModal('take-quiz-modal');
    setTimeout(() => {
        $('#take-quiz-body').html(`
            <h3 id="tq-question-text" style="margin-bottom: 1.5rem; font-size: 1.3rem;"></h3>
            <div id="tq-options-container" style="display: flex; flex-direction: column; gap: 0.75rem;"></div>
        `);
        $('#btn-tq-next').show();
    }, 300);
}

function deleteAttempt(id) {
    if (!confirm("Delete this attempt completely?")) return;

    ajaxCall("DELETE", `${API_BASE_URL}/QuizAttempt/${id}?userId=${getUserId()}`, "",
        (res) => {
            showToast("Attempt Deleted", "success");
            fetchMyAttempts();
        },
        (err) => {
            handleError(err, "Failed to delete attempt.");
        }
    );
}

function likeQuiz(quizId) {
    ajaxCall("POST", `${API_BASE_URL}/Quiz/${quizId}/Like?userId=${getUserId()}`, "",
        (res) => {
            showToast("Quiz Liked! ❤️", "success");
            fetchPublicQuizzes();
        },
        (err) => {
            handleError(err, "Failed to like quiz.");
        }
    );
}

/* ==========================================================================
   RENDER HELPERS
   ========================================================================== */
function renderQuizzes(quizzes, container, isManageView) {
    let html = '';

    if (!quizzes || quizzes.length === 0) {
        renderEmptyState(container, 'No quizzes found.');
    } else {
        quizzes.forEach(q => {
            const badge = q.isPublic
                ? `<span class="badge badge-public">Public</span>`
                : `<span class="badge badge-private">Private</span>`;

            let actions = '';
            if (isManageView) {
                const escTitle = q.title ? q.title.replace(/'/g, "\\'") : '';
                const escDesc = q.description ? q.description.replace(/(\r\n|\n|\r)/gm, " ").replace(/'/g, "\\'") : '';

                if (!q.isPublic) {
                    actions = `
                        <div style="display: flex; gap: 0.5rem; flex-wrap: nowrap; width: 100%;">
                            <button class="btn btn-secondary btn-sm" style="flex: 1; white-space: nowrap; padding: 0.25rem 0.5rem;" onclick="openQuizEditor(${q.id})"><i class="fa-solid fa-pen"></i> Edit</button>
                            <button class="btn btn-success btn-sm" style="flex: 1; white-space: nowrap; padding: 0.25rem 0.5rem;" onclick="publishQuiz(${q.id})"><i class="fa-solid fa-globe"></i> Publish</button>
                            <button class="btn btn-danger btn-sm" style="padding: 0.25rem 0.5rem;" onclick="deleteQuiz(${q.id})"><i class="fa-solid fa-trash"></i></button>
                        </div>
                    `;
                } else {
                    actions = `
                        <div style="display: flex; gap: 0.5rem; flex-wrap: nowrap; width: 100%;">
                            <button class="btn btn-secondary btn-sm" style="flex: 1; white-space: nowrap; padding: 0.25rem 0.5rem;" onclick="openQuizEditor(${q.id})"><i class="fa-solid fa-pen"></i> Edit</button>
                            <button class="btn btn-warning btn-sm" style="flex: 1; white-space: nowrap; padding: 0.25rem 0.5rem;" onclick="unpublishQuiz(${q.id})"><i class="fa-solid fa-globe"></i> Unpublish</button>
                            <button class="btn btn-danger btn-sm" style="padding: 0.25rem 0.5rem;" onclick="deleteQuiz(${q.id})"><i class="fa-solid fa-trash"></i></button>
                        </div>
                    `;
                }
            } else {
                actions = `
                    <button class="btn btn-primary" onclick="takeQuiz(${q.id})"><i class="fa-solid fa-play"></i> Take Quiz</button>
                    <button class="btn btn-secondary" onclick="likeQuiz(${q.id})"><i class="fa-solid fa-heart"></i></button>
                `;
            }

            html += `
                <div class="quiz-card">
                    <div class="quiz-card-header">
                        <div class="quiz-title">${q.title}</div>
                        ${isManageView ? badge : ''}
                    </div>
                    <div class="quiz-meta">
                        <span><i class="fa-solid fa-layer-group"></i> ${q.questionCount !== undefined ? q.questionCount : (q.questions ? q.questions.length : 0)} Qs</span>
                        ${!isManageView ? `<span><i class="fa-solid fa-heart" style="color:var(--danger)"></i> ${q.likes || 0}</span>` : ''}
                    </div>
                    <p style="color: var(--text-muted); font-size: 0.9rem; margin-bottom: 1.5rem; flex-grow: 1;">
                        ${q.description || 'Test your knowledge on this topic.'}
                    </p>
                    <div class="quiz-card-actions">
                        ${actions}
                    </div>
                </div>
            `;
        });
        $(container).html(html);
    }
}
