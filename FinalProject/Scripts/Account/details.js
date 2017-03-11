$(function () {
    var currentUserId = $("#userIdInput").val();
    var videoChatBtn = $(".btn-video-chat");
    var onlineTextElement = $("#connectionOnlineText");
    var offlineTextElement = $("#connectionOfflineText");

    window.onUserStatusChanged = function (userId, isOnline) {
        if (currentUserId === userId) {
            updateUserConnectionStatus(isOnline);
        }
    };

    window.onConnectionStatusHubReady = function(hub) {
        var userId = $("#userIdInput").val();

        hub.server.isUserOnline(userId).done(function (isOnline) {
            updateUserConnectionStatus(isOnline);
        });
    }

    function updateUserConnectionStatus(isOnline) {
        if (isOnline) {
            onlineTextElement.show();
            offlineTextElement.hide();
            videoChatBtn.removeClass("offline").addClass("online");
        } else {
            offlineTextElement.show();
            onlineTextElement.hide();
            videoChatBtn.removeClass("online").addClass("offline");
        }
    }

    $(".profile-img")
        .on("mouseenter",
            function() {
                $(".profile-img-overlay").fadeIn();
            });

    $(".profile-img-overlay")
        .on("mouseleave",
            function() {
                $(".profile-img-overlay").fadeOut();
            })
        .on("click",
            function() {
                $("#fileupload").click();
            });

    $("#fileupload")
        .change(function() {
            previewPicture(this);
            $("#setProfilePicBtn").show();
        });

    $("#removeProfilePicBtn")
        .on("click",
            function() {

            });

    $("#setProfilePicBtn")
        .on("click",
            function() {
                var formData = new FormData($("#uploadProfilePicForm")[0]);

                $.ajax({
                    url: "/Account/SetProfilePicture",
                    type: "POST",
                    beforeSend: function() {},
                    success: function() {
                        location.reload();
                    },
                    error: function() {},
                    data: formData,
                    cache: false,
                    contentType: false,
                    processData: false
                });
            });

    function previewPicture(input) {
        if (input.files && input.files[0]) {
            var reader = new FileReader();

            reader.onload = function(e) {
                $(".profile-img").css("background-image", "url(" + e.target.result + ")");
            };

            reader.readAsDataURL(input.files[0]);
        }
    }

    $(".btn-video-chat")
        .on("click",
            function () {
                var source = $(this);

                if (source.hasClass("offline")) {
                    return;
                }

                var answerAuthor = source.data("answer-author");
                var questionId = source.data("question-id");

                $.post("/Agenda/HasEvent",
                    { studentId: answerAuthor },
                    function (hasEvent) {
                        if (!hasEvent) {
                            $("#confirmAgendaModal").modal("show");
                            return;
                        }

                        var data = {
                            userId: answerAuthor,
                            presenterId: answerAuthor,
                            questionId
                        };

                        $.post("/VideoChat/RequestNewVideoChat",
                            data,
                            requestNewVideoChatCallback);

                    });
            });

    $("#confirmAgendaModal")
        .find(".btn.btn-success")
        .on("click",
            function () {
                var answerAuthor = $(".btn-video-chat").data("answer-author");

                window.location.href = "/Agenda/Index/" + answerAuthor;
            });


    function requestNewVideoChatCallback(response) {
        if (response.chatId) {
            var data = { id: response.chatId };

            $.post("/VideoChat/EnterVideoChat",
                data,
                function (result) {
                    enterVideoChatCallback(result, response.chatId);
                });
        } else {
            // Todo: handle error properly
            alert(response);
        }
    }

    function enterVideoChatCallback(response, chatId) {
        if (response === "OK") {
            window.location.href = "/VideoChat/NewChat/" + chatId;
        } else {
            // Todo: handle error properly
            alert(response);
        }
    }
});