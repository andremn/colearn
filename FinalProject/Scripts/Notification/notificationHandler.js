var NOTIFICATION_HTML_TEMPLATE = "<div class='notification-item' data-action-url='{action}'>" +
                                    "<i class='{icon} notification-icon'>&nbsp;&nbsp;</i>" +
                                    "<span>{title}</span>" +
                                 "</div>";

$(function () {
    var notificationHub = $.connection.notificationHub;
    var notificationCounter = $("#notificationCounter b");
    var notificationPopup = $(".notification-popup");
    var notificationToggle = $(".notification-toggle");

    notificationHub.client.newNotification = function (notificationCollection) {
        if (notificationCollection.notifications.length === 0) {
            return;
        }

        insertNewNotification(JSON.parse(notificationCollection));
    };
    
    function getNotifications() {
        $.post("/Notification/GetNotifications",
            function (notificationCollection) {
                if (notificationCollection.notifications.length === 0) {
                    return;
                }

                notificationPopup.html("");
                notificationCounter.html("0");
                insertNewNotification(notificationCollection);
            });
    }

    window.getNotifications = getNotifications;
    $.connection.hub.start().done(getNotifications);

    $(document).mouseup(function(e) {
        if (notificationToggle.is(e.target) || notificationToggle.has(e.target).length !== 0) {
            toggleNotificationPopup();
        } else if (!notificationPopup.is(e.target) && notificationPopup.has(e.target).length === 0) {
            hideNotificationPopup();
        }
    });

    function toggleNotificationPopup() {
        if (notificationPopup.hasClass("open")) {
            hideNotificationPopup();
        } else {
            showNotificationPopup();
        }
    }

    $(".notification-item").on("click", onNotificationItemClicked);

    $(window).on("resize", function () {
        if ($(window).width() > 768) {
            notificationPopup.css("transform-origin", (notificationPopup.width() - 114) + "px top");
        } else {
            notificationPopup.css("transform-origin", (notificationPopup.width() - 61) + "px top");
        }
    });

    function onNotificationItemClicked() {
        var url = $(this).data("action-url");

        hideNotificationPopup();
        window.location.href = url;
    }

    function hideNotificationPopup() {
        notificationPopup.removeClass("open");
    }

    function showNotificationPopup() {
        notificationPopup.addClass("open");
    }

    function insertNewNotification(notificationCollection) {
        var notifications = notificationCollection.notifications;

        for (var i = 0; i < notifications.length; i++) {
            var notification = notifications[i];
            var notificationHtml = NOTIFICATION_HTML_TEMPLATE.replace("{title}", notification.title);
            var icon = getIconForNotificationCategory(notification.category);

            notificationHtml = notificationHtml.replace("{icon}", icon);
            notificationHtml = notificationHtml.replace("{action}", notification.actionUrl);

            if (notificationPopup.children(".notification-item").length === 0) {
                notificationPopup.html("");
            }

            notificationPopup.prepend(notificationHtml);
            notificationPopup.children().first()
                .on("click", onNotificationItemClicked);
        }

        var oldNotificationCount = parseInt(notificationCounter.text());

        if (isNaN(oldNotificationCount)) {
            oldNotificationCount = 0;
        }

        oldNotificationCount += notificationCollection.count;
        notificationCounter.html(oldNotificationCount);
    }

    function getIconForNotificationCategory(category) {
        switch (category) {
            case "newAnswer":
                return "fa fa-question";
            case "tagApproved":
                return "fa fa-tag";
            case "newTags":
                return "fa fa-tags";
            default:
                return "";
        }
    }
})