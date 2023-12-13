

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
    $(".loading--part").css("display", "flex");
    var getActiveMealsRequest = $.ajax({
        type: "GET",
        url: "http://localhost:5093/odata/Meals/Active/Meal?$select=ID,Title,Price,MealImages&$expand=mealimages&$orderby=price asc",
        dataType: "json",
        contentType: "application/json"
    });

    getActiveMealsRequest.done((data, textStatus, jqXHR) => {
        $(".loading--part").css("display", "none");
        GetMeals(data);
    });

    var MealIdNotFound = sessionStorage.getItem("MealIdNotFound");
    if (MealIdNotFound != null) {
        Swal.fire({
            icon: 'error',
            title: 'Oops...',
            text: MealIdNotFound
        });
        sessionStorage.removeItem("MealIdNotFound");
    }
}

function GetMeals(meals) {
    $(".birdMeal--container").empty();
    var birdMealHTML = "";
    meals.forEach((meal) => {
        birdMealHTML += `
           <div class="col-lg-3 birdMealItem">
                <div class="birdFoodItem-container">
                    <a href="/BirdMeal/${meal.ID}">
                        <img style="height: 300px;" src="${meal.MealImages[0].Source}" />
                        <span class="title">${meal.Title}</span>
                        <span class="price">price <strong>${formatNumber(meal.Price +"")}VNĐ</strong></span>
                    </a>
                    <div>
                        <label for="quantity">quantity</label>
                        <input type="number" class="quantity" id="quantity-${meal.ID}" value="1" />
                        <span class="btn btn-addToCard" onclick='AddToCart(${meal.ID})'>Add To Cart</span>
                    </div>
                </div>
            </div>
                        `;
    });
    $(".birdMeal--container").append(birdMealHTML);
    Pagination($("#numberOfBirdMeals").val());
}

function formatNumber(n) {
    // format number 1000000 to 1,234,567
    return n.replace(/\D/g, "").replace(/\B(?=(\d{3})+(?!\d))/g, ".")
}

$("#numberOfBirdMeals").change(() => {
    Pagination($("#numberOfBirdMeals").val());
});


function Pagination(productsPerPage) {
    $(function () {
        var numberOfItems = $(".birdMeal--container .birdMealItem").length;
        var limitPerPage = productsPerPage;
        var totalPages = Math.ceil(numberOfItems / limitPerPage);
        var paginationSize = 7;
        var currentPage;

        function ShowPage(whichPage) {
            if (whichPage < 1 || whichPage > totalPages) return false;

            currentPage = whichPage;

            $(".birdMeal--container .birdMealItem").hide().slice((currentPage - 1) * limitPerPage, currentPage * limitPerPage).show();

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

$("#filterBirdMeals").change((event) => {
    if (event.target.value == "AZ") {
        var getActiveMealsRequest = $.ajax({
            type: "GET",
            url: "http://localhost:5093/odata/Meals/Active/Meal?$select=ID,Title,Price,MealImages&$expand=mealimages&$orderby=title asc",
            dataType: "json",
            contentType: "application/json"
        });

        getActiveMealsRequest.done((data, textStatus, jqXHR) => {
            $(".loading--part").css("display", "none");
            GetMeals(data);
        });
    }
    if (event.target.value == "ZA") {
        var getActiveMealsRequest = $.ajax({
            type: "GET",
            url: "http://localhost:5093/odata/Meals/Active/Meal?$select=ID,Title,Price,MealImages&$expand=mealimages&$orderby=title desc",
            dataType: "json",
            contentType: "application/json"
        });

        getActiveMealsRequest.done((data, textStatus, jqXHR) => {
            $(".loading--part").css("display", "none");
            GetMeals(data);
        });
    }

    if (event.target.value == "LH") {
        var getActiveMealsRequest = $.ajax({
            type: "GET",
            url: "http://localhost:5093/odata/Meals/Active/Meal?$select=ID,Title,Price,MealImages&$expand=mealimages&$orderby=price asc",
            dataType: "json",
            contentType: "application/json"
        });

        getActiveMealsRequest.done((data, textStatus, jqXHR) => {
            $(".loading--part").css("display", "none");
            GetMeals(data);
        });
    }

    if (event.target.value == "HL") {
        var getActiveMealsRequest = $.ajax({
            type: "GET",
            url: "http://localhost:5093/odata/Meals/Active/Meal?$select=ID,Title,Price,MealImages&$expand=mealimages&$orderby=price desc",
            dataType: "json",
            contentType: "application/json"
        });

        getActiveMealsRequest.done((data, textStatus, jqXHR) => {
            $(".loading--part").css("display", "none");
            GetMeals(data);
        });
    }
});


function SearchBirdMeals(event) {
    var sortValue = $("#filterBirdMeals").val();
    var typeSort = "";
    var sortName = ""; 
    if (sortValue == "ZA") {
        typeSort = "desc";
        sortName = "title";
    }
    if (sortValue == "AZ") {
        typeSort = "asc";
        sortName = "title";
    }

    if (sortValue == "LH") {
        typeSort = "asc";
        sortName = "price";
    }

    if (sortValue == "HL") {
        typeSort = "desc";
        sortName = "price";
    }

    var getActiveMealsRequest = $.ajax({
        type: "GET",
        url: `http://localhost:5093/odata/Meals/Active/Meal?$filter=contains(tolower(title),'${event.target.value.toLowerCase()}')&$select=ID,Title,Price,MealImages&$expand=mealimages&$orderby=${sortName} ${typeSort}`,
        dataType: "json",
        contentType: "application/json"
    });

    getActiveMealsRequest.done((data, textStatus, jqXHR) => {
        $(".loading--part").css("display", "none");
        GetMeals(data);
    });

}