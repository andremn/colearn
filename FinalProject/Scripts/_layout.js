$(function () {
    var connectionStatusHub = $.connection.connectionStatusHub;

    if (window.onUserStatusChanged) {
        connectionStatusHub.client.statusChanged = window.onUserStatusChanged;
    } else {
        connectionStatusHub.client.statusChanged = function () {
            // ignored
        }
    }

    var videoChatHub = $.connection.videoChatHub;
    var videoChatModal = $("#videoChatCallingModal");

    videoChatHub.client.videoChatCalling = function (user, id) {
        videoChatModal.find(".btn-success")
            .on("click",
                function () {
                    $.post("/VideoChat/EnterVideoChat",
                        { id },
                        function (response) {
                            if (response === "OK") {
                                window.location.href = "/VideoChat/Chat/" + id;
                            }
                        });
                });

        videoChatModal.find(".btn-danger")
            .on("click",
                function () {
                    $.post("/VideoChat/RefuseCall/", { id });
                });

        var modalBody = videoChatModal.find(".modal-body");

        modalBody.text(modalBody.text().replace("{0}", user));
        videoChatModal.modal("show");
    };

    videoChatHub.client.videoChatRefused = function (user) {
        var refusedModal = $("#videoChatRefusedModal");
        var refusedModelBody = refusedModal.find(".modal-body");

        videoChatModal.modal("hide");
        refusedModelBody.text(refusedModelBody.text().replace("{0}", user));

        refusedModal.find(".btn-primary")
            .on("click",
                function() {
                    window.history.go(-1);
                });

        refusedModal.modal("show");
    }

    $.connection.hub.start().done(function () {
        if (window.onConnectionStatusHubReady) {
            window.onConnectionStatusHubReady(connectionStatusHub);
        }
    });

    var optionsMenu = $(".options-menu");
    var optionsMenuToggle = $("#optionsMenuToggle");
    
    $(document).mouseup(function(e) {
        if (optionsMenuToggle.is(e.target) || optionsMenuToggle.has(e.target).length !== 0) {
            optionsMenu.toggleClass("open");
        } else if (!optionsMenu.is(e.target) && optionsMenu.has(e.target).length === 0) {
            optionsMenu.removeClass("open");
        }
    });

    $(window).on("resize", function () {
        optionsMenu.css("transform-origin", (optionsMenu.width() - 18) + "px top");
    });

    $("#searchButton")
        .on("click",
            function () {

            });

    $("#loginButton")
        .on("click",
            function () {
                window.location.href = "/Account/Login";
            });

    var groups = $(".input-group");

    groups.find(".fa").addClass("fa-fw");

    $(".dropdown").on("show.bs.dropdown", function () {
        $(this).find(".dropdown-menu").first().stop(true, true).slideDown("fast");
    });

    $(".dropdown").on("hide.bs.dropdown", function () {
        $(this).find(".dropdown-menu").first().stop(true, true).slideUp("fast");
    });
});