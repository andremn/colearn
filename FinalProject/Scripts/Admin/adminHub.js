$(function() {
    function updateTitleCounter(titleElem, count) {
        var str = " ";

        if (count === 0 || count > 1) {
            str += titleElem.attr("data-title-plural");
        } else {
            str += titleElem.attr("data-title-singular");
        }

        titleElem.attr("title", count + str);
    }

    var institutionRequestHub = $.connection.institutionRequestHub;

    institutionRequestHub.client.institutionRequestsUpdated = function(requestsCount) {
        updateTitleCounter($("#notificationCounter").html(requestsCount).parent(), requestsCount);
    };

    $.connection.hub.start();
});