

window.onload = () => {
    var customerJson = localStorage.getItem("Customer");
    var customer = JSON.parse(customerJson);
    if (customer != null) {
        $("#unauthentication").hide();
        $("#authentication").show();
        $("#customerName").append(customer.fullName);
        const params = new Proxy(new URLSearchParams(window.location.search), {
            get: (searchParams, prop) => searchParams.get(prop),
        });
        let value = params.resultCode;
        var amount = params.amount;
        var orderId = params.orderId;
        if (value != null) {
            if (value === "9000") {
                var request = $.ajax({
                    type: "PUT",
                    url: `http://localhost:5093/odata/WalletTransactions/${orderId}`,
                    contentType: "application/json",
                    headers: { Authorization: `Bearer ${customer.accessToken}` }
                });
                request.done((data, textStatus, jqXHR) => {
                    $(".deposit-Box-success").show();
                    $(".deposit-Box-failed").hide();
                    $("#amountMoney").append(`${formatNumber(amount)}VNĐ`);
                });
            } else {
                $(".deposit-Box-success").hide();
                $(".deposit-Box-failed").show();
            }
        } else {
            location.assign("/Wallet");
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