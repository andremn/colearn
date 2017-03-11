$(function () {
    var questionId = $("#details").data("question-id");
    var questionHub = $.connection.questionHub;

    questionHub.client.onNewAnswer = function (dataJson) {
        var data = JSON.parse(dataJson);

        if (data.questionId === questionId) {
            getAnswers();
        }
    }

    window.tinymce.init({
        setup: function (editor) {
            editor.on("init",
                function () {
                    this.getDoc().body.style.fontSize = "12pt";
                });
        },
        selector: "#AnswerText",
        content_css: "/Content/QuestionDetails.css",
        statusbar: false,
        min_height: 200,
        language: "pt_BR",
        plugins: "textcolor",
        toolbar: [
            "styleselect, fontsizeselect, undo, redo",
            "bold, italic, underline, strikethrough, alignleft, aligncenter, alignright, alignjustify, forecolor",
            "cut, copy, paste, bullist, numlist, outdent, indent, blockquote, removeformat"
        ],
        menubar: ""
    });

    $("#ConfirmButton")
        .on("click",
            function () {
                var button = $(this);

                button.prop("disabled", true);
                window.tinyMCE.activeEditor.save();

                var formData = $("#NewAnswerForm").serialize();

                $.post("/Question/AnswerQuestion", formData).always(function () {
                    button.prop("disabled", false);
                });
            });

    var rating = $(".question-rating");

    function initializeRating(ratings) {
        ratings.each(function (index, item) {
            var current = $(item);
            var answerId = current.data("answer-id");

            current.rating({
                hoverOnClear: false,
                showClear: false,
                step: 0.5,
                theme: "krajee-fa",
                filledStar: "<i class='fa fa-star'></i>",
                emptyStar: "<i class='fa fa-star-o'></i>",
                clearCaption: "0 (0)",
                starCaptions: function () {
                    return current.val() + " (" + current.data("count") + ")";
                },
                starCaptionClasses: function () {
                    return "label label-default label-small";
                }
            });

            current.rating("update", Number(current.val().replace(",", ".")).toFixed(1));

            current.on("rating.change",
                function (event, value) {
                    var target = $(this);

                    $.post("/Question/RateAnswer",
                        { value, id: answerId },
                        function (response) {
                            if (response.success) {
                                var count = parseInt(current.data("count"));

                                current.attr("data-count", count + 1);
                                target.rating("refresh", {
                                    readonly: true
                                });

                                target.prev().prev().attr("title", target.attr("data-already-rated"));
                            }
                        });
                });

            current.prev().prev().attr("title", current.attr("title"));
        });
    }

    initializeRating(rating);

    $("#videoModal")
        .on("hidden.bs.modal",
            function () {
                var videoElement = $(".modal-content video").get(0);

                videoElement.pause();
                videoElement.src = "";
            });

    $("#confirmAgendaModal")
        .find(".btn.btn-success")
        .on("click",
            function () {
                var answerAuthor = $("#confirmAgendaModal").data("student-id");

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

    function getAnswers(callback) {
        $.get("/Question/GetAnswersViewForQuestion/" + questionId,
        function (html) {
            var container = $(".answers-container");

            container.html(html);
            initializeRating(container.find(".question-rating"));

            $(".video-answer")
                .on("click",
                    function () {
                        $("#videoModal")
                            .modal("show")
                            .find(".modal-content video")
                            .attr("src", $(this).data("video-src"));
                    });

            $(".btn-video-chat")
                .on("click",
                    function() {
                        var source = $(this);
                        var answerAuthor = source.data("answer-author");

                        $.post("/Agenda/HasEvent",
                            { studentId: answerAuthor },
                            function(hasEvent) {
                                if (!hasEvent) {
                                    $("#confirmAgendaModal").data("student-id", source.data("answer-author")).modal("show");
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

            if (callback) {
                callback();
            }
        });
    };

    getAnswers();
})