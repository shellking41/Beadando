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
    $('#createQuestionLink').on('click', showCreateQuestion);
    $('#submitAnswer').on('click', handleAnswerSubmission);
    $('#createQuestionForm').on('submit', handleQuestionCreation);
    $('#addAnswerBtn').on('click', addAnswerField);
    $('#createFirstQuestionBtn').on('click', showCreateQuestion);

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
    $('#loginForm, #registerForm, #mainContent, #createQuestionSection, #noQuestionsWarning').addClass('d-none');
    $(sectionId).removeClass('d-none');
}

function updateNavigation(isAuthenticated) {
    if (isAuthenticated) {
        $('#loginNav, #registerNav').addClass('d-none');
        $('#logoutNav, #startQuizNav, #createQuestionNav, #resultsNav').removeClass('d-none');
        $('#mainContent').removeClass('d-none');
        $('#loginForm, #registerForm').addClass('d-none');
    } else {
        $('#loginNav, #registerNav').removeClass('d-none');
        $('#logoutNav, #startQuizNav, #createQuestionNav, #resultsNav').addClass('d-none');
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
        $('#quizSection, #resultsSection, #createQuestionSection').addClass('d-none');
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
    $('#welcomeSection, #quizSection, #createQuestionSection').addClass('d-none');
}

function showCreateQuestion() {
    showSection('#mainContent');
    $('#createQuestionSection').removeClass('d-none');
    $('#welcomeSection, #quizSection, #resultsSection').addClass('d-none');
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
                $('#welcomeSection, #resultsSection, #createQuestionSection').addClass('d-none');
                
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

    // Kérdés szövegének megjelenítése
    $('#questionText').text(question.text);
    
    // Kép megjelenítése, ha van
    let questionContent = question.text;
    if (question.image) {
        const imageName = question.image.split('/').pop(); // Csak a fájlnév
        const imageDir = question.image.split('/')[0]; // Az első mappanév (pl. bteszt4)
        let imagePath;
        
        // Ha btesztX/ kezdetű
        if (imageDir.startsWith('bteszt')) {
            imagePath = `bteszt/${imageDir}/${imageName}`; // pl. bteszt/bteszt1/kep.gif
        }
        // Ha atesztX/ kezdetű
        else if (imageDir.startsWith('ateszt')) {
            imagePath = `ateszt/${imageDir}/${imageName}`; // pl. ateszt/ateszt1/kep.gif
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
                <tr>
                    <td>${date.toLocaleString()}</td>
                    <td>${result.score}/${result.totalQuestions}</td>
                    <td>${percentage}%</td>
                    <td>${result.passed ? 'Sikeres' : 'Sikertelen'}</td>
                </tr>
            `;
        })
        .join('');
    
    $('#resultsTableBody').html(resultsHtml || '<tr><td colspan="4" class="text-center">Nincsenek még eredmények.</td></tr>');
}

function addAnswerField() {
    const answerCount = $('.answer-group').length;
    if (answerCount >= 4) {
        alert('Maximum 4 válasz lehetséges!');
        return;
    }

    const newAnswerHtml = `
        <div class="answer-group mb-3">
            <div class="input-group">
                <input type="text" class="form-control answer-text" placeholder="Válasz szövege" required>
                <div class="input-group-text">
                    <input type="radio" name="correctAnswer" class="correct-answer" required>
                    <label class="ms-2 mb-0">Helyes válasz</label>
                </div>
            </div>
        </div>
    `;
    
    $('#answersContainer').append(newAnswerHtml);
}

async function handleQuestionCreation(event) {
    event.preventDefault();

    const questionData = {
        text: $('#createQuestionText').val(),
        image: $('#questionImage').val() || null,
        answers: []
    };

    $('.answer-group').each(function(index) {
        const answerText = $(this).find('.answer-text').val();
        const isCorrect = $(this).find('.correct-answer').prop('checked');
        
        questionData.answers.push({
            text: answerText,
            isCorrect: isCorrect
        });
    });

    if (questionData.answers.length < 2) {
        alert('Legalább két válasz szükséges!');
        return;
    }

    if (!questionData.answers.some(a => a.isCorrect)) {
        alert('Legalább egy helyes választ meg kell jelölni!');
        return;
    }

    try {
        const response = await fetch('/api/Question', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(questionData),
            credentials: 'include'
        });

        if (response.ok) {
            alert('Kérdés sikeresen létrehozva!');
            $('#createQuestionForm')[0].reset();
        } else {
            const error = await response.json();
            alert(error.message || 'Hiba történt a kérdés létrehozása során!');
        }
    } catch (error) {
        console.error('Error creating question:', error);
        alert('Hiba történt a kérdés létrehozása során!');
    }
} 