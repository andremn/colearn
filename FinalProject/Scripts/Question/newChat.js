$(function() {
    var videoChatHub = $.connection.videoChatHub;

    videoChatHub.client.videoChatStarted = function(id) {
        window.location.href = "/VideoChat/Chat/" + id;
    };

    $.connection.hub.start()
        .done(function() {
            $.post("/VideoChat/CallParticipants/" + $("#chatId").val());
        });
});