
window.onload = () => {
    var customerJson = localStorage.getItem("Customer");
    var customer = JSON.parse(customerJson);
    if (customer != null) {
        $("#unauthentication").hide();
        $("#authentication").show();
        $("#customerName").append(customer.fullName);

        var urlPath = window.location.pathname.split('/');
        $(".loading--part").css("display", "flex");
        var request = $.ajax({
            type: "GET",
            url: `http://localhost:5093/odata/orders/order(${urlPath[2]})/customer(${customer.accountId})`,
            dataType: "json",
            contentType: "application/json",
            headers: { Authorization: `Bearer ` + customer.accessToken }
        });

        request.done((data, textStatus, jqXHR) => {
            $(".loading--part").css("display", "none");
            $("#fullName").append(`${data.customer.fullName}`);
            $("#email").append(`${data.customer.account.email}`);
            $("#phone").append(`${data.customer.phone}`);
            $("#address").append(`${data.customer.address}`);
            $("#orderId").append(`${data.id}`);
            var orderedDate = new Date(data.orderedDate);
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
            if (data.orderStatus == 1) {
                status = "Processing";
                statusColor = "text-info";
            } else if (data.orderStatus == 2) {
                status = "Done";
                statusColor = "text-success";
            } else if (data.orderStatus == 3) {
                status = "Cancled";
                statusColor = "text-danger";
            } else {
                $("#confirmCancel").css("display", "flex");
            }
            $("#orderedDate").append(`${date}/${month}/${orderedDate.getFullYear()}`);
            $("#orderTotal").append(`${formatNumber(data.total + "")}VNĐ`);
            $("#orderStatus").append(`${status}`);
            $("#orderStatus").attr("class", statusColor);

            $(".MealTable").empty();
            var orderDetailsHTML = "";
            var numberOrderDetails = 0;
            data.orderDetails.forEach((orderDetail) => {
                numberOrderDetails++;
                orderDetailsHTML += `
                        <tr>
                            <td scope="row">${numberOrderDetails}</td>
                            <td>${orderDetail.meal.title}</td>
                            <td>${orderDetail.quantity}</td>
                            <td>${formatNumber(orderDetail.unitPrices + "")}VNĐ</td>
                            <td>${formatNumber((orderDetail.unitPrices * orderDetail.quantity) + "")}VNĐ</td>
                        </tr>
                `;
            });
            $(".MealTable").append(orderDetailsHTML);

            $(".orderLogsTable").empty();
            var orderLogsHTML = "";
            var numberOrderLogs = 0;
            data.orderTransactions.forEach((orderLogs) => {
                numberOrderLogs++;
                var paymentTime = new Date(orderLogs.paymentTime);
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
                if (orderLogs.status == 1) {
                    status = "Processing";
                    statusColor = "text-info";
                } else if (orderLogs.status == 2) {
                    status = "Done";
                    statusColor = "text-success";
                } else if (orderLogs.status == 3) {
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
                    if (error.Message[0].DescriptionError[0].toLowerCase().includes("does not exist")) {
                        sessionStorage.setItem("CustomerIdNotFound", error.Message[0].DescriptionError[0]);
                        location.assign("/Login");
                    } else {
                        sessionStorage.setItem("OrderIdNotFound", error.Message[0].DescriptionError[0]);
                        location.assign("/HistoryOrders");
                    }
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
        $("#unauthentication").show();
        $("#authentication").hide();
        location.assign("/Login");
    }
}

function formatNumber(n) {
    // format number 1000000 to 1,234,567
    return n.replace(/\D/g, "").replace(/\B(?=(\d{3})+(?!\d))/g, ".")
}

$("#confirmCancel").click(() => {
    var urlPath = window.location.pathname.split('/');
    Swal.fire({
        title: "Are you sure?",
        text: `[OrderId-${urlPath[2]}] will be canceled.`,
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, cancel it!'
    }).then((result) => {
        if (result.isConfirmed) {
            var customerJson = localStorage.getItem("Customer");
            var customer = JSON.parse(customerJson);
            var urlPath = window.location.pathname.split('/');
            $(".loading--part").css("display", "flex");

            var request = $.ajax({
                type: "DELETE",
                url: `http://localhost:5093/odata/orders/order(${urlPath[2]})/customer(${customer.accountId})/cancel`,
                contentType: "application/json",
                headers: { Authorization: `Bearer ` + customer.accessToken }
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
        }
    });
});