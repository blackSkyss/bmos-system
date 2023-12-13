

window.onload = () => {
    var CreateCustomerAccountSuccessfully = sessionStorage.getItem("CreateCustomerAccountSuccessfully");
    if (CreateCustomerAccountSuccessfully != null) {
        Swal.fire({
            position: 'top-end',
            icon: 'success',
            title: CreateCustomerAccountSuccessfully,
            showConfirmButton: false,
            timer: 1000
        });
        sessionStorage.removeItem("CreateCustomerAccountSuccessfully");
    }
}

function LoginForm() {
    var account = {
        Email: $("#email").val(),
        PasswordHash: $("#password").val()
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
        if (data.role.toLowerCase() == "customer") {
            localStorage.setItem("Customer", JSON.stringify(data));
            location.assign("/LandingPage");
        }
    });

    request.fail((jqXHR, textStatus, errorThrown) => {
        $(".loading--part").css("display", "none");
        var error = JSON.parse(jqXHR.responseText);
        $("#password").val("");
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
                    $("#LoginErrors").empty();
                    $("#EmailErrors").empty();
                    $("#PasswordErrors").empty();
                    var errorsHTML = error.Message[0].DescriptionError[0];
                    $("#LoginErrors").append(errorsHTML);
                }
                break;
            }
            default: {
                $("#LoginErrors").empty();
                $("#EmailErrors").empty();
                $("#PasswordErrors").empty();
                var errorsHTML = error.Message[0].DescriptionError[0];
                $("#LoginErrors").append(errorsHTML);
                break;
            }
        }
    });
    return false;
}