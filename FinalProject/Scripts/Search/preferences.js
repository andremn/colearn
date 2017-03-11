$(function () {
    var similaritySlider = $("#similaritySlider");
    var similariryValue = $("#similarityValue");
    var gradeSelected = $("#grades");
    var completedGradeCheckBox = $("#gradeCompletedCheckBox");
    
    similaritySlider.slider({
        min: 1,
        max: 100,
        value: parseFloat(similaritySlider.val()),
        step: 1,
        tooltip: "hide"
    });
    
    function updateSliderText(value) {
        similariryValue.text(value + "%");
    }

    updateSliderText(similaritySlider.slider().data("slider").getValue());

    similaritySlider.on("slide",
        function (e) {
            updateSliderText(e.value);
        });

    $(".kv-ltr-theme-fa-star")
        .rating({
            hoverOnClear: false,
            step: 0.5,
            theme: "krajee-fa",
            filledStar: "<i class='fa fa-star'></i>",
            emptyStar: "<i class='fa fa-star-o'></i>",
            clearCaption: "Qualquer avaliação",
            starCaptions: function (val) {
                return val;
            },
            starCaptionClasses: function () {
                return "label label-default label-small";
            }
        });

    $("#saveBtn")
        .on("click",
            function () {
                var form = $("#preferencesForm");
                var grade = gradeSelected.val();

                if (!form.valid() && grade !== "") {
                    return;
                }

                var data = form.serialize();

                $.post("/Preference/Update",
                    data,
                    function (response) {
                        if (response.success) {
                            $(".popup").html("Preferências atualizadas")
                                .fadeIn(200)
                                .delay(3000)
                                .fadeOut(300);
                        }
                    });
            });

    $("#resetBtn")
        .on("click",
            function () {
                $.post("/Preference/Reset")
                .done(function (response) {
                    if (response.success) {
                        similaritySlider.slider("setValue", response.preferences.MinSimilarity);
                        updateSliderText(similaritySlider.slider().data("slider").getValue());
                        $("#minRating").rating("update", response.preferences.MinRating);
                        $("#maxRating").rating("update", response.preferences.MaxRating);

                        $(".popup")
                            .html("Preferências restauradas")
                            .fadeIn(200)
                            .delay(3000)
                            .fadeOut(300);
                    }
                });
            });
})