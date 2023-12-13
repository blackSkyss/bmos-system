

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
        url: `http://localhost:5093/odata/Products/Active/Product/${urlPath[2]}?$expand=productimages, productmeals($expand=meal($expand=mealImages))`,
        dataType: "json",
        contentType: "application/json",
    });

    getActiveProductsRequest.done((data, textStatus, jqXHR) => {
        $(".loading--part").css("display", "none");
        $(".mainImage img").attr("src", data.ProductImages[0].Source);
        $(".image-item").empty();
        var imageItemsHTML = "";
        data.ProductImages.forEach((image) => {
            if (image.Source == data.ProductImages[0].Source) {
                imageItemsHTML += `<img class="col-lg-3 imgageActive" src="${image.Source}" onclick="image('${image.Source}')" />`;
            } else {
                imageItemsHTML += `<img class="col-lg-3" src="${image.Source}" onclick="image('${image.Source}')" />`;
            }
        });
        $(".image-item").append(imageItemsHTML);
        $("#productName").append(data.Name);
        $("#birdFoodPrice").append(formatNumber(data.Price + ""));
        $(".productDetails").append(`${data.Description}`);
        $("#numberOfMeals").append(`${data.ProductMeals.length}`);
        GetBirdMeals(data.ProductMeals);
    });

    getActiveProductsRequest.fail((jqXHR, textStatus, errorThrown) => {
        $(".loading--part").css("display", "none");
        switch (jqXHR.status) {
            case 404: {
                var error = JSON.parse(jqXHR.responseText);
                sessionStorage.setItem("ProductIdNotFound", error.Message[0].DescriptionError[0]);
                location.assign("/BirdFoods");
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

function GetBirdMeals(birdMeals) {
    $(".birdMeals--container").empty();
    var birdMealsHTML = "";
    if (birdMeals.length == 0) {
        $(".pagination").hide();
    } else {
        birdMeals.forEach((birdMeal) => {
            birdMealsHTML += `
         <div class="col-lg-3 birdMealItem">
                <div class="birdFoodItem-container">
                    <a href="/BirdMeal/${birdMeal.Meal.ID}">
                        <img style="height: 300px;" src="${birdMeal.Meal.MealImages[0].Source}" />
                        <span class="title">${birdMeal.Meal.Title}</span>
                        <span class="price">price <strong>${formatNumber(birdMeal.Meal.Price + "")}VNĐ</strong></span>
                    </a>
                    <div>
                        <label for="quantity-${birdMeal.Meal.ID}">quantity</label>
                        <input type="number" class="quantity" step="1" id="quantity-${birdMeal.Meal.ID}" value="1" />
                        <span class="btn btn-addToCard" onclick='AddToCart(${birdMeal.Meal.ID})'>Add To Cart</span>
                    </div>
                </div>
        </div>
        `;
        });
        $(".birdMeals--container").append(birdMealsHTML);
        Pagination($("#numberOfBirdMeals").val());
    }
}

$("#numberOfBirdMeals").change(() => {
    Pagination($("#numberOfBirdMeals").val());
});

function Pagination(productsPerPage) {
    $(function () {
        var numberOfItems = $(".birdMeals--container .birdMealItem").length;
        var limitPerPage = productsPerPage;
        var totalPages = Math.ceil(numberOfItems / limitPerPage);
        var paginationSize = 7;
        var currentPage;

        function ShowPage(whichPage) {
            if (whichPage < 1 || whichPage > totalPages) return false;

            currentPage = whichPage;

            $(".birdMeals--container .birdMealItem").hide().slice((currentPage - 1) * limitPerPage, currentPage * limitPerPage).show();

            $(".pagination li").slice(1, -1).remove();

            GetPageList(totalPages, currentPage, paginationSize).forEach(item => {
                $("<li>").empty();
                $("<li>").addClass("page-item").addClass(item ? "curent-page" : "dots").toggleClass("active", item === currentPage).append($("<a>").attr({ href: "javascript:void(0)" }).text(item || "...")).insertBefore(".next-page");
            });

            $(".previous-page").toggleClass("disable", currentPage === 1);
            $(".next-page").toggleClass("disable", currentPage === totalPages);
            return true;
        }
        $(".pagination .numberPages ul").append(
            $("<li>").addClass("page-item").addClass("previous-page").append($("<a>").attr({ href: "javascript:void(0)" }).text("Prev")),
            $("<li>").addClass("page-item").addClass("next-page").append($("<a>").attr({ href: "javascript:void(0)" }).text("Next"))
        );
        if (numberOfItems == 0) {
            $(".pagination .numberPages ul").empty();
        }
        ShowPage(1);

        $(document).on("click", ".pagination li.curent-page:not(.active)", function () {
            return ShowPage(+$(this).text());
        });

        $(".next-page").on("click", function () {
            return ShowPage(currentPage + 1);
        });

        $(".previous-page").on("click", function () {
            return ShowPage(currentPage - 1);
        });
    });
}

function GetPageList(totalPages, page, maxLength) {
    function range(start, end) {
        return Array.from(Array(end - start + 1), (_, i) => i + start);
    }

    var sideWidth = maxLength < 9 ? 1 : 2;
    var leftWidth = (maxLength - sideWidth * 2 - 3) >> 1;
    var rightWidth = (maxLength - sideWidth * 2 - 3) >> 1;


    if (totalPages <= maxLength) {
        return range(1, totalPages);
    }

    if (page <= maxLength - sideWidth - 1 - rightWidth) {
        return range(1, maxLength - sideWidth - 1).concat(0, range(totalPages - sideWidth + 1, totalPages));
    }

    if (page >= totalPages - sideWidth - 1 - rightWidth) {
        return range(1, sideWidth).concat(0, range(totalPages - sideWidth - 1 - rightWidth - leftWidth, totalPages));
    }

    return range(1, sideWidth).concat(0, range(page - leftWidth, page + rightWidth), 0, range(totalPages - sideWidth + 1, totalPages));
}