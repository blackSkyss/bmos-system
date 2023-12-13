const imageInput = document.querySelector("#imageInput");
var uploadedImage = "";

imageInput.addEventListener("change", function () {
    const reader = new FileReader();
    reader.addEventListener("load", () => {
        uploadedImage = reader.result;
        document.querySelector("#displayImage").src = uploadedImage;
    });
    reader.readAsDataURL(this.files[0]);
})

window.onload = () => {
    var customerJson = localStorage.getItem("Customer");
    var customer = JSON.parse(customerJson);
    if (customer != null) {
        $("#unauthentication").hide();
        $("#authentication").show();
        $("#customerName").append(customer.fullName);

        $(".loading--part").css("display", "flex");
        var request = $.ajax({
            type: "GET",
            url: `http://localhost:5093/odata/Customers/${customer.accountId}?$expand=account`,
            dataType: "json",
            contentType: "application/json",
            headers: { Authorization: `Bearer ${customer.accessToken}` }
        });

        request.done((data, textStatus, jqXHR) => {
            $(".loading--part").css("display", "none");
            $("#displayImage").attr("src", `${data.Avatar}`);
            $("#Email").val(`${data.Account.Email}`);
            $("#FullName").val(`${data.FullName}`);
            $("#Password").val(`********`);
            $("#Address").val(`${data.Address}`);
            $("#phone").val(`${data.Phone}`);
            var gender = data.Gender;
            var genderEmlements = document.getElementsByName("gender");
            if (gender == true) {
                genderEmlements[0].checked = true;
            } else {
                genderEmlements[1].checked = true;
            }
            var Birthday = new Date(data.BirthDate);
            var date = `${Birthday.getDate()}`;
            var month = `${Birthday.getMonth() + 1}`;
            if (date < 10) {
                date = "0" + date;
            }
            if (month < 10) {
                month = "0" + month;
            }
            $("#Birthday").val(`${Birthday.getFullYear()}-${month}-${date}`);
        });

        request.fail((jqXHR, textStatus, errorThrown) => {
            $(".loading--part").css("display", "none");
            switch (jqXHR.status) {
                case 401: {
                    var tokens = {
                        AccessToken: customer.accessToken,
                        RefreshToken: customer.refreshToken
                    };
                    var regenerateTokenRequest = $.ajax({
                        type: "POST",
                        url: "http://localhost:5093/odata/authentications/recreate-token",
                        dataType: "json",
                        contentType: "application/json",
                        data: JSON.stringify(tokens),
                    });

                    regenerateTokenRequest.done((data, textStatus, jqXHR) => {
                        customer.accessToken = data.accessToken;
                        customer.refreshToken = data.refreshToken;
                        localStorage.setItem("Customer", JSON.stringify(customer));
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
                    sessionStorage.setItem("CustomerIdNotFound", error.Message[0].DescriptionError[0]);
                    location.assign("/Login");
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
        $("#unauthentication").show();
        $("#authentication").hide();
        location.assign("/Login");
    }
}

function UpdateProfileForm() {

    var customerJson = localStorage.getItem("Customer");
    var customer = JSON.parse(customerJson);

    var avatar = $("#imageInput")[0].files[0];
    var fullName = $("#FullName").val();
    var Address = $("#Address").val();
    var Password = $("#Password").val();
    var phone = $("#phone").val();
    var Birthday = $("#Birthday").val();
    var genderElements = document.getElementsByName("gender");
    var gender = 0;
    for (var i = 0; i < genderElements.length; i++) {
        if (genderElements[i].checked) {
            gender = genderElements[i].value;
        }
    }

    var formData = new FormData();
    formData.append("FullName", fullName);
    formData.append("Address", Address);
    formData.append("Phone", phone);
    formData.append("PasswordHash", Password.includes("*") == true ? "" : Password);
    formData.append("Avatar", avatar);
    formData.append("Gender", gender == 1 ? true : false);
    formData.append("BirthDate", Birthday);

    $(".loading--part").css("display", "flex");
    var request = $.ajax({
        type: "PUT",
        url: `http://localhost:5093/odata/customers/${customer.accountId}`,
        processData: false,
        contentType: false,
        data: formData,
        headers: { Authorization: `Bearer ${customer.accessToken}` }
    });

    request.done((data, textStatus, jqXHR) => {
        $(".loading--part").css("display", "none");
        sessionStorage.setItem("UpdateProfileSuccessfully", "Update Profile Successfully.");
        location.assign(`/Profile`);
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
                    AccessToken: customer.accessToken,
                    RefreshToken: customer.refreshToken
                };
                var regenerateTokenRequest = $.ajax({
                    type: "POST",
                    url: "http://localhost:5093/odata/authentications/recreate-token",
                    dataType: "json",
                    contentType: "application/json",
                    data: JSON.stringify(tokens),
                });

                regenerateTokenRequest.done((data, textStatus, jqXHR) => {
                    customer.accessToken = data.accessToken;
                    customer.refreshToken = data.refreshToken;
                    localStorage.setItem("Customer", JSON.stringify(Customer));
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
                sessionStorage.setItem("AccountIdNotFound", error.Message[0].DescriptionError[0]);
                location.assign("/Profile");
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