let currentUser = null;
let currentQuiz = null;
let currentQuestionIndex = 0;
let userAnswers = [];
let currentQuizResultId = null;

$(document).ready(function() {
    checkAuthStatus();
    setupEventListeners();
    
    $('#showRegisterForm').on('click', function(e) {
        e.preventDefault();
        $('#loginForm').addClass('d-none');
        $('#registerForm').removeClass('d-none');
    });

    $('#showLoginForm').on('click', function(e) {
        e.preventDefault();
        $('#registerForm').addClass('d-none');
        $('#loginForm').removeClass('d-none');
    });
});

function setupEventListeners() {
    $('#loginForm').on('submit', handleLogin);
    $('#registerForm').on('submit', handleRegister);
    $('#logoutLink').on('click', handleLogout);
    $('#homeLink').on('click', showHome);
    $('#loginLink').on('click', showLoginForm);
    $('#registerLink').on('click', showRegisterForm);
    $('#newQuizBtn, #startQuizLink').on('click', startNewQuiz);
    $('#viewResultsBtn, #resultsLink').on('click', showResults);
    $('#submitAnswer').on('click', handleAnswerSubmission);

    $(document).on('change', '.answer-radio', function() {
        $('#submitAnswer').prop('disabled', false);
    });
}

function setupQuizEventListeners() {
    $('#submitAnswer').off('click').on('click', handleAnswerSubmission);
    $(document).off('change', '.answer-radio').on('change', '.answer-radio', function() {
        $('#submitAnswer').prop('disabled', false);
    });
}

function showSection(sectionId) {
    $('#loginForm, #registerForm, #mainContent').addClass('d-none');
    $(sectionId).removeClass('d-none');
}

function updateNavigation(isAuthenticated) {
    if (isAuthenticated) {
        $('#loginNav, #registerNav').addClass('d-none');
        $('#logoutNav, #startQuizNav, #resultsNav').removeClass('d-none');
        $('#mainContent').removeClass('d-none');
        $('#loginForm, #registerForm').addClass('d-none');
    } else {
        $('#loginNav, #registerNav').removeClass('d-none');
        $('#logoutNav, #startQuizNav, #resultsNav').addClass('d-none');
        $('#mainContent').addClass('d-none');
        showLoginForm();
    }
}

function showLoginForm() {
    showSection('#loginForm');
}

function showRegisterForm() {
    showSection('#registerForm');
}

function showHome() {
    if (currentUser) {
        showSection('#mainContent');
        $('#welcomeSection').removeClass('d-none');
        $('#quizSection, #resultsSection').addClass('d-none');
        if (currentUser.name) {
            $('#userContent').text(`Bejelentkezve mint: ${currentUser.name}`);
        } else {
            $('#userContent').text('');
        }
    } else {
        showLoginForm();
    }
}

function showResults() {
    loadResults();
    showSection('#mainContent');
    $('#resultsSection').removeClass('d-none');
    $('#welcomeSection, #quizSection').addClass('d-none');
}

function resetQuizState() {
    currentQuiz = null;
    currentQuestionIndex = 0;
    currentQuizResultId = null;
    userAnswers = [];
    $('#quizContent').html(`
        <div class="card mb-4">
            <div class="card-body">
                <h5 class="card-title" id="questionText"></h5>
                <div id="answerOptions" class="list-group"></div>
            </div>
        </div>
        <button class="btn btn-primary" id="submitAnswer" disabled>Következő kérdés</button>
    `);
    setupQuizEventListeners();
}

async function handleLogout() {
    try {
        const response = await fetch('/api/Session/logout', {
            method: 'POST',
            credentials: 'include'
        });

        if (response.ok) {
           window.location.reload();    
        }
    } catch (error) {
        console.error('Logout error:', error);
    }
}

