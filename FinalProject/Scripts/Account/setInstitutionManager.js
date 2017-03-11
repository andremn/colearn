$(function() {
    $("#searchBtn")
        .on("click",
            function() {
                var email = $("#Email").val();

                $.post("/Account/FindStudentDetails",
                    { email },
                    function(data) {
                        if (data.Found) {
                            $("#FirstName").val(data.FirstName);
                            $("#LastName").val(data.LastName);
                        } else {
                            $("#notFoundModal").modal("show");
                        }
                    });
            });
});