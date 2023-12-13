

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
    $(".loading--part").css("display", "none");
    var getActiveProductsRequest = $.ajax({
        type: "GET",
        url: "http://localhost:5093/odata/Products/Active/Product?$select=ID,Name,ProductImages&$expand=productimages&$top=4",
        dataType: "json",
        contentType: "application/json"
    });

    getActiveProductsRequest.done((data, textStatus, jqXHR) => {
        $(".loading--part").css("display", "none");
        GetProducts(data);
    });

    var getActiveMealsRequest = $.ajax({
        type: "GET",
        url: "http://localhost:5093/odata/Meals/Active/Meal?$select=ID,Title,Price,MealImages&$expand=mealimages&$top=4",
        dataType: "json",
        contentType: "application/json"
    });

    getActiveMealsRequest.done((data, textStatus, jqXHR) => {
        $(".loading--part").css("display", "none");
        GetMeals(data);
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
}

function GetProducts(products) {
    $(".birdFood--container").empty();
    var birdFoodHTML = "";
    products.forEach((product) => {
        birdFoodHTML += `
                <div class="col-md-3 birdFoodItem">
                    <a href="/BirdFood/${product.ID}">
                        <img height="230" width="230" src="${product.ProductImages[0].Source}" />
                        <span class="title">${product.Name}</span>
                        <span class="btn btn-birdFoodItem">View</span>
                    </a>
                </div>
                        `;
    });
    $(".birdFood--container").append(birdFoodHTML);
}

function formatNumber(n) {
    // format number 1000000 to 1,234,567
    return n.replace(/\D/g, "").replace(/\B(?=(\d{3})+(?!\d))/g, ".")
}

function GetMeals(meals) {
    $(".birdMeal--container").empty();
    var birdMealsHTML = "";
    meals.forEach((meal) => {
        birdMealsHTML += `
                <div class="col-md-3 birdFoodItem">
                    <a href="/BirdMeal/${meal.ID}">
                        <img height="230" width="230" src="${meal.MealImages[0].Source}" />
                        <span class="title">${meal.Title}</span>
                        <span class="price">price <strong>${formatNumber(meal.Price + "")}VNĐ</strong></span>
                        <span class="btn btn-birdFoodItem">View</span>
                    </a>
                </div>
                        `;
    });
    $(".birdMeal--container").append(birdMealsHTML);
}