async function checkAuthStatus() {
    try {
        const response = await fetch('/api/Session/validate', {
            method: 'GET',
            headers: {
                'Accept': 'application/json'
            },
            credentials: 'include'
        });

        if (response.ok) {
            const userData = await response.json();
            if (userData) {
                currentUser = userData;
                if (window.location.pathname === '/signup.html') {
                    window.location.href = '/';
                } else {
                    updateNavigation(true);
                    showHome();
                }
            } else {
                currentUser = null;
                if (window.location.pathname !== '/signup.html') {
                    window.location.href = '/signup.html';
                }
            }
        } else {
            currentUser = null;
            if (window.location.pathname !== '/signup.html') {
                window.location.href = '/signup.html';
            }
        }
    } catch (error) {
        console.error('Error checking auth status:', error);
        currentUser = null;
        if (window.location.pathname !== '/signup.html') {
            window.location.href = '/signup.html';
        }
    }
}

function validateLoginForm() {
    const email = $('#loginEmail').val().trim();
    const password = $('#loginPassword').val();
    let isValid = true;
    let errors = [];

    $('.validation-error').remove();
    $('.is-invalid').removeClass('is-invalid');

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
        isValid = false;
        $('#loginEmail').addClass('is-invalid');
        errors.push('Kérjük, adjon meg egy érvényes email címet.');
    }

    if (password.length === 0) {
        isValid = false;
        $('#loginPassword').addClass('is-invalid');
        errors.push('A jelszó megadása kötelező.');
    }

    if (!isValid) {
        const errorHtml = errors.map(error => 
            `<div class="alert alert-danger validation-error mb-2">${error}</div>`
        ).join('');
        $('#loginForm').prepend(errorHtml);
    }

    return isValid;
}

async function handleLogin(event) {
    event.preventDefault();
    
    $('.validation-error').remove();

    if (!validateLoginForm()) {
        return;
    }
    
    const loginData = {
        email: $('#loginEmail').val().trim(),
        password: $('#loginPassword').val()
    };

    try {
        const response = await fetch('/api/Session/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(loginData),
            credentials: 'include'
        });

        const data = await response.json();

        if (response.ok) {
            currentUser = data;
            window.location.href = '/';
        } else {
            const errorHtml = `<div class="alert alert-danger validation-error mb-2">${data.message || 'Hibás bejelentkezési adatok!'}</div>`;
            $('#loginForm').prepend(errorHtml);
        }
    } catch (error) {
        console.error('Login error:', error);
        const errorHtml = '<div class="alert alert-danger validation-error mb-2">Hiba történt a bejelentkezés során!</div>';
        $('#loginForm').prepend(errorHtml);
    }
}

function validateRegistrationForm() {
    const name = $('#registerName').val().trim();
    const email = $('#registerEmail').val().trim();
    const password = $('#registerPassword').val();
    let isValid = true;
    let errors = [];

    $('.validation-error').remove();
    $('.is-invalid').removeClass('is-invalid');

    if (name.length < 2) {
        isValid = false;
        $('#registerName').addClass('is-invalid');
        errors.push('A név legalább 2 karakter hosszú kell legyen.');
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
        isValid = false;
        $('#registerEmail').addClass('is-invalid');
        errors.push('Kérjük, adjon meg egy érvényes email címet.');
    }

    if (password.length < 6) {
        isValid = false;
        $('#registerPassword').addClass('is-invalid');
        errors.push('A jelszó legalább 6 karakter hosszú kell legyen.');
    }

    if (!isValid) {
        const errorHtml = errors.map(error => 
            `<div class="alert alert-danger validation-error mb-2">${error}</div>`
        ).join('');
        $('#registerForm').prepend(errorHtml);
    }

    return isValid;
}

async function handleRegister(event) {
    event.preventDefault();
    
    $('.validation-error').remove();

    if (!validateRegistrationForm()) {
        return;
    }
    
    const registerData = {
        name: $('#registerName').val().trim(),
        email: $('#registerEmail').val().trim(),
        password: $('#registerPassword').val()
    };

    try {
        const response = await fetch('/api/Session/register', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(registerData),
            credentials: 'include'
        });

        const data = await response.json();

        if (response.ok) {
            alert(data.message || 'Sikeres regisztráció! Most már bejelentkezhet.');
            $('#registerForm')[0].reset();
            $('#registerForm').addClass('d-none');
            $('#loginForm').removeClass('d-none');
        } else {
            const errorHtml = `<div class="alert alert-danger validation-error mb-2">${data.message || 'Hiba a regisztráció során!'}</div>`;
            $('#registerForm').prepend(errorHtml);
        }
    } catch (error) {
        console.error('Registration error:', error);
        const errorHtml = '<div class="alert alert-danger validation-error mb-2">Hiba történt a regisztráció során!</div>';
        $('#registerForm').prepend(errorHtml);
    }
}

