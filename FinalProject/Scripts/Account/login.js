function sendForgotPasswordRequest() {
    var emailVal = $("#Email").val();

    window.location = "/Account/ForgotPassword?email=" + emailVal;
}