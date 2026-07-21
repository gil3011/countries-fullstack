function renderUsers() {
    ajaxCall("GET", userAPI, null, success, failed);
}

function renderAdminsStats() {
    ajaxCall("GET", "https://localhost:7255/api/User/admin/stats" , null, success, failed);
}

//userAPI + "/admin/stats"

function getDailyLoginCounts() {
    ajaxCall("GET", userAPI + "/admin/GetDailyLoginCounts", null, success, failed);
}


$(document).ready(function () {
    renderUsers();
    renderAdminsStats();
    getDailyLoginCounts()
});


function success(data) {
    console.log(data);
}

function failed(error) {
    alert("not oved");
    console.log(error);
}