

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
    var getActiveProductsRequest = $.ajax({
        type: "GET",
        url: "http://localhost:5093/odata/Products/Active/Product?$select=ID,Name,ProductImages&$expand=productimages&$orderby=name asc",
        dataType: "json",
        contentType: "application/json"
    });

    getActiveProductsRequest.done((data, textStatus, jqXHR) => {
        $(".loading--part").css("display", "none");
        GetProducts(data);
    });

    var ProductIdNotFound = sessionStorage.getItem("ProductIdNotFound");
    if (ProductIdNotFound != null) {
        Swal.fire({
            icon: 'error',
            title: 'Oops...',
            text: ProductIdNotFound
        });
        sessionStorage.removeItem("ProductIdNotFound");
    }
}

function GetProducts(products) {
    $(".birdFood--container").empty();
    var birdFoodHTML = "";
    products.forEach((product) => {
        birdFoodHTML += `
                <div class="col-lg-3 birdFoodItem">
                    <a href="/BirdFood/${product.ID}" class="link-birdFoodItem">
                        <div>
                            <img height="230" width="230" src="${product.ProductImages[0].Source}" />
                            <span class="title">${product.Name}</span>
                            <span class="btn btn-birdFoodItem">View</span>
                        </div>
                    </a>
                </div>
                        `;
    });
    $(".birdFood--container").append(birdFoodHTML);
    Pagination($("#numberOfBirdFoods").val());
}

$("#numberOfBirdFoods").change(() => {
    Pagination($("#numberOfBirdFoods").val());
});

function Pagination(productsPerPage) {
    $(function () {
        var numberOfItems = $(".birdFood--container .birdFoodItem").length;
        var limitPerPage = productsPerPage;
        var totalPages = Math.ceil(numberOfItems / limitPerPage);
        var paginationSize = 7;
        var currentPage;

        function ShowPage(whichPage) {
            if (whichPage < 1 || whichPage > totalPages) return false;

            currentPage = whichPage;

            $(".birdFood--container .birdFoodItem").hide().slice((currentPage - 1) * limitPerPage, currentPage * limitPerPage).show();

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

$("#filterBirdFoods").change((event) => {
    if (event.target.value == "AZ") {
        var getActiveProductsRequest = $.ajax({
            type: "GET",
            url: "http://localhost:5093/odata/Products/Active/Product?$select=ID,Name,ProductImages&$expand=productimages&$orderby=name asc",
            dataType: "json",
            contentType: "application/json"
        });

        getActiveProductsRequest.done((data, textStatus, jqXHR) => {
            GetProducts(data);
        });
    }
    if (event.target.value == "ZA") {
        var getActiveProductsRequest = $.ajax({
            type: "GET",
            url: "http://localhost:5093/odata/Products/Active/Product?$select=ID,Name,ProductImages&$expand=productimages&$orderby=name desc",
            dataType: "json",
            contentType: "application/json"
        });

        getActiveProductsRequest.done((data, textStatus, jqXHR) => {
            GetProducts(data);
        });
    }
});

function SearchBirdFoods(event) {
    var sortValue = $("#filterBirdFoods").val();
    var typeSort = "asc";
    if (sortValue == "ZA") {
        typeSort = "desc";
    }
    var getActiveProductsRequest = $.ajax({
        type: "GET",
        url: `http://localhost:5093/odata/Products/Active/Product?$filter=contains(tolower(name),'${event.target.value.toLowerCase()}')&$select=ID,Name,ProductImages&$expand=productimages&$orderby=name ${typeSort}`,
        dataType: "json",
        contentType: "application/json"
    });

    getActiveProductsRequest.done((data, textStatus, jqXHR) => {
        GetProducts(data);
    });
}