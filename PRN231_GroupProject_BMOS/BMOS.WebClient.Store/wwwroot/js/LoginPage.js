
window.onload = () => {
    var CustomerIdNotFound = sessionStorage.getItem("CustomerIdNotFound");
    if (CustomerIdNotFound != null) {
        Swal.fire({
            icon: 'error',
            title: 'Oops...',
            text: CustomerIdNotFound
        });
        sessionStorage.removeItem("CustomerIdNotFound");
    }
}

const showHiddenPass = (loginPass, loginEye) => {
    const input = document.getElementById(loginPass),
        iconEye = document.getElementById(loginEye);

    iconEye.addEventListener('click', () => {
        if (input.type === 'password') {
            input.type = 'text';

            iconEye.classList.add('fa-eye');
            iconEye.classList.remove('fa-eye-slash');
        } else {
            input.type = 'password'
            iconEye.classList.remove('fa-eye');
            iconEye.classList.add('fa-eye-slash');
        }
    });
}

showHiddenPass('login-pass', 'login-eye');

function LoginForm() {

    var account = {
        Email: $("#login-email").val(),
        PasswordHash: $("#login-pass").val()
    };

    $(".loading--part").css("display", "flex");
    var request = $.ajax({
        type: "POST",
        url: "http://localhost:5093/odata/authentications/login",
        dataType: "json",
        contentType: "application/json",
        data: JSON.stringify(account)
    });

    request.done((data, textStatus, jqXHR) => {
        $(".loading--part").css("display", "none");
        sessionStorage.setItem("LoginSuccessfully", "Login Successfully.");
        if (data.role.toLowerCase() == "store owner") {
            localStorage.setItem("StoreOwner", JSON.stringify(data));
            location.assign("/StoreOWner/Dashboard");
        } else if (data.role.toLowerCase() == "staff") {
            localStorage.setItem("Staff", JSON.stringify(data));
            location.assign("/Staff/Dashboard");
        }
    });

    request.fail((jqXHR, textStatus, errorThrown) => {
        $(".loading--part").css("display", "none");
        var error = JSON.parse(jqXHR.responseText);
        $("#login-pass").val("");
        switch (error.StatusCode) {
            case 400: {
                if (error.Message.length >= 1 && error.Message[0].FieldNameError.toLowerCase() != "exception") {
                    $(".Errors").empty();
                    $("#EmailErrors").empty();
                    var emailErrorsHTML = "";

                    $("#PasswordErrors").empty();
                    var passwordErrorHTML = "";
                    error.Message.forEach((errorDetail) => {
                        switch (errorDetail.FieldNameError.toLowerCase()) {
                            case "email": {
                                errorDetail.DescriptionError.forEach((descriptionErrorDetail) => {
                                    emailErrorsHTML += descriptionErrorDetail + "<br />";
                                });
                                $("#EmailErrors").html(emailErrorsHTML);
                                break;
                            } case "passwordhash": {
                                errorDetail.DescriptionError.forEach((descriptionErrorDetail) => {
                                    passwordErrorHTML += descriptionErrorDetail + "<br />";
                                });
                                $("#PasswordErrors").html(passwordErrorHTML);
                                break;
                            }
                        }
                    });
                } else {
                    $(".Errors").empty();
                    $("#EmailErrors").empty();
                    $("#PasswordErrors").empty();
                    var errorsHTML = error.Message[0].DescriptionError[0];
                    $(".Errors").append(errorsHTML);
                }
                break;
            }
            default: {
                $(".Errors").empty();
                $("#EmailErrors").empty();
                $("#PasswordErrors").empty();
                var errorsHTML = error.Message[0].DescriptionError[0];
                $(".Errors").append(errorsHTML);
                break;
            }
        }
    });



    return false;
}