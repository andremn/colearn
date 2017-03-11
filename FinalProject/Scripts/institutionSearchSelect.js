$(function() {
    $("#Institution")
        .on("loaded.bs.select",
            function() {
                $(".bs-searchbox input").attr("placeholder", "Procure uma universidade");
                // Make it big enough for desktop and mobile devices
                $(".dropdown-menu.open").attr("style", "max-width: 200px;");
            });
});