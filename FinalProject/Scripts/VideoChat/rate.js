$(function () {
    var chatId = $("#rating").data("chat-id");

    $("#rating")
        .rating({
            hoverOnClear: false,
            showClear: false,
            step: .5,
            theme: "krajee-fa",
            filledStar: "<i class='fa fa-star'></i>",
            emptyStar: "<i class='fa fa-star-o'></i>",
            clearCaption: "-",
            starCaptions: function(val) {
                return val;
            },
            starCaptionClasses: function() {
                return "label label-default label-small";
            }
        });

    $("#rateBtn")
        .on("click",
            function() {
                var data = {
                    chatId,
                    rating: $("#rating").val()
                };

                $.post("/VideoChat/Rate",
                    data,
                    function (res) {
                        if (res.result === "NEW") {
                            window.location.href = "/Question/Create?videoChatId=" + chatId + "&answerId=" + res.answerId;
                            return;
                        }

                        window.location.href = "/";
                    });
            });
})