async function startNewQuiz() {
    console.log('Starting new quiz...');
    try {
        resetQuizState();

        const response = await fetch('/api/Quiz/start', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            credentials: 'include'
        });

        console.log('Quiz start response:', response);

        if (response.ok) {
            const quizResponse = await response.json();
            console.log('Quiz response data:', quizResponse);
            
            if (quizResponse.questions && quizResponse.questions.length > 0) {
                currentQuiz = quizResponse;
                currentQuestionIndex = 0;
                currentQuizResultId = quizResponse.userQuizResultId;
                
                showSection('#mainContent');
                $('#quizSection').removeClass('d-none');
                $('#welcomeSection, #resultsSection').addClass('d-none');
                
                showQuestion();
            } else {
                $('#noQuestionsWarning').removeClass('d-none');
                $('#welcomeSection, #quizSection').addClass('d-none');
            }
        } else {
            const errorData = await response.json();
            throw new Error(errorData.message || 'Hiba történt a teszt indítása során!');
        }
    } catch (error) {
        console.error('Error starting quiz:', error);
        alert(error.message || 'Hiba történt a teszt indítása során!');
    }
}

function showQuestion() {
    if (!currentQuiz || !currentQuiz.questions || currentQuestionIndex >= currentQuiz.questions.length) {
        console.error('Invalid quiz state:', { currentQuiz, currentQuestionIndex });
        return;
    }

    const question = currentQuiz.questions[currentQuestionIndex];
    console.log('Showing question:', question);

  
    $('#questionText').text(question.text);
    
   
    let questionContent = question.text;
    if (question.image) {
        const imageName = question.image.split('/').pop(); 
        const imageDir = question.image.split('/')[0]; 
        let imagePath;
        
       
        if (imageDir.startsWith('bteszt')) {
            imagePath = `bteszt/${imageDir}/${imageName}`;
        }
      
        else if (imageDir.startsWith('ateszt')) {
            imagePath = `ateszt/${imageDir}/${imageName}`; 
        }
        
        questionContent = `
            <div class="mb-3">
                <img src="/img/${imagePath}" alt="Kérdéshez tartozó kép" class="img-fluid rounded" 
                     onerror="this.style.display='none'">
            </div>
            <div>${question.text}</div>
        `;
    }
    $('#questionText').html(questionContent);
    
    const answersHtml = question.answers.map(answer => `
        <div class="list-group-item">
            <div class="form-check">
                <input class="form-check-input answer-radio" type="radio" name="answer" value="${answer.id}">
                <label class="form-check-label">${answer.text}</label>
            </div>
        </div>
    `).join('');
    
    $('#answerOptions').html(answersHtml);
    $('#submitAnswer').prop('disabled', true);

    if (currentQuestionIndex === currentQuiz.questions.length - 1) {
        $('#submitAnswer').text('Teszt befejezése');
    } else {
        $('#submitAnswer').text('Következő kérdés');
    }
    
    setupQuizEventListeners();
}

async function handleAnswerSubmission() {
    const selectedAnswerId = $('input[name="answer"]:checked').val();
    if (!selectedAnswerId) return;

    const currentQuestion = currentQuiz.questions[currentQuestionIndex];
    userAnswers.push({
        questionId: currentQuestion.id,
        answerId: parseInt(selectedAnswerId)
    });

    if (currentQuestionIndex === currentQuiz.questions.length - 1) {
        await submitQuiz();
    } else {
        currentQuestionIndex++;
        showQuestion();
    }
}

async function submitQuiz() {
    try {
        const response = await fetch('/api/Quiz/submit', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(userAnswers),
            credentials: 'include'
        });

        if (response.ok) {
            const result = await response.json();
            displayQuizResult(result);
        } else {
            alert('Hiba történt a teszt beküldése során!');
        }
    } catch (error) {
        console.error('Error submitting quiz:', error);
        alert('Hiba történt a teszt beküldése során!');
    }
}

