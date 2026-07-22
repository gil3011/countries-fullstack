const develop_mode = true; //change when developing/publishing

const ruppin_path = "https://proj.ruppin.ac.il/cgroup24/test2/tar1";
const local_path = "https://localhost:7255"

const BASE_API = develop_mode ? local_path : ruppin_path;

const API_ROUTES = {
    countryApi: BASE_API + "/api/Country",
    usersApi: BASE_API + "/api/User"
};