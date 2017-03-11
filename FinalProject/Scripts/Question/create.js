$(function () {
    var videoAnswerModal = $("#videoCallQuestionModal");

    if (videoAnswerModal.length > 0) {
        videoAnswerModal.modal("show");
    }

    var questionForm = $("#CreateQuestionForm");

    window.tinymce.init({
        setup: function (editor) {
            editor.on("init",
                function () {
                    this.getDoc().body.style.fontSize = "12pt";
                });
        },
        selector: "#Description",
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

    questionForm.submit(function (e) {
        e.preventDefault();

        var hasErrors = !questionForm.valid();

        hasErrors |= !validateTagsInputField();

        if (hasErrors) {
            return false;
        }

        window.tinyMCE.activeEditor.save();
        $.post("/Question/Create",
            questionForm.serialize(),
            function (args) {
                if (args.pendingTagsMessage) {
                    $("#moderatingTagsModalBtn")
                        .on("click",
                            function () {
                                window.location.href = args.redirectUrl;
                            });
                    $("#moderatingTagsModal").modal("show");
                    $("#moderatingTagsModalBody").html(args.pendingTagsMessage);
                } else {
                    window.location.href = args.redirectUrl;
                }
            });
    });

    var titleInput = $("#Title");

    titleInput.blur(function () {
        setTimeout(function () {
            $("#suggestions").addClass("hidden").html("");
        }, 350);
    });

    titleInput.on("input",
        function () {
            var query = $(this).val();

            if (!query || query.length < 3) {
                $("#suggestions").addClass("hidden").html("");
                return;
            }

            var html = "";

            $.post("/Question/GetSuggestions",
                { query },
                function (data) {
                    var suggestions = data.suggestions;

                    if (suggestions.length === 0) {
                        $("#suggestions").addClass("hidden").html("");
                        return;
                    }

                    for (var i = 0; i < suggestions.length; i++) {
                        var suggestion = suggestions[i];

                        html += "<li class='suggestion-item'>";
                        html += "<p class='dropdown-item'><a href='/Question/Details/" +
                            suggestion.id +
                            "' role='option' target='_blank'>" +
                            suggestion.title +
                            "</a>" +
                            "<br/><span>" + suggestion.answers + "</span>" +
                            "</p>";
                        html += "</li>";
                    }

                    $("#suggestions").html(html).removeClass("hidden");
                });
        });

    var tagsInput = $("#Tags");
    var isTagsInputInitialized = false;

    $("#addNewTagBtn")
        .on("click",
            function () {
                tagsInput.tagsinput("add", tagsInput.tagsinput("input").val());
                tagsInput.tagsinput("input").focus();
            });

    function validateTagsInputField() {
        if (tagsInput.val() === "") {
            $(".bootstrap-tagsinput").removeClass("bootstrap-tagsinput-success");
            $(".bootstrap-tagsinput").addClass("bootstrap-tagsinput-error");
            $(".bootstrap-tagsinput").parent().parent().addClass("has-error");
            $(".bootstrap-tagsinput").parent().parent().removeClass("has-success");
            $("span[data-valmsg-for='Tags']").html(tagsInput.attr("data-val-required"));

            return false;
        }

        $(".bootstrap-tagsinput").removeClass("bootstrap-tagsinput-error");
        $(".bootstrap-tagsinput").addClass("bootstrap-tagsinput-success");
        $(".bootstrap-tagsinput").parent().parent().removeClass("has-error");
        $(".bootstrap-tagsinput").parent().parent().addClass("has-success");
        $("span[data-valmsg-for='Tags']").html("");

        return true;
    }

    tagsInput.tagsinput({
        addOnBlur: false,
        freeInput: true,
        tagClass: "label label-primary label-big"
    });

    tagsInput.tagsinput("input")
        .on("focus",
            function () {
                $(".bootstrap-tagsinput").addClass("bootstrap-tagsinput-focus");
            });

    tagsInput.tagsinput("input")
        .on("blur",
            function () {
                $(".bootstrap-tagsinput").removeClass("bootstrap-tagsinput-focus");

                if (isTagsInputInitialized) {
                    validateTagsInputField();
                }
            });

    tagsInput.tagsinput("input").attr("style", "border: none; height: 27px");

    tagsInput.on("itemAdded",
        function () {
            if (!isTagsInputInitialized) {
                isTagsInputInitialized = true;
            }

            setTimeout(function () {
                tagsInput.tagsinput("input").val("");
            }, 1);
        });

    tagsInput.tagsinput("input")
        .on("input",
            function () {
                var query = $(this).val();

                if (!query || query.length < 3) {
                    $("#tag-suggestions").addClass("hidden").html("");
                    return;
                }

                var html = "";
                var addedTags = tagsInput.val();

                $.getJSON("/Tag/Find",
                    { query, addedTags },
                    function (suggestions) {
                        if (suggestions.length === 0) {
                            $("#tag-suggestions").addClass("hidden").html("");
                            return;
                        }

                        for (var i = 0; i < suggestions.length; i++) {
                            var suggestion = suggestions[i];

                            html += "<li class='tag-suggestion-item'>";
                            html += "<a class='dropdown-item'style='cursor: pointer'>" + suggestion + "</a>";
                            html += "</li>";
                        }

                        $("#tag-suggestions")
                            .html(html)
                            .removeClass("hidden")
                            .children()
                            .on("click",
                                function () {
                                    tagsInput.tagsinput("add", $(this).children()[0].innerText);
                                    tagsInput.tagsinput("input").focus();
                                    $("#tag-suggestions").addClass("hidden").html("");
                                });
                    },
                    "json");
            });
});