

window.onload = () => {
    var customerJson = localStorage.getItem("Customer");
    var customer = JSON.parse(customerJson);
    if (customer != null) {
        $("#unauthentication").hide();
        $("#authentication").show();
        $("#customerName").append(customer.fullName);
    } else {
        $("#unauthentication").show();
        $("#authentication").hide();
    }
    var cart = localStorage.getItem("Cart");
    var cartItems = JSON.parse(cart);
    if (cartItems != null && cartItems.length > 0) {
        $(".card--number").show();
        $(".cart-empty").hide();
        $(".cart-meals").show();
        $(".bodyRow").empty();
        var cartItemsHTML = "";
        var total = 0;
        cartItems.forEach((cartItem) => {
            cartItemsHTML += `
                       <div class="row regular">
                            <div class="col1">
                                <img height="250" src="${cartItem.Meal.MealImages[0].Source}" width="250">
                                <div class="about">
                                    <div class="title">
                                        <a href="/BirdMeal/${cartItem.Meal.ID}">${cartItem.Meal.Title}</a>
                                    </div>
                                    <a class="remove" data-ajax="true" data-ajax-method="GET" data-ajax-mode="replace" data-ajax-update="#basket" onclick="RemoveItemFromCart(${cartItem.Meal.ID})">Remove</a>
                                </div>
                            </div>
                            <div class="col2">
                                <input id="quantity-${cartItem.Meal.ID}" type="number" value="${cartItem.Quantity}" />
                                <a class="btn" style="display: block; color: #f77400; font-family: Arial; text-transform: uppercase; font-weight: 700; font-size: 0.8em;" onclick="UpdateQuantityItemFromCart(${cartItem.Meal.ID})" >Update</a>
                            </div>
                            <div class="col3">
                                <strong>${formatNumber(cartItem.Meal.Price + "")}</strong><br />(VNĐ)
                            </div>
                            <div class="col4">
                                <strong>${formatNumber(cartItem.Meal.Price * cartItem.Quantity + "")}</strong><br />(VNĐ)
                            </div>
                        </div>
            `;
            total += cartItem.Meal.Price * cartItem.Quantity;
        });
        $(".bodyRow").append(cartItemsHTML);
        $("#total").append(`${formatNumber(total + "")} VNĐ`);

        var RemoveItemOutCartSuccessfully = sessionStorage.getItem("RemoveItemOutCartSuccessfully");
        if (RemoveItemOutCartSuccessfully != null) {
            Swal.fire({
                position: 'top-end',
                icon: 'success',
                title: RemoveItemOutCartSuccessfully,
                showConfirmButton: false,
                timer: 1000
            });

            sessionStorage.removeItem("RemoveItemOutCartSuccessfully");
        }


        var AddToCartSuccessfully = sessionStorage.getItem("AddToCartSuccessfully");
        if (AddToCartSuccessfully != null) {
            Swal.fire({
                position: 'top-end',
                icon: 'success',
                title: AddToCartSuccessfully,
                showConfirmButton: false,
                timer: 1000
            });

            sessionStorage.removeItem("AddToCartSuccessfully");
        }
    } else {
        $(".cart-empty").show();
        $(".cart-meals").hide();
        $(".card--number").hide();
    }
}

function formatNumber(n) {
    // format number 1000000 to 1,234,567
    return n.replace(/\D/g, "").replace(/\B(?=(\d{3})+(?!\d))/g, ".")
}

function EmptyCart() {
    localStorage.removeItem("Cart");
    $(".cart-empty").show();
    $(".cart-meals").hide();
    $(".card--number").hide();
}

function UpdateQuantityItemFromCart(mealId) {
    var cart = localStorage.getItem("Cart");
    var cartItems = JSON.parse(cart);
    var quantity = Number($(`#quantity-${mealId}`).val());
    if (quantity == 0) {
        var meal = cartItems.find(x => x.Meal.ID == mealId);

        const index = cartItems.indexOf(meal);
        cartItems.splice(index, 1);

        localStorage.setItem("Cart", JSON.stringify(cartItems));
        sessionStorage.setItem("AddToCartSuccessfully", "Add to Cart Successfully");
        location.reload();
    } else if (quantity < 0 || Number.isInteger(quantity) == false) {
        Swal.fire({
            position: 'top-end',
            icon: 'error',
            title: 'Quantity is required an integer number and greater than 0.',
            showConfirmButton: false,
            timer: 2000
        });
    } else {
        var meal = cartItems.find(x => x.Meal.ID == mealId);

        meal.Quantity = quantity;
        localStorage.setItem("Cart", JSON.stringify(cartItems));
        sessionStorage.setItem("AddToCartSuccessfully", "Add to Cart Successfully");
        location.reload();
    }
}

