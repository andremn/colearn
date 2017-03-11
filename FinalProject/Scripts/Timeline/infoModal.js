$(function() {
    var infoModal = $("#infoModal");

    infoModal.on("hide.bs.modal",
        function() {
            window.location = "/Account/SelectInstitution";
        });

    infoModal.modal("show");
});