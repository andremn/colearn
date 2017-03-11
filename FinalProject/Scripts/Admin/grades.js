$(function() {
    var gradeForm = $("#createGradeForm");
    var gradesList = $(".list-group");

    gradeForm.on("submit",
        function(e) {
            e.preventDefault();

            if (!gradeForm.valid()) {
                return;
            }

            $.post(gradeForm.attr("action"),
                gradeForm.serialize(),
                function(item) {
                    gradesList.append(item);
                    initiListItems(gradesList.children().last().find(".grade-list-item"));
                    gradeForm.find("input").val("");
                })
            .fail(function(msg) {
                    alert(msg);
                });
        });

    $.fn.editable.defaults.mode = 'inline';

    function initiListItems(elements) {
        elements.editable({
            params: function (params) {
                params["id"] = $(this).data('pk');
                params["name"] = params.value;
                params["order"] = $(this).data('order');
                return params;
            }
        });

        gradesList.sortable({
            onUpdate: onListUpdated
        });
    }

    initiListItems($(".grade-list-item"));

    function onListUpdated(e) {
        var gradeItem = $(e.item).find("a");
        var data = {
            id: gradeItem.data("pk"),
            name: gradeItem.editable("getValue", true).toString(),
            order: e.newIndex + 1
        };

        $.post("/Admin/UpdateGrade/", data, function (response) {
            gradeItem.data("order", response.order);
        });
    }
});