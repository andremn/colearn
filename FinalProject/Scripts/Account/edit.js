$(function () {
    var hash = window.location.hash;

    $('#tabs a[href="' + hash + '"]').tab("show");

    $("#tabs a").click(function (e) {
        e.preventDefault();
        $(this).tab("show");
    });

    $("ul.nav-tabs > li > a").on("shown.bs.tab", function (e) {
        var id = $(e.target).attr("href").substr(1);

        window.location.hash = id;
    });

    $("#tabs").tab();

    var tagsList = $(".help-tags");
    var emptyTag = $(".empty-tags");
    var tagsInput = $("#AddTagForm").find("input[name=tags]");

    tagsInput.tagsinput({
        freeInput: true,
        addOnBlur: false,
        tagClass: "label label-primary label-big"
    });

    tagsInput.tagsinput("input")
        .on("input",
            function() {
                var query = $(this).val();

                if (!query || query.length < 3) {
                    $("#tag-suggestions").addClass("hidden").html("");
                    return;
                }

                var html = "";
                var addedTags = tagsInput.val();

                $.getJSON("/Tag/Find",
                    { query, addedTags },
                    function(suggestions) {
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
                                function() {
                                    tagsInput.tagsinput("add", $(this).children()[0].innerText);
                                    tagsInput.tagsinput("input").focus();
                                    $("#tag-suggestions").addClass("hidden").html("");
                                });
                    },
                    "json");
            });

    tagsInput.tagsinput("input")
        .on("focus",
            function() {
                $(".bootstrap-tagsinput").addClass("bootstrap-tagsinput-focus");
            });

    tagsInput.tagsinput("input")
        .on("blur",
            function() {
                $(".bootstrap-tagsinput").removeClass("bootstrap-tagsinput-focus");
            });

    tagsInput.tagsinput("input").attr("style", "border: none; height: 27px; width: 210px");

    tagsInput.tagsinput("input")
        .on("input",
            function() {
                if (!$(this).val()) {
                    $("#addTagBtn").prop("disabled", true);
                    $("#addTagsBtn").prop("disabled", true);
                } else {
                    $("#addTagBtn").prop("disabled", false);
                    $("#addTagsBtn").prop("disabled", false);
                }
            });

    tagsInput.on("itemAdded",
        function() {
            setTimeout(function() {
                    tagsInput.tagsinput("input").val("");
                },
                1);
        });

    $("#addTagBtn")
        .on("click",
            function() {
                tagsInput.tagsinput("add", tagsInput.tagsinput("input").val());
            });

    $("#addTagsBtn")
        .on("click",
            function() {
                var data = $("#AddTagForm").serialize();

                $.post("/Account/AddInstructorTags",
                    data,
                    function(response) {
                        if (response.success) {
                            var tagsCount = $(".tag-box").length;

                            if (tagsCount === 0) {
                                emptyTag.hide();
                            }

                            tagsInput.tagsinput("input").val("");
                            tagsInput.tagsinput("removeAll");
                            tagsList.children().find(".remove-tag").off("click", onRemoveClicked);
                            tagsList.append(response.html);
                            tagsList.children().find(".remove-tag").on("click", onRemoveClicked);

                            if (response.notAddedTagsMessage) {
                                $("#moderatingTagsModal").modal("show");
                                $("#moderatingTagsModalBody").html(response.notAddedTagsMessage);
                            }
                        }
                    });
            });

    $(".remove-tag").on("click", onRemoveClicked);

    function onRemoveClicked() {
        var data = $(this).parent().serialize();
        var elementToRemove = $(this).parent().parent();

        $.post("/Account/RemoveInstructorTag",
            data,
            function(response) {
                if (response.success) {
                    elementToRemove.hide("fast",
                        function() {
                            elementToRemove.remove();
                        });

                    var tagsRemaining = $(".tag-box").length;

                    if (tagsRemaining === 0) {
                        emptyTag.show();
                    }
                }
            });
    }
});