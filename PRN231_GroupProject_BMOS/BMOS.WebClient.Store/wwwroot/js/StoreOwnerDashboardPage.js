
var profitsChart = null;

window.onload = () => {
    let storeOwnerJson = localStorage.getItem("StoreOwner");
    let storeOwner = JSON.parse(storeOwnerJson);
    if (storeOwner != null) {
        $("#storeOwnerName").append(`${storeOwner.fullName}`);

        $(".loading--part").css("display", "flex");
        var request = $.ajax({
            type: "GET",
            url: `http://localhost:5093/odata/StoreOwnerDashBoards`,
            dataType: "json",
            contentType: "application/json",
            headers: { Authorization: `Bearer ` + storeOwner.accessToken }
        });

        request.done((data, textStatus, jqXHR) => {
            $(".loading--part").css("display", "none");
            var today = new Date();
            $("#AmountCustomers").append(data.TotalCustomers);
            $("#AmountStaffs").append(data.TotalStaffs);
            $("#AmountMeals").append(data.TotalMeals);
            $("#AmountProducts").append(data.TotalProducts);
            var thisMonthProfits = data.MonthProfits.find(x => x.Month == (today.getMonth() + 1));
            $("#AmountProfits").append(`${formatNumber(thisMonthProfits.Profits + "")}VNĐ`);
            $("#ProfitsYears").empty();
            var profitYearHTML = "";
            data.ProfitYears.forEach((year) => {
                var selectedYear = year == today.getFullYear() ? "selected" : "";
                profitYearHTML += `<option value="${year}" ${selectedYear}>${year}</option>`;
            });
            $("#ProfitsYears").append(profitYearHTML);
            const ctx = document.getElementById('lineChart');
            var monthProfits = data.MonthProfits.sort(function (a, b) { return a.Month - b.Month });
            var monthProfitsStr = monthProfits.map(({ Month, Profits }) => `${[Profits].join(',')}`);
            if (profitsChart != null) {
                profitsChart.destroy();
            }
            profitsChart = new Chart(ctx, {
                type: 'line',
                data: {
                    labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
                    datasets: [{
                        label: 'Profits Of BMOS Store',
                        data: monthProfitsStr,
                        borderWidth: 1
                    }]
                },
                options: {
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    },
                    responsive: true,
                    maintainAspectRatio: false
                }
            });
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
                default: {
                    console.error(jqXHR.responseText);
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

        var LoginSuccessfully = sessionStorage.getItem("LoginSuccessfully");
        if (LoginSuccessfully != null) {
            Swal.fire({
                position: 'top-end',
                icon: 'success',
                title: LoginSuccessfully,
                showConfirmButton: false,
                timer: 1000
            });
            sessionStorage.removeItem("LoginSuccessfully");
        }

    } else {
        location.assign("/Login");
    }
}

function formatNumber(n) {
    // format number 1000000 to 1,234,567
    return n.replace(/\D/g, "").replace(/\B(?=(\d{3})+(?!\d))/g, ".")
}

$("#ProfitsYears").change((event) => {
    let storeOwnerJson = localStorage.getItem("StoreOwner");
    let storeOwner = JSON.parse(storeOwnerJson);
    $(".loading--part").css("display", "flex");
    var request = $.ajax({
        type: "GET",
        url: `http://localhost:5093/odata/StoreOwnerDashBoards?year=${event.target.value}`,
        dataType: "json",
        contentType: "application/json",
        headers: { Authorization: `Bearer ` + storeOwner.accessToken }
    });

    request.done((data, textStatus, jqXHR) => {
        $(".loading--part").css("display", "none");
        var today = new Date();
        $("#AmountCustomers").append(data.TotalCustomers);
        $("#AmountStaffs").append(data.TotalStaffs);
        $("#AmountMeals").append(data.TotalMeals);
        $("#AmountProducts").append(data.TotalProducts);
        var thisMonthProfits = data.MonthProfits.find(x => x.Month == (today.getMonth() + 1));
        $("#AmountProfits").append(`${formatNumber(thisMonthProfits.Profits + "")}VNĐ`);
        $("#ProfitsYears").empty();
        var profitYearHTML = "";
        data.ProfitYears.forEach((year) => {
            var selectedYear = year == today.getFullYear() ? "selected" : "";
            profitYearHTML += `<option value="${year}">${year}</option>`;
        });
        $("#ProfitsYears").append(profitYearHTML);
        var monthProfitsStr = "";
        var monthProfits = data.MonthProfits.sort(function (a, b) { return a.Month - b.Month });
        monthProfitsStr = monthProfits.map(({ Month, Profits }) => `${[Profits].join(',')}`);
        const ctx = document.getElementById('lineChart');
        if (profitsChart != null) {
            profitsChart.destroy();
        }
        profitsChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
                datasets: [{
                    label: 'Profits Of BMOS Store',
                    data: monthProfitsStr,
                    borderWidth: 1
                }]
            },
            options: {
                scales: {
                    y: {
                        beginAtZero: true
                    }
                },
                maintainAspectRatio: false
            }
        });
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
            default: {
                console.error(jqXHR.responseText);
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
});