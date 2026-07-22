// Base URL for API (assuming standard backend location)
const API_BASE_URL = 'https://localhost:7255/api';

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

    // Setup Modals
    $('#btn-create-quiz').on('click', () => {
        $('#modal-quiz-title').text('Create New Quiz');
        $('#quiz-id-input').val('');
        $('#quiz-title-input').val('');
        $('#quiz-desc-input').val('');
        openModal('quiz-modal');
    });

    $('#btn-save-quiz').on('click', saveQuiz);
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
$(document).ready(function() {
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

function saveQuiz() {
    const title = $('#quiz-title-input').val();
    if (!title) return showToast("Title is required", "error");

    const quizId = $('#quiz-id-input').val();
    const quizData = {
        title: title,
        description: $('#quiz-desc-input').val() || "",
        creatorId: getUserId()
    };

    if (quizId) {
        quizData.id = parseInt(quizId);
        ajaxCall("PUT", `${API_BASE_URL}/Quiz/${quizId}?userId=${getUserId()}`, JSON.stringify(quizData), 
            (res) => {
                showToast("Quiz Updated Successfully!", "success");
                closeModal('quiz-modal');
                fetchMyQuizzes();
            },
            (err) => {
                handleError(err, "Error updating quiz.");
            }
        );
    } else {
        ajaxCall("POST", `${API_BASE_URL}/Quiz`, JSON.stringify(quizData), 
            (res) => {
                showToast("Quiz Created Successfully!", "success");
                closeModal('quiz-modal');
                fetchMyQuizzes();
            },
            (err) => {
                handleError(err, "Error creating quiz.");
            }
        );
    }
}

function editQuiz(id, title, desc) {
    $('#modal-quiz-title').text('Edit Quiz Info');
    $('#quiz-id-input').val(id);
    $('#quiz-title-input').val(title);
    $('#quiz-desc-input').val(desc);
    openModal('quiz-modal');
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

function manageQuestions(quizId, quizTitle) {
    currentEditingQuizId = quizId;
    $('#mq-quiz-title').text(`(${quizTitle})`);

    // Reset editor
    clearQuestionEditor();

    // Fetch existing questions
    $('#questions-list').html('<p>Loading...</p>');

    ajaxCall("GET", `${API_BASE_URL}/Quiz/${quizId}`, "",
        (quiz) => {
            currentQuestions = quiz.questions || [];
            let html = '';
            if (currentQuestions.length === 0) {
                html = '<p class="text-muted">No questions yet.</p>';
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
            $('#questions-list').html(html);
        },
        (err) => {
            $('#questions-list').html('<p class="text-danger">Failed to load questions.</p>');
            handleError(err, "Failed to fetch quiz details.");
        }
    );

    openModal('questions-modal');
}

function editQuestion(qId) {
    const q = currentQuestions.find(x => x.id === qId);
    if(!q) return;
    
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
    if (qId) {
        newQuestion.id = parseInt(qId);
        ajaxCall("PUT", `${API_BASE_URL}/Quiz/Question/${qId}?userId=${getUserId()}`, JSON.stringify(newQuestion),
            (res) => {
                showToast("Question updated successfully!", "success");
                clearQuestionEditor();
                manageQuestions(currentEditingQuizId, $('#mq-quiz-title').text().replace(/[()]/g, '')); // Reload list
            },
            (err) => {
                handleError(err, "Failed to update question.");
            }
        );
    } else {
        ajaxCall("POST", `${API_BASE_URL}/Quiz/${currentEditingQuizId}/Question?userId=${getUserId()}`, JSON.stringify(newQuestion),
            (res) => {
                showToast("Question saved successfully!", "success");
                clearQuestionEditor();
                manageQuestions(currentEditingQuizId, $('#mq-quiz-title').text().replace(/[()]/g, '')); // Reload list
            },
            (err) => {
                handleError(err, "Failed to save question.");
            }
        );
    }
}

function deleteQuestion(qId) {
    if (!confirm("Delete question?")) return;

    ajaxCall("DELETE", `${API_BASE_URL}/Quiz/Question/${qId}?userId=${getUserId()}`, "",
        (res) => {
            showToast("Question deleted", "success");
            manageQuestions(currentEditingQuizId, $('#mq-quiz-title').text().replace(/[()]/g, '')); // Reload list
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
                                <h3 style="margin-bottom: 0.25rem;">Quiz ID: ${a.quizId}</h3>
                                <span class="text-muted text-sm"><i class="fa-regular fa-calendar"></i> ${a.dateAttempted ? new Date(a.dateAttempted).toLocaleDateString() : 'Unknown date'}</span>
                            </div>
                            <div style="display: flex; gap: 1rem; align-items: center;">
                                <div class="score-badge">${a.score}</div>
                                <button class="btn btn-secondary" onclick="viewAttempt(${a.id})"><i class="fa-solid fa-eye"></i> Details</button>
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
            console.log(attemptDetails);
            showToast(`Loaded details for attempt ${id}. Score: ${attemptDetails.score}`, "success");
            // Here you'd open a modal in a full implementation to show exactly what answers were chosen.
        },
        (err) => {
            handleError(err, "Failed to load attempt details.");
        }
    );
}

function takeQuiz(quizId) {
    ajaxCall("GET", `${API_BASE_URL}/Quiz/${quizId}`, "",
        (quiz) => {
            showToast(`Ready to start quiz: ${quiz.title}. Implement UI to display questions!`, "info");
        },
        (err) => {
            handleError(err, "Failed to load quiz for taking.");
        }
    );
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
    ajaxCall("POST", `${API_BASE_URL}/Quiz/${quizId}/Like`, "",
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
                if (!q.isPublic) {
                    const escTitle = q.title ? q.title.replace(/'/g, "\\'") : '';
                    const escDesc = q.description ? q.description.replace(/(\r\n|\n|\r)/gm, " ").replace(/'/g, "\\'") : '';
                    actions = `
                        <button class="btn btn-secondary btn-sm" onclick="editQuiz(${q.id}, '${escTitle}', '${escDesc}')"><i class="fa-solid fa-pen"></i> Info</button>
                        <button class="btn btn-secondary btn-sm" onclick="manageQuestions(${q.id}, '${escTitle}')"><i class="fa-solid fa-list-check"></i> Questions</button>
                        <button class="btn btn-success btn-sm" onclick="publishQuiz(${q.id})"><i class="fa-solid fa-globe"></i> Publish</button>
                        <button class="btn btn-danger btn-sm" onclick="deleteQuiz(${q.id})"><i class="fa-solid fa-trash"></i></button>
                    `;
                } else {
                    actions = `<span class="text-muted" style="font-size: 0.85rem;"><i class="fa-solid fa-lock"></i> Published & Locked</span>`;
                }
            } else {
                actions = `
                    <button class="btn btn-primary" onclick="takeQuiz(${q.id})"><i class="fa-solid fa-play"></i> Take Quiz</button>
                    <button class="btn btn-secondary" onclick="likeQuiz(${q.id})"><i class="fa-solid fa-heart"></i> ${q.likes || 0}</button>
                `;
            }

            html += `
                <div class="quiz-card">
                    <div class="quiz-card-header">
                        <div class="quiz-title">${q.title}</div>
                        ${isManageView ? badge : ''}
                    </div>
                    <div class="quiz-meta">
                        <span><i class="fa-solid fa-layer-group"></i> ${q.questions ? q.questions.length : (q.questionCount || 0)} Qs</span>
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
