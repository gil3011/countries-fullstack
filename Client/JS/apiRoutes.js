const develop_mode = true; //change when developing/publishing

const ruppin_path = "https://proj.ruppin.ac.il/cgroup24/test2/tar1";
const local_path = "https://localhost:7255"

const BASE_API = develop_mode ? local_path : ruppin_path;

const API_ROUTES = {
    quizAPI: BASE_API + "/Quiz",
    countryAPI: BASE_API + "/Country",
    quizAttemptAPI: BASE_API + "/QuizAttempt",
    shareAPI: BASE_API + "/Share",
    userAPI: BASE_API + "/User"
};