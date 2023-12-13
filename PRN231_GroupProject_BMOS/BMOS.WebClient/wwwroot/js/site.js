// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function Logout() {
    localStorage.removeItem("Customer");
    localStorage.removeItem("Cart");
    localStorage.clear();
    location.assign("/LandingPage");
}

let subMenu = document.getElementById("subMenu");
function toggleMenu() {
    subMenu.classList.toggle("open-menu");
}

window.addEventListener("click", (e) => {
    if (e.target.classList.contains("close") == false) {
        subMenu.classList.remove("open-menu");
    }
});

var cart = localStorage.getItem("Cart");
var cartItems = JSON.parse(cart);
if (cartItems != null && cartItems.length > 0) {
    $(".card--number").show();
    $(".card--number").empty();
    $(".card--number").append(`${cartItems.length}`);
}

function AddToCart(mealId) {
    var quantity = $(`#quantity-${mealId}`).val();
    if (quantity == null) {
        quantity = $(`#numberOfBirdMeal`).val();
    }
    if (Number.parseFloat(quantity) < 0 || Number.isInteger(Number.parseFloat(quantity)) == false) {
        Swal.fire({
            position: 'top-end',
            icon: 'error',
            title: 'Quantity is required an integer number and greater than 0.',
            showConfirmButton: false,
            timer: 2000
        });
    } else {
        var cart = localStorage.getItem("Cart");
        var cartItems = JSON.parse(cart);
        var getActiveProductsRequest = $.ajax({
            type: "GET",
            url: `http://localhost:5093/odata/Meals/Active/Meal/${mealId}?$expand=MealImages,productmeals($expand=product($expand=productImages))`,
            dataType: "json",
            contentType: "application/json",
        });
        if (cartItems == null) {
            cartItems = new Array();
            getActiveProductsRequest.done((data, textStatus, jqXHR) => {
                var meal = {
                    Meal: data,
                    Quantity: Number.parseInt(quantity)
                }

                cartItems.push(meal);
                localStorage.setItem("Cart", JSON.stringify(cartItems));

                Swal.fire({
                    position: 'top-end',
                    icon: 'success',
                    title: 'Add to Cart Successfully.',
                    showConfirmButton: false,
                    timer: 1000
                });
                $(".card--number").show();
                $(".card--number").empty();
                $(".card--number").append(`${cartItems.length}`);
            });
        } else {
            getActiveProductsRequest.done((data, textStatus, jqXHR) => {
                var meal = cartItems.find(item => item.Meal.ID == data.ID);
                if (meal != undefined) {
                    meal.Quantity += Number.parseInt(quantity);
                    localStorage.setItem("Cart", JSON.stringify(cartItems));
                    Swal.fire({
                        position: 'top-end',
                        icon: 'success',
                        title: 'Add to Cart Successfully.',
                        showConfirmButton: false,
                        timer: 1000
                    })
                } else {
                    var item = {
                        Meal: data,
                        Quantity: Number.parseInt(quantity)
                    }
                    cartItems.push(item);
                    localStorage.setItem("Cart", JSON.stringify(cartItems));
                    Swal.fire({
                        position: 'top-end',
                        icon: 'success',
                        title: 'Add to Cart Successfully.',
                        showConfirmButton: false,
                        timer: 1000
                    });
                    $(".card--number").show();
                    $(".card--number").empty();
                    $(".card--number").append(`${cartItems.length}`);
                }
            });
        }
    }
}