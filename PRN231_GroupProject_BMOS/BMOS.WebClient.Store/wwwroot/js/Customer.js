window.onload = () => {
    let storeOwnerJson = localStorage.getItem("StoreOwner");
    let storeOwner = JSON.parse(storeOwnerJson);
    if (storeOwner != null) {
        $("#storeOwnerName").append(`${storeOwner.fullName}`);
        var urlPath = window.location.pathname.split('/');
        $(".loading--part").css("display", "flex");
        var request = $.ajax({
            type: "GET",
            url: `http://localhost:5093/odata/Customers/${urlPath[3]}?$expand=account`,
            dataType: "json",
            contentType: "application/json",
            headers: { Authorization: `Bearer ${storeOwner.accessToken}` }
        });

        request.done((data, textStatus, jqXHR) => {
            $(".loading--part").css("display", "none");
            $("#Avatar").attr("src", `${data.Avatar}`);
            $("#Email").append(`${data.Account.Email}`);
            $("#FullName").append(`${data.FullName}`);
            $("#Address").append(`${data.Address}`);
            $("#Phone").append(`${data.Phone}`);
            var Birthday = new Date(data.BirthDate);
            var date = `${Birthday.getDate()}`;
            var month = `${Birthday.getMonth() + 1}`;
            if (date < 10) {
                date = "0" + date;
            }
            if (month < 10) {
                month = "0" + month;
            }
            $("#Birthday").append(`${date}/${month}/${Birthday.getFullYear()}`);
            var gender = "Female";
            if (data.Gender == true) {
                gender = "Male";
            }
            $("#Gender").append(gender);
            var status = "Inactive";
            var statusColor = "text-danger";
            if (data.Account.Status == true) {
                status = "Active";
                statusColor = "text-success";
            }
            $("#Status").append(status);
            $("#Status").attr("class", statusColor);
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
                    sessionStorage.setItem("CustomerIdNotFound", error.Message[0].DescriptionError[0]);
                    location.assign("/StoreOwner/Customers");
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

        var AccountIdNotFound = sessionStorage.getItem("AccountIdNotFound");
        if (AccountIdNotFound != null) {
            Swal.fire({
                icon: 'error',
                title: 'Oops...',
                text: AccountIdNotFound
            });
            sessionStorage.removeItem("AccountIdNotFound");
        }

        var UpdateProfileSuccessfully = sessionStorage.getItem("UpdateProfileSuccessfully");
        if (UpdateProfileSuccessfully != null) {
            Swal.fire({
                position: 'top-end',
                icon: 'success',
                title: UpdateProfileSuccessfully,
                showConfirmButton: false,
                timer: 1000
            });
            sessionStorage.removeItem("UpdateProfileSuccessfully");
        }
    } else {
        location.assign("/Login");
    }
}