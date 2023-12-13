window.onload = () => {
    let staffJson = localStorage.getItem("Staff");
    let staff = JSON.parse(staffJson);
    if (staff != null) {
        $("#staffName").append(`${staff.fullName}`);

        var urlPath = window.location.pathname.split('/');
        $(".loading--part").css("display", "flex");
        var request = $.ajax({
            type: "GET",
            url: `http://localhost:5093/odata/orders(${urlPath[3]})?$expand=customer($expand=account), OrderDetails, OrderTransactions`,
            dataType: "json",
            contentType: "application/json",
            headers: { Authorization: `Bearer ` + staff.accessToken }
        });

        request.done((data, textStatus, jqXHR) => {
            $(".loading--part").css("display", "none");
            $("#fullName").append(`${data.Customer.FullName}`);
            $("#email").append(`${data.Customer.Account.Email}`);
            $("#phone").append(`${data.Customer.Phone}`);
            $("#address").append(`${data.Customer.Address}`);
            $("#orderId").append(`${data.ID}`);
            var orderedDate = new Date(data.OrderedDate);
            var month = orderedDate.getMonth() + 1;
            var date = orderedDate.getDate();
            if (month < 10) {
                month = "0" + month;
            }
            if (date < 10) {
                date = "0" + date;
            }
            var status = "New Order";
            var statusColor = "text-primary";
            if (data.OrderStatus == 1) {
                status = "Processing";
                statusColor = "text-info";
                $("#confirmButton").css("display", "flex");
                $("#confirmButton").append("Confirm to Done");
            } else if (data.OrderStatus == 2) {
                status = "Done";
                statusColor = "text-success";
            } else if (data.OrderStatus == 3) {
                status = "Cancled";
                statusColor = "text-danger";
            } else {
                $("#confirmButton").css("display", "flex");
                $("#confirmCancel").css("display", "flex");
                $("#confirmCancel").attr("data-customerId", `${data.Customer.AccountID}`);
                $("#confirmButton").append("Confirm to Processing");
            }
            $("#orderedDate").append(`${date}/${month}/${orderedDate.getFullYear()}`);
            $("#orderTotal").append(`${formatNumber(data.Total + "")}VNĐ`);
            $("#orderStatus").append(`${status}`);
            $("#orderStatus").attr("class", statusColor);
            

            $(".MealTable").empty();
            var orderDetailsHTML = "";
            var numberOrderDetails = 0;
            data.OrderDetails.forEach((orderDetail) => {
                numberOrderDetails++;
                orderDetailsHTML += `
                        <tr>
                            <td scope="row">${numberOrderDetails}</td>
                            <td>${orderDetail.Meal.Title}</td>
                            <td>${orderDetail.Quantity}</td>
                            <td>${formatNumber(orderDetail.UnitPrices + "")}VNĐ</td>
                            <td>${formatNumber((orderDetail.UnitPrices * orderDetail.Quantity) + "")}VNĐ</td>
                            <td><a href="/Staff/Meal/${orderDetail.Meal.ID}" style="color: #000; font-size: 18px;"><i class="fa-solid fa-circle-info" id="historyOrder"></i></a></td>
                        </tr>
                `;
            });
            $(".MealTable").append(orderDetailsHTML);

            $(".orderLogsTable").empty();
            var orderLogsHTML = "";
            var numberOrderLogs = 0;
            data.OrderTransactions.forEach((orderLogs) => {
                numberOrderLogs++;
                var paymentTime = new Date(orderLogs.PaymentTime);
                var month = paymentTime.getMonth() + 1;
                var date = paymentTime.getDate();
                if (month < 10) {
                    month = "0" + month;
                }
                if (date < 10) {
                    date = "0" + date;
                }
                var status = "New Order";
                var statusColor = "text-primary";
                if (orderLogs.Status == 1) {
                    status = "Processing";
                    statusColor = "text-info";
                } else if (orderLogs.Status == 2) {
                    status = "Done";
                    statusColor = "text-success";
                } else if (orderLogs.Status == 3) {
                    status = "Cancled";
                    statusColor = "text-danger";
                }
                orderLogsHTML += `
                        <tr>
                                <td scope="row">${numberOrderLogs}</td>
                                <td>${date}/${month}/${paymentTime.getFullYear()}</td>
                                <td class="${statusColor}">${status}</td>
                        </tr>
                `;
            });
            $(".orderLogsTable").append(orderLogsHTML);
        });


        request.fail((jqXHR, textStatus, errorThrown) => {
            $(".loading--part").css("display", "none");
            switch (jqXHR.status) {
                case 401: {
                    var tokens = {
                        AccessToken: staff.accessToken,
                        RefreshToken: staff.refreshToken
                    };
                    var regenerateTokenRequest = $.ajax({
                        type: "POST",
                        url: "http://localhost:5093/odata/authentications/recreate-token",
                        dataType: "json",
                        contentType: "application/json",
                        data: JSON.stringify(tokens),
                    });

                    regenerateTokenRequest.done((data, textStatus, jqXHR) => {
                        staff.accessToken = data.accessToken;
                        staff.refreshToken = data.refreshToken;
                        localStorage.setItem("Staff", JSON.stringify(staff));
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
                    sessionStorage.setItem("OrderIdNotFound", error.Message[0].DescriptionError[0]);
                    location.assign("/Staff/Orders");
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

        var OrderWasCanceledSuccessfully = sessionStorage.getItem("OrderWasCanceledSuccessfully");
        if (OrderWasCanceledSuccessfully != null) {
            Swal.fire({
                position: 'top-end',
                icon: 'success',
                title: OrderWasCanceledSuccessfully,
                showConfirmButton: false,
                timer: 1000
            });
            sessionStorage.removeItem("OrderWasCanceledSuccessfully");
        }
    } else {
        location.assign("/Login");
    }
}

function formatNumber(n) {
    // format number 1000000 to 1,234,567
    return n.replace(/\D/g, "").replace(/\B(?=(\d{3})+(?!\d))/g, ".")
}

$("#confirmCancel").click(() => {
    var urlPath = window.location.pathname.split('/');
    var customerId = $("#confirmCancel").attr("data-customerId");
    Swal.fire({
        title: "Are you sure?",
        text: `[OrderId-${urlPath[3]}] of [CustomerId-${customerId}] will be canceled.`,
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, cancel it!'
    }).then((result) => {
        if (result.isConfirmed) {
            let staffJson = localStorage.getItem("Staff");
            let staff = JSON.parse(staffJson);
            $(".loading--part").css("display", "flex");

            var request = $.ajax({
                type: "DELETE",
                url: `http://localhost:5093/odata/orders/order(${urlPath[3]})/customer(${customerId})/cancel`,
                contentType: "application/json",
                headers: { Authorization: `Bearer ` + staff.accessToken }
            });

            request.done((data, textStatus, jqXHR) => {
                $(".loading--part").css("display", "none");
                sessionStorage.setItem("OrderWasCanceledSuccessfully", "Order was canceled successfully.");
                location.reload();
            });

            request.fail((jqXHR, textStatus, errorThrown) => {
                $(".loading--part").css("display", "none");
                switch (jqXHR.status) {
                    case 400: {
                        var error = JSON.parse(jqXHR.responseText);
                        Swal.fire({
                            icon: 'error',
                            title: 'Oops...',
                            text: error.Message[0].DescriptionError[0]
                        });
                        break;
                    }
                    case 401: {
                        var tokens = {
                            AccessToken: staff.accessToken,
                            RefreshToken: staff.refreshToken
                        };
                        var regenerateTokenRequest = $.ajax({
                            type: "POST",
                            url: "http://localhost:5093/odata/authentications/recreate-token",
                            dataType: "json",
                            contentType: "application/json",
                            data: JSON.stringify(tokens),
                        });

                        regenerateTokenRequest.done((data, textStatus, jqXHR) => {
                            staff.accessToken = data.accessToken;
                            staff.refreshToken = data.refreshToken;
                            localStorage.setItem("Staff", JSON.stringify(staff));
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
        }
    });
});

$("#confirmButton").click((event) => {
    var action = $(event.target).text();
    var urlPath = window.location.pathname.split('/');
    switch (action.toLowerCase()) {
        case "confirm to processing": {
            Swal.fire({
                title: "Are you sure?",
                text: `[OrderId-${urlPath[3]}] will be changed action status to be a processing.`,
                icon: "warning",
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, change it!'
            }).then((result) => {
                if (result.isConfirmed) {
                    let staffJson = localStorage.getItem("Staff");
                    let staff = JSON.parse(staffJson);
                    $(".loading--part").css("display", "flex");

                    var request = $.ajax({
                        type: "PUT",
                        url: `http://localhost:5093/odata/orders/update-processing/${urlPath[3]}`,
                        contentType: "application/json",
                        headers: { Authorization: `Bearer ` + staff.accessToken }
                    });

                    request.done((data, textStatus, jqXHR) => {
                        $(".loading--part").css("display", "none");
                        sessionStorage.setItem("OrderWasCanceledSuccessfully", "Order was changed a processing action status successfully.");
                        location.reload();
                    });

                    request.fail((jqXHR, textStatus, errorThrown) => {
                        $(".loading--part").css("display", "none");
                        switch (jqXHR.status) {
                            case 400: {
                                var error = JSON.parse(jqXHR.responseText);
                                Swal.fire({
                                    icon: 'error',
                                    title: 'Oops...',
                                    text: error.Message[0].DescriptionError[0]
                                });
                                break;
                            }
                            case 401: {
                                var tokens = {
                                    AccessToken: staff.accessToken,
                                    RefreshToken: staff.refreshToken
                                };
                                var regenerateTokenRequest = $.ajax({
                                    type: "POST",
                                    url: "http://localhost:5093/odata/authentications/recreate-token",
                                    dataType: "json",
                                    contentType: "application/json",
                                    data: JSON.stringify(tokens),
                                });

                                regenerateTokenRequest.done((data, textStatus, jqXHR) => {
                                    staff.accessToken = data.accessToken;
                                    staff.refreshToken = data.refreshToken;
                                    localStorage.setItem("Staff", JSON.stringify(staff));
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
                }
            });
            break;
        } case "confirm to done": {
            Swal.fire({
                title: "Are you sure?",
                text: `[OrderId-${urlPath[3]}] will be changed action status to be a done.`,
                icon: "warning",
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, change it!'
            }).then((result) => {
                if (result.isConfirmed) {
                    let staffJson = localStorage.getItem("Staff");
                    let staff = JSON.parse(staffJson);
                    $(".loading--part").css("display", "flex");

                    var request = $.ajax({
                        type: "PUT",
                        url: `http://localhost:5093/odata/orders/update-done/${urlPath[3]}`,
                        contentType: "application/json",
                        headers: { Authorization: `Bearer ` + staff.accessToken }
                    });

                    request.done((data, textStatus, jqXHR) => {
                        $(".loading--part").css("display", "none");
                        sessionStorage.setItem("OrderWasCanceledSuccessfully", "Order was changed a done action status successfully.");
                        location.reload();
                    });

                    request.fail((jqXHR, textStatus, errorThrown) => {
                        $(".loading--part").css("display", "none");
                        switch (jqXHR.status) {
                            case 400: {
                                var error = JSON.parse(jqXHR.responseText);
                                Swal.fire({
                                    icon: 'error',
                                    title: 'Oops...',
                                    text: error.Message[0].DescriptionError[0]
                                });
                                break;
                            }
                            case 401: {
                                var tokens = {
                                    AccessToken: staff.accessToken,
                                    RefreshToken: staff.refreshToken
                                };
                                var regenerateTokenRequest = $.ajax({
                                    type: "POST",
                                    url: "http://localhost:5093/odata/authentications/recreate-token",
                                    dataType: "json",
                                    contentType: "application/json",
                                    data: JSON.stringify(tokens),
                                });

                                regenerateTokenRequest.done((data, textStatus, jqXHR) => {
                                    staff.accessToken = data.accessToken;
                                    staff.refreshToken = data.refreshToken;
                                    localStorage.setItem("Staff", JSON.stringify(staff));
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
                }
            });
            break;
        }
    }
});