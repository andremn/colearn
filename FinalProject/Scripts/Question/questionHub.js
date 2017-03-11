$(function() {
    var questionHub = $.connection.questionHub;

    questionHub.client.newAnswer = function() {
        var notificationCounter = $("#notificationCounter");
        var notifcationCount = parseInt(notificationCounter.text());

        if (!isNaN(notifcationCount)) {
            notificationCounter.text(++notifcationCount);
        }
    };

    $.connection.hub.start();
});