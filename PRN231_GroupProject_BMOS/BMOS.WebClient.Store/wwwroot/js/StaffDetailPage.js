

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
            $("#avatar").attr("src", data.Avatar);
            $("#email").append(data.Account.Email);
            $("#fullname").append(data.FullName);
            $("#identityNumber").append(data.IdentityNumber);
            $("#phone").append(data.Phone);
            $("#address").append(data.Address);
            var gender = data.Gender;
            if (gender == true) {
                $("#gender").append("Male");
            } else {
                $("#gender").append("Female");
            }
            var birthday = new Date(data.BirthDate);
            $("#birthday").append(`${birthday.getDate()}/${birthday.getMonth() + 1}/${birthday.getFullYear()}`);
            var registerDate = new Date(data.RegisteredDate);
            $("#registerDate").append(`${registerDate.getDate()}/${registerDate.getMonth() + 1}/${registerDate.getFullYear()}`);
            if (data.QuitDate != null) {
                var quitDate = new Date(data.QuitDate);
                $("#quitDate").append(`${quitDate.getDate()}/${quitDate.getMonth() + 1}/${quitDate.getFullYear()}`);
                $("#status").append("Inactive").attr("class", "text-danger");
            } else {
                $("#quitDate").append("NONE").attr("class", "text-danger");
                $("#status").append("Active").attr("class", "text-success");
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

        var CreateStaffSuccessfully = sessionStorage.getItem("CreateStaffSuccessfully");
        if (CreateStaffSuccessfully != null) {
            Swal.fire({
                position: 'top-end',
                icon: 'success',
                title: CreateStaffSuccessfully,
                showConfirmButton: false,
                timer: 1000
            });
            sessionStorage.removeItem("CreateStaffSuccessfully");
        }

        var UpdateStaffSuccessfully = sessionStorage.getItem("UpdateStaffSuccessfully");
        if (UpdateStaffSuccessfully != null) {
            Swal.fire({
                position: 'top-end',
                icon: 'success',
                title: UpdateStaffSuccessfully,
                showConfirmButton: false,
                timer: 1000
            });
            sessionStorage.removeItem("UpdateStaffSuccessfully");
        }
    } else {
        location.assign("/Login");
    }
}