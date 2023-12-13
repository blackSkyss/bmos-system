

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


window.onload = () => {
    let storeOwnerJson = localStorage.getItem("StoreOwner");
    let storeOwner = JSON.parse(storeOwnerJson);
    if (storeOwner != null) {
        $("#storeOwnerName").append(`${storeOwner.fullName}`);
    } else {
        location.assign("/Login");
    }
};

function CreateStaffForm() {
    let storeOwnerJson = localStorage.getItem("StoreOwner");
    let storeOwner = JSON.parse(storeOwnerJson);

    let formData = new FormData();

    var genderElements = document.getElementsByName("gender");
    var gender = 0;
    for (var i = 0; i < genderElements.length; i++) {
        if (genderElements[i].checked) {
            gender = genderElements[i].value;
        }
    }

    formData.append("FullName", $("#fullName").val());
    formData.append("Address", $("#address").val());
    formData.append("Phone", $("#phone").val());
    formData.append("Avatar", $("#imageInput")[0].files[0]);
    formData.append("Gender", gender == 1 ? true : false);
    formData.append("BirthDate", $("#birthday").val());
    formData.append("IdentityNumber", $("#identityNumber").val());
    formData.append("Account.Email", $("#emailAddress").val());
    formData.append("Account.PasswordHash", $("#password").val());

    if ($("#password").val().length > 0 && $("#confirmPassword").val().length > 0 && $("#password").val() != $("#confirmPassword").val()) {
        ClearErrorForm();
        $("#ConfirmPasswordError").append("<br /> Confirm Password does not match with your password.");
    }
    if ($("#birthday").val() == "") {
        ClearErrorForm();
        $("#BirthDateErrors").append("<br /> Birthday is null.");
        $("#BirthDateErrors").append("<br /> Birthday is empty.");
    }

    $(".loading--part").css("display", "flex");
    var request = $.ajax({
        type: "POST",
        url: "http://localhost:5093/odata/staffs",
        processData: false,
        contentType: false,
        data: formData,
        headers: { Authorization: `Bearer ${storeOwner.accessToken}` }
    });

    request.done((data, textStatus, jqXHR) => {
        $(".loading--part").css("display", "none");
        sessionStorage.setItem("CreateStaffSuccessfully", "Create Staff Successfully.");
        location.assign("/StoreOwner/Staffs");
    });

    request.fail((jqXHR, textStatus, errorThrown) => {
        $(".loading--part").css("display", "none");
        switch (jqXHR.status) {
            case 400: {
                var error = JSON.parse(jqXHR.responseText);
                if (error.Message.length >= 1 && error.Message[0].FieldNameError != "Exception") {
                    $("#password").val("");
                    $("#confirmPassword").val("");
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
                                $("#BirthDateErrors").empty();
                                var birthDateErrorHTML = "";
                                errorDetail.DescriptionError.forEach((descriptionErrorDetail) => {
                                    birthDateErrorHTML += `<br />${descriptionErrorDetail}`;
                                });
                                $("#BirthDateErrors").append(birthDateErrorHTML);
                                break;
                            }
                            case "identitynumber": {
                                $("#IdentityNumberErrors").empty();
                                var identityNumberErrorHTML = "";
                                errorDetail.DescriptionError.forEach((descriptionErrorDetail) => {
                                    identityNumberErrorHTML += `<br />${descriptionErrorDetail}`;
                                });
                                $("#IdentityNumberErrors").append(identityNumberErrorHTML);
                                break;
                            }
                            case "account.email": {
                                $("#EmailErrors").empty();
                                var emailErrorHTML = "";
                                errorDetail.DescriptionError.forEach((descriptionErrorDetail) => {
                                    emailErrorHTML += `<br />${descriptionErrorDetail}`;
                                });
                                $("#EmailErrors").append(emailErrorHTML);
                                break;
                            }
                            case "account.passwordhash": {
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
                    if (error.Message[0].FieldNameError.toLowerCase() == "birthdate") {
                        ClearErrorForm();
                        var birthDateErrorHTML = `<br />${error.Message[0].DescriptionError[0]}`;
                        $("#BirthDateErrors").append(birthDateErrorHTML);
                    } else if (error.Message[0].FieldNameError.toLowerCase() == "email") {
                        ClearErrorForm();
                        var emailErrorHTML = `<br />${error.Message[0].DescriptionError[0]}`;
                        $("#EmailErrors").append(emailErrorHTML);
                    } else if (error.Message[0].FieldNameError.toLowerCase() == "phone") {
                        ClearErrorForm();
                        var phoneErrorHTML = `<br />${error.Message[0].DescriptionError[0]}`;
                        $("#PhoneErrors").append(phoneErrorHTML);
                    } else if (error.Message[0].FieldNameError.toLowerCase() == "identitynumber") {
                        ClearErrorForm();
                        var identityNumberErrorHTML = `<br />${error.Message[0].DescriptionError[0]}`;
                        $("#IdentityNumberErrors").append(identityNumberErrorHTML);
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
            case 401: {
                var tokens = {
                    AccessToken: storeOwner.accessToken,
                    RefreshToken: storeOwner.refreshToken
                };
                var regenerateTokenRequest = $.ajax({
                    type: "POST",
                    url: "http://localhost:5093/odata/authentications/recreate-token",
                    dataType: "json",
                    contentType: "application/json",
                    data: JSON.stringify(tokens),
                });

                regenerateTokenRequest.done((data, textStatus, jqXHR) => {
                    storeOwner.accessToken = data.accessToken;
                    storeOwner.refreshToken = data.refreshToken;
                    localStorage.setItem("StoreOwner", JSON.stringify(storeOwner));
                    Swal.fire({
                        icon: 'error',
                        title: 'Oops...',
                        text: "There are some errors in the processing. Please try again!"
                    }).then(() => {
                        location.reload();
                    });
                });

                regenerateTokenRequest.fail((jqXHR, textStatus, errorThrown) => {
                    location.assign("/Login");
                });
                break;
            }
            default: {
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
    $("#ConfirmPasswordError").empty();
    $("#FullNameErrors").empty();
    $("#AddressErrors").empty();
    $("#PasswordErrors").empty();
    $("#EmailErrors").empty();
    $("#IdentityNumberErrors").empty();
    $("#BirthDateErrors").empty();
    $("#PhoneErrors").empty();
    $("#AvatarErrors").empty();
    $("#password").val("");
    $("#confirmPassword").val("");
}