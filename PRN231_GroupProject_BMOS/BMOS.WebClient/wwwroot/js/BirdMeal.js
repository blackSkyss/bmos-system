

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
    var urlPath = window.location.pathname.split('/');
    $(".loading--part").css("display", "flex");
    var getActiveProductsRequest = $.ajax({
        type: "GET",
        url: `http://localhost:5093/odata/Meals/Active/Meal/${urlPath[2]}?$expand=MealImages,productmeals($expand=product($expand=productImages))`,
        dataType: "json",
        contentType: "application/json",
    });

    getActiveProductsRequest.done((data, textStatus, jqXHR) => {
        $(".loading--part").css("display", "none");
        $(".mainImage img").attr("src", data.MealImages[0].Source);
        $(".image-item").empty();
        var imageItemsHTML = "";
        data.MealImages.forEach((image) => {
            if (image.Source == data.MealImages[0].Source) {
                imageItemsHTML += `<img class="col-lg-3 imgageActive" src="${image.Source}" onclick="image('${image.Source}')" />`;
            } else {
                imageItemsHTML += `<img class="col-lg-3" src="${image.Source}" onclick="image('${image.Source}')" />`;
            }
        });
        $(".image-item").append(imageItemsHTML);
        $("#title").append(data.Title);
        $("#birdMealPrice").append(formatNumber(data.Price + ""));
        $("#AddToCart").attr("onclick", `AddToCart(${data.ID})`);
        $(".mealDetails").append(`${data.Description}`);
        GetProductMeals(data.ProductMeals);
    });

    getActiveProductsRequest.fail((jqXHR, textStatus, errorThrown) => {
        $(".loading--part").css("display", "none");
        switch (jqXHR.status) {
            case 404: {
                var error = JSON.parse(jqXHR.responseText);
                sessionStorage.setItem("MealIdNotFound", error.Message[0].DescriptionError[0]);
                location.assign("/BirdMeals");
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

function image(imageSrc) {
    $(".mainImage img").attr("src", imageSrc);
    var imageOptions = $(".image-item").find("img");
    for (var i = 0; i < imageOptions.length; i++) {
        if (imageOptions[i].classList.contains("imgageActive")) {
            imageOptions[i].classList.remove("imgageActive");
        }
        if (imageOptions[i].src == imageSrc) {
            imageOptions[i].classList.add("imgageActive");
        }
    }
}

function formatNumber(n) {
    // format number 1000000 to 1,234,567
    return n.replace(/\D/g, "").replace(/\B(?=(\d{3})+(?!\d))/g, ".")
}

function GetProductMeals(productMeals) {
    $(".products").empty();
    var productsHTML = "";
    productMeals.forEach((productMeal) => {
        productsHTML += `
            <div style="margin-bottom: 1.5rem;" class="col-lg-3 birdMealItem">
                <div class="birdFoodItem-container">
                    <a href="/BirdFood/${productMeal.Product.ID}">
                        <img src="${productMeal.Product.ProductImages[0].Source}" />
                        <span class="title">${productMeal.Product.Name}</span>
                        <span class="price">price <strong>${formatNumber(productMeal.Price + "")}VNĐ</strong> / <span id="quality">${productMeal.Amount} Kilogram</span></span>
                        <span class="btn btn-BirdMeal">View</span>
                    </a>
                </div>
            </div>
        `;
    });
    $(".products").append(productsHTML);
}