$(function () {
    var form = $("#searchForm");
    var ratingInput = $("#ratingInput");
    var spinner = $(".cssload-container");
    var lastRatingValue = 0;

    function initRating(elements, showCaption, captionFunc) {
        if (!elements.rating) {
            return;
        }

        elements.each(function (index, elem) {
            var element = $(elem);

            element.rating({
                hoverOnClear: false,
                showCaption: showCaption || false,
                theme: "krajee-fa",
                filledStar: "<i class='fa fa-star'></i>",
                emptyStar: "<i class='fa fa-star-o'></i>",
                starCaptions: captionFunc ? function (value) {
                    return captionFunc(element, value);
                } : undefined,
                starCaptionClasses: function () {
                    return "label label-default label-small";
                }
            });
        });
    }

    function startCaptions(elem, value) {
        if (value == null) {
            return "";
        }

        var ratings = elem.data("ratings");
        var text = ratings === 1 ? "avaliação" : "avaliações";

        return value + " (" + ratings + " " + text + ")";
    }

    initRating(ratingInput);
    initRating($(".search-user-rating .kv-ltr-theme-fa-star"), true, startCaptions);

    ratingInput.on("rating.change", function (event, value) {
        lastRatingValue = parseFloat(value).toFixed(1);
        $("#ratingLabel b").text(lastRatingValue);
    });

    ratingInput.on("rating.hover", function (event, value) {
        $("#ratingLabel b").text(value.toFixed(1));
    });

    ratingInput.on("rating.hoverleave", function () {
        $("#ratingLabel b").text(lastRatingValue);
    });

    ratingInput.on("rating.clear", function () {
        lastRatingValue = 0.0;
        $("#ratingLabel b").text("0.0");
    });

    $("#institutions")
        .on("loaded.bs.select",
            function () {
                // Make it big enough for desktop and mobile devices
                $(".dropdown-menu.open").attr("style", "max-width: 200px;");
            });

    $("#searchBtn")
        .on("click",
            function(e) {
                e.preventDefault();
                $("#recommended").hide();
                $("#search").html("");
                spinner.show();

                var data = form.serialize();
                var ratingVal = $("#ratingInput").val();
                var ratingValLoc = ratingVal.replace(".", ",");

                data = data.replace("StudentMinAvgRating=" + ratingVal, "StudentMinAvgRating=" + ratingValLoc);

                $.post(form[0].action,
                    data,
                    function (results) {
                        if (!$("#search").is(":visible")) {
                            $("#recommended").hide();
                            $("#search").html(results).prop("visible", true).show();
                        } else {
                            $("#search").html(results);
                        }

                        spinner.hide();
                        initRating($(".search-user-rating .kv-ltr-theme-fa-star"), true, startCaptions);
                    });
            });

    $("#clearBtn")
        .on("click",
            function (e) {
                e.preventDefault();
                $("#recommended").show();
                $("#search").html("").prop("visible", false).hide();
                $(".search-advance").slideUp("fast");
            });

    $.get("/Search/GetRecommendedInstructors",
        function (html) {
            spinner.hide();
            $("#recommended").show().html(html);
            initRating($(".search-user-rating .kv-ltr-theme-fa-star"), true, startCaptions);
        });
});