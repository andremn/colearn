var AVAILABLE_EVENT_BACK_COLOR = "#4caf50";

$(function () {
    var calendarDiv = $("#calendar");
    var addEventModal = $("#addEventModal");
    var startDateTimePicker = $("#startDatetimepicker");
    var endDateTimePicker = $("#endDatetimepicker");
    var allDayCheckBox = $("#allDayCheckbox");
    var recurenceInterval = $("#recurrenceInterval");
    var recurrenceCheckbox = $("#recurrenceCheckbox");
    var allDaysLink = $("#allDaysLink");
    var isMonthView = false;
    var allDaysClicked = false;

    recurenceInterval.hide().next().hide().next().hide();

    calendarDiv
        .fullCalendar({
            lang: "pt-br",
            height: "auto",
            header: false,
            defaultView: "agendaWeek",
            dayClick: onCalendarDayClicked,
            eventClick: onCalendarEventClicked
        });

    $(".btn-calendar-prev")
        .on("click",
            function () {
                calendarDiv.fullCalendar("prev");
                onCalendarViewChanged();
            });

    $(".btn-calendar-next")
        .on("click",
            function () {
                calendarDiv.fullCalendar("next");
                onCalendarViewChanged();
            });

    $(".btn-calendar")
        .on("click",
            function() {
                var target = $("#" + $(this).attr("for"));
                
                target.focus();
            });

    $("#weekViewBtn")
        .on("click",
            function () {
                calendarDiv.fullCalendar("changeView", "agendaWeek");
                allDaysLink.fadeIn("fast");
                onCalendarViewChanged();
                isMonthView = false;
                onViewModeButtonClicked();
            });

    $("#monthViewBtn")
        .on("click",
            function () {
                calendarDiv.fullCalendar("changeView", "month");
                allDaysLink.fadeOut("fast");
                onCalendarViewChanged();
                isMonthView = true;
                onViewModeButtonClicked();
            });

    allDayCheckBox.on("change", onAllDayCheckBoxChanged);

    recurrenceCheckbox
        .on("click", onRecurrenceCheckBoxChanged);

    allDaysLink
        .on("click", onAllDaysLinkClicked);

    addEventModal
        .find(".btn.btn-success")
        .on("click",
            function () {
                var startDate = startDateTimePicker.data("DateTimePicker").date();
                var endDate = endDateTimePicker.data("DateTimePicker").date();

                if (allDaysClicked) {
                    var now = moment();
                    var lastWeekDay = calendarDiv.fullCalendar("getView").end.local();
                    var diff = lastWeekDay.diff(now, "days");

                    now = now.set({ "hour": 0, "minute": 0, "second": 0 });

                    for (var i = 0; i <= diff; i++) {
                        createEvent(startDate.clone().add(i, "d"), startDate.clone().add(i, "d"));
                    }

                    allDaysClicked = false;
                    return;
                }

                if (recurrenceCheckbox.is(":checked")) {
                    createRecurentEvent(startDate, endDate);
                    return;
                }
                
                if (allDayCheckBox.is(":checked")) {
                    createAllDayEvent(startDate);
                    return;
                }

                createEvent(startDate, endDate);
            });

    function onAllDaysLinkClicked() {
        var now = moment();

        startDateTimePicker.data("DateTimePicker").date(now);
        endDateTimePicker.data("DateTimePicker").date(now.clone().add(1, "h"));
        recurrenceCheckbox.parent().hide();
        allDaysClicked = true;
        addEventModal.modal("show");
    }

    function onRecurrenceCheckBoxChanged() {
        if (recurrenceCheckbox.is(":checked")) {
            recurenceInterval.fadeIn("fast")
                .next().fadeIn("fast")
                .next().fadeIn("fast");
        } else {
            recurenceInterval.fadeOut("fast")
                .next().fadeOut("fast")
                .next().fadeOut("fast");
        }
    }

    function onViewModeButtonClicked() {
        var viewModeBtns = $(".btn-view-mode");

        viewModeBtns.each(function (i, item) {
            var element = $(item);

            if (element.hasClass("btn-primary")) {
                element.removeClass("btn-primary");
                element.addClass("btn-default");
            } else {
                element.removeClass("btn-default");
                element.addClass("btn-primary");
            }
        });
    }

    function onAllDayCheckBoxChanged () {
        var endDateTimePickerContainer = $("#endDatetimepickerContainer");

        if (allDayCheckBox.is(":checked")) {
            endDateTimePickerContainer.hide("fast");
        } else {
            endDateTimePickerContainer.show("fast");
        }
    }

    function onCalendarViewChanged() {
        var view = calendarDiv.fullCalendar("getView");

        $(".calendar-month").html(view.title);
    }

    function onCalendarDayClicked(date) {
        resetModalContent();

        if (isMonthView) {
            startDateTimePicker.data("DateTimePicker").date(moment());
        } else {
            startDateTimePicker.data("DateTimePicker").date(date);
        }

        if (!date.hasTime() && !isMonthView) {
            // Add 23 hours and 59 minutes
            var allDayDate = date.clone().add(1439, "m");

            allDayCheckBox.prop("checked", true);
            onAllDayCheckBoxChanged();
            endDateTimePicker.data("DateTimePicker").date(allDayDate);
            addEventModal.modal("show")
            return;
        }

        endDateTimePicker.data("DateTimePicker").date(date.clone().add(30, "m"));
        allDayCheckBox.prop("checked", false);
        addEventModal.modal("show");
    }

    function onCalendarEventClicked(event) {
    }

    function initializeDatetimepicker(element) {
        element.datetimepicker({
            locale: "pt-br"
        });
    }

    function resetModalContent() {
        recurenceInterval.find("option")
            .filter(function (i, e) { return $(e).val() === "day" })
            .prop("selected", true);
        recurrenceCheckbox.prop("checked", false).show();
        onRecurrenceCheckBoxChanged();
    }

    function createRecurentEvent(startDate, endDate) {
        var recurrenceMode = recurenceInterval.val();
        var recurrenceExpireDate = startDate.clone().add(1, "y");
        var diff;
        var addMode;
        var events = [];

        switch (recurrenceMode) {
            case "day":
                diff = recurrenceExpireDate.diff(startDate, "days");
                addMode = "d";
                break;
            case "week":
                diff = recurrenceExpireDate.diff(startDate, "weeks");
                addMode = "w";
                break;
            case "month":
                diff = recurrenceExpireDate.diff(startDate, "months");
                addMode = "M";
                break;
            default:
                throw "Invalid recurrence mode";
        }

        if (allDayCheckBox.is(":checked")) {

        } else {
            for (var i = 0; i < diff; i++) {
                var start = startDate.clone().add(i, addMode);
                var end = endDate.clone().add(i, addMode);
                var event = {
                    title: "Disponível",
                    start: start.format(),
                    end: end.format(),
                    color: AVAILABLE_EVENT_BACK_COLOR,
                    allDay: false
                };

                events.push(event);
            }
        }

        calendarDiv.fullCalendar("addEventSource", events);
    }

    function createEvent(startDate, endDate) {
        var event = {
            title: "Disponível",
            start: startDate.format(),
            end: endDate.format(),
            color: AVAILABLE_EVENT_BACK_COLOR,
            allDay: false
        };

        calendarDiv.fullCalendar("renderEvent", event, true);
    }

    function createAllDayEvent(date) {
        // Adds a all day event to "all day" slot
        var event = {
            title: "Disponível",
            start: date.format(),
            color: AVAILABLE_EVENT_BACK_COLOR,
            allDay: true
        };

        calendarDiv.fullCalendar("renderEvent", event, true);

        // Adds a event to fill the whole day column (agenda view)
        event = {
            title: "Disponível",
            start: date.format(),
            end: date.add(24, "h").add(-1, "m").format(),
            color: AVAILABLE_EVENT_BACK_COLOR,
            allDay: false
        };

        calendarDiv.fullCalendar("renderEvent", event, true);
    }

    initializeDatetimepicker(startDateTimePicker);
    initializeDatetimepicker(endDateTimePicker);
    onCalendarViewChanged();
})