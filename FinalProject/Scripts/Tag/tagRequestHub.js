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

    var tagRequestHub = $.connection.tagRequestHub;

    tagRequestHub.client.tagRequestsUpdated = function(requestsCount) {
        if (window.getNotifications) {
            window.getNotifications();
        }
    };

    $.connection.hub.start();
});