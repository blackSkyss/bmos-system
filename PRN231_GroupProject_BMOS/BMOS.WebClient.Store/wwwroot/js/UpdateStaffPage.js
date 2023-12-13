

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
        $(".loading--part").css("display", "flex");
        var urlPath = window.location.pathname.split('/');
        var request = $.ajax({
            type: "GET",
            url: `http://localhost:5093/odata/staffs/${urlPath[3]}?$expand=account`,
            dataType: "json",
            contentType: "application/json",
            headers: { Authorization: `Bearer ${storeOwner.accessToken}` }
        });

        request.done((data, textStatus, jqXHR) => {
            $(".loading--part").css("display", "none");
            $("#displayImage").attr("src", data.Avatar);
            $("#email").val(data.Account.Email);
            $("#fullName").val(data.FullName);
            $("#password").val("********");
            $("#identityNumber").val(data.IdentityNumber);
            $("#phone").val(data.Phone);
            $("#address").val(data.Address);
            var gender = data.Gender;
            var genderEmlements = document.getElementsByName("gender");
            if (gender == true) {
                genderEmlements[0].checked = true;
            } else {
                genderEmlements[1].checked = true;
            }
            var birthday = new Date(data.BirthDate);
            var month = "";
            var date = "";
            if (birthday.getMonth() + 1 < 10) {
                month = `0${birthday.getMonth() + 1}`;
            } else {
                date = `${birthday.getDate() + 1}`;
            }
            if (birthday.getDate() < 10) {
                date = `0${birthday.getDate()}`;
            } else {
                date = `${birthday.getDate()}`;
            }
            $("#birthday").val(`${birthday.getFullYear()}-${month}-${date}`);

            if (data.Account.Status == true) {
                $("#status").val(1);
            } else {
                $("#status").val(0);
            }
        });

        request.fail((jqXHR, textStatus, errorThrown) => {
            $(".loading--part").css("display", "none");
            switch (jqXHR.status) {
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
                case 404: {
                    var error = JSON.parse(jqXHR.responseText);
                    sessionStorage.setItem("StaffIdNotFound", error.Message[0].DescriptionError[0]);
                    location.assign("/StoreOwner/Staffs");
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
    } else {
        location.assign("/Login");
    }
}

function UpdateStaffForm() {
    let storeOwnerJson = localStorage.getItem("StoreOwner");
    let storeOwner = JSON.parse(storeOwnerJson);

    var urlPath = window.location.pathname.split('/');

    var fullName = $("#fullName").val();

    var fileImage = $("#imageInput")[0].files[0];
    var password = $("#password").val();
    var address = $("#address").val();
    var phone = $("#phone").val();
    var birthday = $("#birthday").val();
    var status = $("#status option:selected").val();
    var genderElements = document.getElementsByName("gender");
    var gender = 0;
    for (var i = 0; i < genderElements.length; i++) {
        if (genderElements[i].checked) {
            gender = genderElements[i].value;
        }
    }

    var formData = new FormData();

    formData.append("FullName", fullName);
    formData.append("Address", address);
    formData.append("Phone", phone);
    formData.append("Avatar", fileImage);
    formData.append("Gender", gender == 1 ? true : false);
    formData.append("BirthDate", birthday);
    formData.append("PasswordHash", password);
    formData.append("Status", status == 1 ? true : false);

    if ($("#birthday").val() == "") {
        ClearErrorForm();
        $("#BirthDateErrors").append("<br /> Birthday is null.");
        $("#BirthDateErrors").append("<br /> Birthday is empty.");
    }

    $(".loading--part").css("display", "flex");
    var request = $.ajax({
        type: "PUT",
        url: `http://localhost:5093/odata/staffs/${urlPath[3]}`,
        processData: false,
        contentType: false,
        data: formData,
        headers: { Authorization: `Bearer ${storeOwner.accessToken}` }
    });

    request.done((data, textStatus, jqXHR) => {
        $(".loading--part").css("display", "none");
        sessionStorage.setItem("UpdateStaffSuccessfully", "Update Staff Successfully.");
        location.assign(`/StoreOwner/Staff/${urlPath[3]}`);
    });

    request.fail((jqXHR, textStatus, errorThrown) => {
        $(".loading--part").css("display", "none");
        switch (jqXHR.status) {
            case 400: {
                var error = JSON.parse(jqXHR.responseText);
                if (error.Message.length >= 1 && error.Message[0].FieldNameError != "Exception") {
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
                    if (error.Message[0].FieldNameError.toLowerCase() == "birthdate") {
                        ClearErrorForm();
                        var birthDateErrorHTML = `<br />${error.Message[0].DescriptionError[0]}`;
                        $("#BirthDateErrors").append(birthDateErrorHTML);
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
            case 404: {
                var error = JSON.parse(jqXHR.responseText);
                sessionStorage.setItem("StaffIdNotFound", error.Message[0].DescriptionError[0]);
                location.assign("/StoreOwner/Staffs");
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