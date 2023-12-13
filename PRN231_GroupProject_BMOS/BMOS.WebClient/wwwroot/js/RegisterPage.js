
const imageInput = document.querySelector("#imageInput");
var uploadedImage = "";

imageInput.addEventListener("change", function () {
    const reader = new FileReader();
    reader.addEventListener("load", () => {
        uploadedImage = reader.result;
        document.querySelector("#displayImage").src = uploadedImage;
    });
    reader.readAsDataURL(this.files[0]);
});

function RegisterCustomerAccountForm() {

    var email = $("#email").val();
    var fullName = $("#fullName").val();
    var password = $("#password").val();
    var confirmPassword = $("#confirmPassword").val();
    var address = $("#address").val();
    var phone = $("#phone").val();
    var birthDate = $("#birthDate").val();
    var genderElements = document.getElementsByName("gender");
    var gender = 0;
    for (var i = 0; i < genderElements.length; i++) {
        if (genderElements[i].checked) {
            gender = genderElements[i].value;
        }
    }

    var formData = new FormData();
    formData.append("Email", email);
    formData.append("PasswordHash", password);
    formData.append("FullName", fullName);
    formData.append("Address", address);
    formData.append("Phone", phone);
    formData.append("Avatar", $("#imageInput")[0].files[0]);
    formData.append("Gender", gender == 1 ? true : false);
    formData.append("BirthDate", birthDate);

    if ($("#password").val().length > 0 && $("#confirmPassword").val().length > 0 && $("#password").val() != $("#confirmPassword").val()) {
        ClearErrorForm();
        $("#ConfirmPasswordErrors").append("<br /> Confirm Password does not match with your password.");
    }
    if ($("#birthday").val() == "") {
        ClearErrorForm();
        $("#BirthdayErrors").append("<br /> Birthday is null.");
        $("#BirthdayErrors").append("<br /> Birthday is empty.");
    }

    $(".loading--part").css("display", "flex");
    var request = $.ajax({
        type: "POST",
        url: "http://localhost:5093/odata/customers/register",
        processData: false,
        contentType: false,
        data: formData
    });

    request.done((data, textStatus, jqXHR) => {
        $(".loading--part").css("display", "none");
        sessionStorage.setItem("CreateCustomerAccountSuccessfully", "Create Account Successfully.");
        location.assign("/Login");
    });

    request.fail((jqXHR, textStatus, errorThrown) => {
        $(".loading--part").css("display", "none");
        switch (jqXHR.status) {
            case 400: {
                var error = JSON.parse(jqXHR.responseText);
                if (error.Message.length >= 1 && error.Message[0].FieldNameError != "Exception") {
                    $("#password").val("");
                    $("#confirmPassword").val("");
                    ClearErrorForm();
                    error.Message.forEach((errorDetail) => {
                        switch (errorDetail.FieldNameError.toLowerCase()) {
                            case "fullname": {
                                $("#FullNameErrors").empty();
                                var fullNameErrorHTML = "";
                                errorDetail.DescriptionError.forEach((descriptionErrorDetail) => {
                                    fullNameErrorHTML += `<br />${descriptionErrorDetail}`;
                                });
                                $("#FullNameErrors").append(fullNameErrorHTML);
                                break;
                            }
                            case "address": {
                                $("#AddressErrors").empty();
                                var addressErrorHTML = "";
                                errorDetail.DescriptionError.forEach((descriptionErrorDetail) => {
                                    addressErrorHTML += `<br />${descriptionErrorDetail}`;
                                });
                                $("#AddressErrors").append(addressErrorHTML);
                                break;
                            }
                            case "phone": {
                                $("#PhoneErrors").empty();
                                var phoneErrorHTML = "";
                                errorDetail.DescriptionError.forEach((descriptionErrorDetail) => {
                                    phoneErrorHTML += `<br />${descriptionErrorDetail}`;
                                });
                                $("#PhoneErrors").append(phoneErrorHTML);
                                break;
                            }
                            case "avatar": {
                                $("#AvatarErrors").empty();
                                var avatarErrorHTML = "";
                                errorDetail.DescriptionError.forEach((descriptionErrorDetail) => {
                                    avatarErrorHTML += `<br />${descriptionErrorDetail}`;
                                });
                                $("#AvatarErrors").append(avatarErrorHTML);
                                break;
                            }
                            case "birthdate": {
                                $("#BirthdayErrors").empty();
                                var birthDateErrorHTML = "";
                                errorDetail.DescriptionError.forEach((descriptionErrorDetail) => {
                                    birthDateErrorHTML += `<br />${descriptionErrorDetail}`;
                                });
                                $("#BirthdayErrors").append(birthDateErrorHTML);
                                break;
                            }
                            case "email": {
                                $("#EmailErrors").empty();
                                var emailErrorHTML = "";
                                errorDetail.DescriptionError.forEach((descriptionErrorDetail) => {
                                    emailErrorHTML += `<br />${descriptionErrorDetail}`;
                                });
                                $("#EmailErrors").append(emailErrorHTML);
                                break;
                            }
                            case "passwordhash": {
                                $("#PasswordErrors").empty();
                                var passwordErrorHTML = "";
                                errorDetail.DescriptionError.forEach((descriptionErrorDetail) => {
                                    passwordErrorHTML += `<br />${descriptionErrorDetail}`;
                                });
                                $("#PasswordErrors").append(passwordErrorHTML);
                                break;
                            }
                        }
                    });
                }

                if (error.Message.length == 1 && error.Message[0].FieldNameError == "Exception") {
                    if (error.Message[0].FieldNameError.toLowerCase() == "email") {
                        ClearErrorForm();
                        var emailErrorHTML = `<br />${error.Message[0].DescriptionError[0]}`;
                        $("#EmailErrors").append(emailErrorHTML);
                    } else if (error.Message[0].FieldNameError.toLowerCase() == "phone") {
                        ClearErrorForm();
                        var phoneErrorHTML = `<br />${error.Message[0].DescriptionError[0]}`;
                        $("#PhoneErrors").append(phoneErrorHTML);
                    } else if (error.Message[0].FieldNameError.toLowerCase() == "exception") {
                        Swal.fire({
                            icon: 'error',
                            title: 'Oops...',
                            text: error.Message[0].DescriptionError[0]
                        });
                    }
                }
                break;
            }
            default: {
                console.error(jqXHR);
                console.error(jqXHR.responseText);
                console.error(jqXHR.status);
                console.error(errorThrown);
                console.error(textStatus);
                var error = JSON.parse(jqXHR.responseText);
                Swal.fire({
                    icon: 'error',
                    title: 'Oops...',
                    text: error.Message[0].DescriptionError[0]
                });
                break;
            }
        }
    });



    return false;
}

function ClearErrorForm() {
    $("#ConfirmPasswordErrors").empty();
    $("#FullNameErrors").empty();
    $("#AddressErrors").empty();
    $("#PasswordErrors").empty();
    $("#EmailErrors").empty();
    $("#BirthdayErrors").empty();
    $("#PhoneErrors").empty();
    $("#AvatarErrors").empty();
    $("#password").val("");
    $("#confirmPassword").val("");
}