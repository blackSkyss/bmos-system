

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
            $("#Email").append(`${data.Account.Email}`);
            $("#FullName").append(`${data.FullName}`);
            $("#Address").append(`${data.Address}`);
            $("#phone").append(`${data.Phone}`);
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
            $(`input[name=gender][value=${data.Gender == true ? 1 : 0}]`).prop("checked", true);
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
        $("#unauthentication").show();
        $("#authentication").hide();
        location.assign("/Login");
    }
}