function displayQuizResult(result) {
    const resultHtml = `
        <div class="card">
            <div class="card-body">
                <h3>Teszt eredménye</h3>
                <p>Pontszám: ${result.score}/${result.totalQuestions}</p>
                <p>Eredmény: ${result.passed ? 'Sikeres' : 'Sikertelen'}</p>
                <div class="mt-3">
                    <button class="btn btn-primary me-2" onclick="clearAndStartNewQuiz()">Új teszt indítása</button>
                    <button class="btn btn-secondary" onclick="clearAndShowHome()">Vissza a főoldalra</button>
                </div>
            </div>
        </div>
    `;
    
    $('#quizContent').html(resultHtml);
}

function clearAndStartNewQuiz() {
    $('#quizContent').empty();
    currentQuiz = null;
    currentQuestionIndex = 0;
    currentQuizResultId = null;
    userAnswers = [];
    startNewQuiz();
}

function clearAndShowHome() {
    $('#quizContent').empty();
    currentQuiz = null;
    currentQuestionIndex = 0;
    currentQuizResultId = null;
    userAnswers = [];
    showHome();
}

async function loadResults() {
    try {
        const response = await fetch('/api/Quiz/results', {
            method: 'GET',
            headers: {
                'Accept': 'application/json'
            },
            credentials: 'include'
        });

        if (response.ok) {
            const results = await response.json();
            if (Array.isArray(results)) {
                displayResults(results);
            } else {
                console.error('Unexpected results format:', results);
                $('#resultsTableBody').html('<tr><td colspan="4" class="text-center">Nem sikerült betölteni az eredményeket.</td></tr>');
            }
        } else {
            const errorData = await response.json();
            console.error('Error loading results:', errorData);
            $('#resultsTableBody').html('<tr><td colspan="4" class="text-center">Hiba történt az eredmények betöltése során.</td></tr>');
        }
    } catch (error) {
        console.error('Error loading results:', error);
        $('#resultsTableBody').html('<tr><td colspan="4" class="text-center">Hiba történt az eredmények betöltése során.</td></tr>');
    }
}

function displayResults(results) {
    if (!results || results.length === 0) {
        $('#resultsTableBody').html('<tr><td colspan="4" class="text-center">Nincsenek még eredmények.</td></tr>');
        return;
    }

    const resultsHtml = results
        .filter(result => result.completedAt)
        .map(result => {
            const date = new Date(result.completedAt);
            const percentage = Math.round((result.score / result.totalQuestions) * 100);
            
            return `
                <tr class="result-row" data-result-id="${result.id}">
                    <td>${date.toLocaleString()}</td>
                    <td>${result.score}/${result.totalQuestions}</td>
                    <td>${percentage}%</td>
                    <td>${result.passed ? 'Sikeres' : 'Sikertelen'}</td>
                    <td>
                        <button class="btn btn-sm btn-info show-details-btn" data-quiz-id="${result.id}">
                            Részletek
                        </button>
                    </td>
                </tr>
                <tr class="details-row d-none" id="details-${result.id}">
                    <td colspan="5">
                        <div class="details-content p-3">
                            <div class="text-center">
                                <div class="spinner-border text-primary d-none" role="status">
                                    <span class="visually-hidden">Betöltés...</span>
                                </div>
                            </div>
                            <div class="quiz-details-content"></div>
                        </div>
                    </td>
                </tr>
            `;
        })
        .join('');
    
    $('#resultsTableBody').html(resultsHtml || '<tr><td colspan="5" class="text-center">Nincsenek még eredmények.</td></tr>');

   
    $('.show-details-btn').on('click', async function() {
        const quizId = $(this).data('quiz-id');
        const resultRow = $(this).closest('tr');
        const detailsRow = $(`#details-${quizId}`);
        const spinner = detailsRow.find('.spinner-border');
        const detailsContent = detailsRow.find('.quiz-details-content');
        
       
        detailsRow.toggleClass('d-none');
        
        
        const btnText = detailsRow.hasClass('d-none') ? 'Részletek' : 'Bezárás';
        $(this).text(btnText);

        
        if (!detailsRow.hasClass('d-none') && !detailsContent.data('loaded')) {
            try {
                spinner.removeClass('d-none');
                const details = await loadQuizDetails(quizId);
                displayQuizDetails(details, detailsContent);
                detailsContent.data('loaded', true);
            } catch (error) {
                detailsContent.html('<div class="alert alert-danger">Hiba történt a részletek betöltése során.</div>');
                console.error('Error loading quiz details:', error);
            } finally {
                spinner.addClass('d-none');
            }
        }
    });
}