function RemoveItemFromCart(mealId) {
    var cart = localStorage.getItem("Cart");
    var cartItems = JSON.parse(cart);

    var meal = cartItems.find(x => x.Meal.ID == mealId);
    Swal.fire({
        title: "Are you sure?",
        text: `[${meal.Meal.Title}] will be removed out the Cart.`,
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            const index = cartItems.indexOf(meal);
            cartItems.splice(index, 1);

            if (cartItems.length == 0) {
                cartItems = null;
            }

            localStorage.setItem("Cart", JSON.stringify(cartItems));
            sessionStorage.setItem("RemoveItemOutCartSuccessfully", "Remove Item out Cart Successfully.")
            location.reload();
        }
    });
}

function Checkout() {
    var customerJson = localStorage.getItem("Customer");
    var customer = JSON.parse(customerJson);
    if (customer != null) {

        var cart = localStorage.getItem("Cart");
        var cartItems = JSON.parse(cart);
        var orderDetais = new Array();
        cartItems.forEach((item) => {
            var orderDetail = {
                Id: item.Meal.ID,
                Amount: Number(item.Quantity)
            }
            orderDetais.push(orderDetail);
        });

        var order = {
            Email: customer.email,
            Meals: orderDetais
        };
        $(".loading--part").css("display", "flex");
        var request = $.ajax({
            type: "POST",
            url: `http://localhost:5093/odata/orders`,
            contentType: "application/json",
            data: JSON.stringify(order),
            headers: { Authorization: `Bearer ` + customer.accessToken }
        });

        request.done(() => {
            $(".loading--part").css("display", "none");
            localStorage.removeItem("Cart");
            sessionStorage.setItem("CheckoutSuccess", "Checkout Successfully.");
            location.assign(`/HistoryOrders`);
        });

        request.fail((jqXHR, textStatus, errorThrown) => {
            $(".loading--part").css("display", "none");
            switch (jqXHR.status) {
                case 400: {
                    var error = JSON.parse(jqXHR.responseText);
                    if (error.Message.length >= 1 && error.Message[0].FieldNameError != "Exception") {
                        error.Message.forEach((errorDetail) => {
                            switch (errorDetail.FieldNameError.toLowerCase()) {
                                case "amount": {
                                    var amountErrorHTML = "";
                                    errorDetail.DescriptionError.forEach((descriptionErrorDetail) => {
                                        amountErrorHTML += `<br />${descriptionErrorDetail}`;
                                    });
                                    Swal.fire({
                                        icon: 'error',
                                        title: 'Oops...',
                                        text: amountErrorHTML
                                    });
                                }
                            }
                        });
                    }

                    if (error.Message.length == 1 && error.Message[0].FieldNameError == "Exception") {
                        if (error.Message[0].FieldNameError.toLowerCase() == "exception") {
                            var includeDots = error.Message[0].DescriptionError[0].includes(".");
                            if (includeDots) {
                                var dotErrors = error.Message[0].DescriptionError[0].trim().split(".");
                                if (dotErrors.length <= 2) {
                                    Swal.fire({
                                        icon: 'error',
                                        title: 'Oops...',
                                        text: error.Message[0].DescriptionError[0]
                                    });
                                } else {
                                    var dotErrorsHTML = "";
                                    dotErrors.forEach((error) => {
                                        if (error.trim().length > 0) {
                                            dotErrorsHTML += `<li>${error}</li>`
                                        }
                                    });
                                    dotErrorsHTML = `<ul>${dotErrorsHTML}</ul>`
                                    Swal.fire({
                                        icon: 'error',
                                        title: 'Oops...',
                                        html: dotErrorsHTML
                                    });
                                }
                            }
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
        location.assign("/Login");
    }
}