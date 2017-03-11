$(function() {
    var reasonModal = $("#reasonModal");
    var reasonText = $("#Reason");

    reasonModal.on("hidden.bs.modal",
        function() {
            reasonText.val("");
        });
    reasonModal.on("shown.bs.modal",
        function() {
            reasonText.focus();
        });

    $(".deny-link")
        .on("click",
            function() {
                var institutionId = $(this).attr("data-id");
                var insititutionRequestOwnerEmail = $(this).attr("data-owner-email");

                reasonModal.modal("show");
                $("#InstitutionRequestId").val(institutionId);

                var ownerEmailInput = $("#ManagerEmail");

                if (insititutionRequestOwnerEmail) {
                    ownerEmailInput.val(insititutionRequestOwnerEmail);
                    ownerEmailInput.attr("readonly", "readonly");
                }
            });

    var responseModal = $("#responseModal");
    var sendingModal = $("#sendingModal");

    $("#sendReasonButton")
        .on("click",
            function() {
                var reasonForm = $("#ReasonForm");
                var dataToSend = reasonForm.serialize();

                sendingModal.modal("show");

                $.post("/Institution/DenyInstitutionRegisterRequest",
                        dataToSend,
                        function() {
                            reasonModal.modal("hide");
                            $("#responseModalTitle").html("Justificativa enviada");
                        })
                    .fail(function() {
                        $("#responseModalTitle").html("Justificativa não enviada");
                    })
                    .always(function(res) {
                        sendingModal.modal("hide");
                        responseModal.modal("show");
                        $("#responseModalBody").html(res.Message);

                        if (res.Success) {
                            removeInstitution(res.Id);
                        }
                    });
            });

    function removeInstitution(id) {
        $("#" + id).remove();

        var pendingCount = $("tr").length - 1;

        if (pendingCount === 0) {
            $(".table-responsive").addClass("hidden");
            $("#noRequestsPending").removeClass("hidden");
        }
    }
});