async function loadQuizDetails(quizId) {
    const response = await fetch(`/api/Quiz/details/${quizId}`, {
        method: 'GET',
        headers: {
            'Accept': 'application/json'
        },
        credentials: 'include'
    });

    if (!response.ok) {
        throw new Error('Failed to load quiz details');
    }

    return await response.json();
}

function displayQuizDetails(details, container) {
    const questionsHtml = details.questions.map((q, index) => {
     
        let imagePath = '';
        if (q.questionImage) {
            const parts = q.questionImage.split('/');
            if (parts.length >= 2) {
                const imageDir = parts[0]; 
                const imageName = parts[1]; 
                
                const baseDir = imageDir.replace(/[0-9]+$/, ''); 
                const number = imageDir.match(/\d+$/)?.[0] || '1'; 
                
                imagePath = `/img/${baseDir}/${baseDir}${number}/${imageName}`;
            }
        }

        return `
            <div class="question-details card mb-3 ${q.isCorrect ? 'border-success' : 'border-danger'}">
                <div class="card-header ${q.isCorrect ? 'bg-success text-white' : 'bg-danger text-white'}">
                    <strong>${index + 1}. kérdés</strong>
                    <span class="float-end">
                        ${q.isCorrect 
                            ? '<i class="bi bi-check-circle-fill"></i> Helyes válasz' 
                            : '<i class="bi bi-x-circle-fill"></i> Helytelen válasz'}
                    </span>
                </div>
                <div class="card-body">
                    <h5 class="card-title">${q.questionText}</h5>
                    ${imagePath ? `
                        <div class="text-center mb-3">
                            <img src="${imagePath}" 
                                 alt="Kérdéshez tartozó kép" 
                                 class="img-fluid rounded question-image"
                                 style="max-height: 200px; object-fit: contain;"
                                 onerror="this.style.display='none'">
                        </div>
                    ` : ''}
                    <div class="answer-section">
                        <p class="mb-2">
                            <strong>Az Ön válasza:</strong> 
                            <span class="${q.isCorrect ? 'text-success' : 'text-danger'}">
                                ${q.userAnswerText}
                            </span>
                        </p>
                        ${!q.isCorrect ? `
                            <p class="mb-0">
                                <strong>Helyes válasz:</strong> 
                                <span class="text-success">${q.correctAnswerText}</span>
                            </p>
                        ` : ''}
                    </div>
                </div>
            </div>
        `;
    }).join('');

    container.html(`
        <div class="quiz-details">
            <div class="quiz-summary card mb-4">
                <div class="card-body">
                    <h5 class="card-title">Teszt összegzés</h5>
                    <div class="row">
                        <div class="col-md-4">
                            <p class="mb-2"><strong>Pontszám:</strong> ${details.score}/${details.totalQuestions}</p>
                        </div>
                        <div class="col-md-4">
                            <p class="mb-2"><strong>Százalék:</strong> ${Math.round((details.score / details.totalQuestions) * 100)}%</p>
                        </div>
                        <div class="col-md-4">
                            <p class="mb-2">
                                <strong>Eredmény:</strong> 
                                <span class="${details.passed ? 'text-success' : 'text-danger'}">
                                    ${details.passed ? 'Sikeres' : 'Sikertelen'}
                                </span>
                            </p>
                        </div>
                    </div>
                </div>
            </div>
            <h5 class="mb-3">Kérdések és válaszok</h5>
            ${questionsHtml}
        </div>
    `);
} 