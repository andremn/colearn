$(function() {
    var root = $("#root");
    var query = root.attr("data-query");

    $.post("/Question/LoadMaterials",
            { query },
            function(page) {
                root.html(page);
                $(".cssload-container").hide();

                $(".page-link")
                    .on("click", onPageLinkClicked);
            })
        .fail(function() {
            $(".success").addClass("hidden");
            $(".error").removeClass("hidden");
        });

    function onPageLinkClicked() {
        var page = $(this).text();

        $("#materials-section").html("");
        $(".cssload-container").show();

        $.get("/Question/GetMaterials?page=" + page,
                function(html) {
                    root.html(html);

                    $(".page-link")
                        .on("click", onPageLinkClicked);

                    $(".cssload-container").hide();

                    $("html, body")
                        .animate({
                                scrollTop: $("html").offset().top
                            },
                            400);
                })
            .fail(function() {
                $(".success").addClass("hidden");
                $(".error").removeClass("hidden");
            });
    }
});