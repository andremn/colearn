var RecurrenceMode = {
    NONE: 0,
    DAYLY: 4,
    WEEKLY: 5,
    MONTHLY: 6
};

$(function () {
    var calendarDiv = $("#calendar");
    var addEventModal = $("#addEventModal");
    var deleteEventModal = $("#deleteEventModal");
    var recDeleteEventModal = $("#recDeleteEventModal");
    var recEditEventModal = $("#recEditEventModal");
    var joinEventModal = $("#joinEventModal");
    var eventSettingsModal = $("#eventSettingsModal");
    var eventSettingTimepicker = $("#eventSettingTimepicker");
    var startDateTimePicker = $("#startDatetimepicker");
    var endDateTimePicker = $("#endDatetimepicker");
    var startEventTimePicker = $("#startEventTimepicker");
    var endEventTimePicker = $("#endEventTimepicker");
    var allDayCheckBox = $("#allDayCheckbox");
    var recurenceInterval = $("#recurrenceInterval");
    var recurrenceCheckbox = $("#recurrenceCheckbox");
    var allDaysLink = $("#allDaysLink");
    var editingEvent = null;
    var isMonthView = false;
    var studentId = calendarDiv.data("student") || null;
    var isReadOnly = calendarDiv.data("readonly") === "True" ? true : false;
    var maxScheduleTime = calendarDiv.data("max-schedule-time");
    //var allDaysClicked = false;

    moment.locale("pt-br");
    recurenceInterval.hide();

    // Not sure why I needed to force the widget to display, but it won't work without this workaround...
    startDateTimePicker.datetimepicker().on("dp.show", showDateTimePickerWidget);
    endDateTimePicker.datetimepicker().on("dp.show", showDateTimePickerWidget);
    startEventTimePicker.datetimepicker().on("dp.show", showDateTimePickerWidget);
    endEventTimePicker.datetimepicker().on("dp.show", showDateTimePickerWidget);
    eventSettingTimepicker.datetimepicker().on("dp.show", showDateTimePickerWidget);

    function showDateTimePickerWidget() {
        $(".bootstrap-datetimepicker-widget").show();
    }

    var agendaHub = $.connection.agendaHub;

    agendaHub.client.userCalendarChanged = function (userId) {
        if (userId === studentId) {
            calendarDiv.fullCalendar("refetchEvents");
        }
    }

    calendarDiv.fullCalendar({
        lang: "pt-br",
        height: "auto",
        header: false,
        defaultView: "agendaWeek",
        timezone: "local",
        events: fetchEvents,
        dayClick: isReadOnly ? null : onCalendarDayClicked,
        eventClick: onCalendarEventClicked
    });

    function fetchEvents(start, end, timezone, callback) {
        var data = {
            start: start.toISOString(),
            end: end.toISOString(),
            studentId
        };

        $.get("/Agenda/GetEvents",
            data,
            function (eventsJson) {
                var events = [];

                for (var i = 0; i < eventsJson.length; i++) {
                    var event = eventsJson[i];

                    if (event.isAllDay) {
                        var timezoneOffset = new Date().getTimezoneOffset();
                        var startTime = moment(event.start).add(timezoneOffset, "minutes").startOf("day");
                        var endTime = startTime.clone().add(23, "hours").add(59, "minutes");

                        event.start = startTime;
                        event.end = endTime;
                    }

                    event.textColor = "white";
                    events.push(event);
                }

                callback(events);
            });
    }

    startEventTimePicker.datetimepicker().on("dp.change",
        function () {
            var date = startEventTimePicker.data("DateTimePicker").date();
            var maxScheduleDuration = moment.duration(maxScheduleTime);
            var end = editingEvent.end;
            var maxEnd = date.clone().add(maxScheduleDuration);

            if (end < maxEnd) {
                maxEnd = end;
            }
            
            endEventTimePicker.data("DateTimePicker").minDate(date);
            endEventTimePicker.data("DateTimePicker").maxDate(maxEnd);
        });

    $(".btn-settings")
        .on("click",
            function () {
                eventSettingsModal.modal("show");
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
            function () {
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

    //allDaysLink
    //    .on("click", onAllDaysLinkClicked);

    addEventModal
        .find(".btn.btn-danger")
        .on("click",
            function () {
                deleteEventModal.modal("show");
            });

    deleteEventModal
        .find(".btn.btn-danger")
        .on("click",
            function () {
                confirmEventEditDelete(false);
            });

    recDeleteEventModal.find(".btn.btn-default")
        .on("click",
            function () {
                var deleteMode = $(this).data("delete-mode");

                if (deleteMode === "single") {
                    deleteEvent(false);
                } else if (deleteMode === "all") {
                    deleteEvent(true);
                }
            });

    recEditEventModal.find(".btn.btn-default")
        .on("click",
            function () {
                var editMode = $(this).data("edit-mode");

                if (editMode === "single") {
                    editEvent(false);
                } else if (editMode === "all") {
                    editEvent(true);
                }
            });

    addEventModal
        .find(".btn.btn-success")
        .on("click",
            function () {
                var startDate = startDateTimePicker.data("DateTimePicker").date();
                var event = {
                    start: startDate.valueOf()
                };

                if (recurrenceCheckbox.is(":checked")) {
                    event.recurrenceMode = getRecurrenceMode();
                    event.recurrenceDay = startDate.day();
                }

                if (!allDayCheckBox.is(":checked")) {
                    var endDate = endDateTimePicker.data("DateTimePicker").date();

                    event.end = endDate.valueOf();
                }

                if (editingEvent) {
                    var id = editingEvent.id;

                    editingEvent = event;
                    editingEvent.id = id;
                    confirmEventEditDelete(true);
                    return;
                }

                $.post("/Agenda/New",
                        event,
                        function () {
                            calendarDiv.fullCalendar("refetchEvents");
                        })
                    .fail(function (e) {
                        alert(e);
                    });

                addEventModal.modal("hide");
            });

    joinEventModal.find(".btn.btn-success")
        .on("click",
            function () {
                var startDate = startEventTimePicker.data("DateTimePicker").date();
                var endDate = endEventTimePicker.data("DateTimePicker").date();
                var title = $("#titleEventTimepicker").val();
                
                if (!$("#joinEventForm").valid()) {
                    return;
                }

                joinEvent(title, startDate, endDate);
            });

    joinEventModal.find(".btn.btn-danger")
        .on("click",
            function () {
                leaveEvent(editingEvent.id);
            });

    eventSettingsModal.find(".btn.btn-success")
        .on("click",
            function () {
                var date = eventSettingTimepicker.data("DateTimePicker").date();
                var minutes = (date.get("hours") * 60) + date.get("minutes");
                var duration = moment.duration(minutes, "minutes");

                $.post("/Agenda/SetMaxScheduleTime",
                    { maxScheduleTime: duration.toString() },
                    function () {
                        eventSettingsModal.modal("hide");
                    });
            });

    //function onAllDaysLinkClicked() {
    //    var now = moment();

    //    startDateTimePicker.data("DateTimePicker").date(now);
    //    endDateTimePicker.data("DateTimePicker").date(now.clone().add(1, "h"));
    //    recurrenceCheckbox.parent().hide();
    //    allDaysClicked = true;
    //    addEventModal.modal("show");
    //}

    function onRecurrenceCheckBoxChanged() {
        if (recurrenceCheckbox.is(":checked")) {
            recurenceInterval.fadeIn("fast");
        } else {
            recurenceInterval.fadeOut("fast");
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

        calendarDiv.fullCalendar("refetchEvents");
    }

    function onAllDayCheckBoxChanged() {
        var endDateTimePickerContainer = $("#endDatetimepickerContainer");

        if (allDayCheckBox.is(":checked")) {
            endDateTimePickerContainer.hide("fast");

            var startDate = startDateTimePicker.data("DateTimePicker").date();

            startDateTimePicker.data("DateTimePicker").date(startDate.startOf("day"));
            endDateTimePicker.data("DateTimePicker").date(startDate.endOf("day"));
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
        fillDateFields(date, date.clone().add(30, "m"), false);
        addEventModal.modal("show");
    }

    function onCalendarEventClicked(event) {
        $("#joinEventForm").find(".has-error").removeClass("has-error");
        $("#titleEventTimepicker-error").hide();

        startEventTimePicker.data("DateTimePicker").maxDate(false).minDate(false);
        endEventTimePicker.data("DateTimePicker").maxDate(false).minDate(false);

        editingEvent = event;

        if (isReadOnly) {
            var start = event.start;
            var maxScheduleDuration = moment.duration(maxScheduleTime);
            var end = start.clone().add(maxScheduleDuration);
            var minEnd = start.clone().add(15, "m");
            var joinEventModalTitle = joinEventModal.find(".modal-title");

            joinEventModalTitle.html(
                joinEventModal.data("title-join").replace("{0}", humanizeDuration(maxScheduleTime, {
                    language: "pt",
                    delimiter: " e "
                })));
                
            startEventTimePicker.data("DateTimePicker").date(start).maxDate(editingEvent.end).minDate(start);

            if (event.isAllDay) {
                var timezoneOffset = new Date().getTimezoneOffset();
                var eventEnd = start.clone().add(timezoneOffset).endOf("day");

                if (end > eventEnd) {
                    end = eventEnd;
                }

                endEventTimePicker.data("DateTimePicker").date(end).maxDate(eventEnd).minDate(minEnd);
            } else {
                var maxEnd = event.end;

                if (end > event.end) {
                    end = event.end;
                } else {
                    maxEnd = end;
                }

                endEventTimePicker.data("DateTimePicker").date(end).maxDate(maxEnd).minDate(minEnd);
            }

            joinEventModal.find(".modal-body").show();

            if (editingEvent.isBusy) {
                joinEventModal.find(".btn.btn-danger").show();
            } else {
                joinEventModal.find(".btn.btn-danger").hide();
            }

            joinEventModal.modal("show");
            return;
        }

        if (editingEvent.isBusy) {
            joinEventModal.find(".modal-title").text(joinEventModal.data("title-leave"));
            joinEventModal.find(".modal-body").hide();
            joinEventModal.find(".btn.btn-success").hide();
            joinEventModal.find(".btn.btn-danger").show();
            joinEventModal.modal("show");
            return;
        }

        fillDateFields(editingEvent.start, editingEvent.end, true);

        addEventModal
            .find(".modal-title")
            .text(addEventModal.data("title-edit"));

        addEventModal
            .modal("show")
            .find(".btn-danger")
            .show();
    }

    function onEditDeleteEventResponse(response) {
        if (response === "OK") {
            calendarDiv.fullCalendar("refetchEvents");
            return;
        }
    }

    function joinEvent(title, start, end) {
        var diff = end.diff(start, "milliseconds");

        if (diff > maxScheduleTime) {
            var text = $(".popup").html().replace("{0}",
                humanizeDuration(maxScheduleTime, {
                    language: "pt",
                    delimiter: " e "
                }));

            $(".popup")
                .html(text)
                .fadeIn(200)
                .delay(5000)
                .fadeOut(300);
            return;
        }

        $.post("/Agenda/Join",
                {
                    id: editingEvent.id,
                    title,
                    start: start.toISOString(),
                    end: end.toISOString(),
                    studentId
                },
                function () {
                    calendarDiv.fullCalendar("refetchEvents");
                    joinEventModal.modal("hide");
                })
            .fail(function () {
                // Todo: replace alert by modal
                alert("Não foi possível atualizar o calendário");
            });
    }

    function leaveEvent(id) {
        $.post("/Agenda/Leave",
                { id },
                function () {
                    calendarDiv.fullCalendar("refetchEvents");
                })
            .fail(function () {
                // Todo: replace alert by modal
                alert("Não foi possível atualizar o calendário");
            });
    }

    function fillDateFields(start, end, isEdit) {
        if (isMonthView && !isEdit) {
            startDateTimePicker.data("DateTimePicker").date(moment());
            endDateTimePicker.data("DateTimePicker").date(moment().add(30, "m"));
            return;
        }

        startDateTimePicker.data("DateTimePicker").date(start);

        if (!start.hasTime() || allDayCheckBox.is(":checked")) {
            // Add 23 hours and 59 minutes
            var allDayStart = start.clone().local().time("00:00");
            var allDayEnd = allDayStart.clone().add(24, "h");

            allDayCheckBox.prop("checked", true);
            onAllDayCheckBoxChanged();
            startDateTimePicker.data("DateTimePicker").date(allDayStart);
            endDateTimePicker.data("DateTimePicker").date(allDayEnd);
            addEventModal.modal("show");
            return;
        }

        if (isEdit) {
            endDateTimePicker.data("DateTimePicker").date(end);
            return;
        }

        endDateTimePicker.data("DateTimePicker").date(start.clone().add(30, "m"));
    }

    function editEvent(editAll) {
        var eventData = {
            id: editingEvent.id,
            start: editingEvent.start,
            end: editingEvent.end,
            recurrenceMode: editingEvent.recurrenceMode,
            recurrenceDay: editingEvent.recurrenceDay
        };
        $.post("/Agenda/Edit",
                {
                    eventData,
                    editAll
                },
                onEditDeleteEventResponse)
            .done(function () {
                editingEvent = null;
                addEventModal.modal("hide");
            });

        editingEvent = null;
    }

    function confirmEventEditDelete(isEdit) {
        $.post("/Agenda/Event",
            { id: editingEvent.id },
            function (event) {
                if (event && event.recurrenceMode !== 0) {
                    if (isEdit) {
                        recEditEventModal.modal("show");
                        return;
                    }

                    recDeleteEventModal.modal("show");
                } else {
                    if (isEdit) {
                        editEvent(true);
                        return;
                    }

                    deleteEvent(true);
                }
            });
    }

    function deleteEvent(deleteAll) {
        var eventData = {
            id: editingEvent.id,
            start: editingEvent.start.format(),
            end: editingEvent.end.format()
        };
        $.post("/Agenda/Delete",
                {
                    eventData,
                    deleteAll
                },
                onEditDeleteEventResponse)
            .done(function () {
                editingEvent = null;
                addEventModal.modal("hide");
            });
    }

    function getRecurrenceMode() {
        var recurrenceMode = recurenceInterval.val();

        switch (recurrenceMode) {
            case "d":
                return RecurrenceMode.DAYLY;
            case "w":
                return RecurrenceMode.WEEKLY;
            case "m":
                return RecurrenceMode.MONTHLY;
            default:
                throw "Invalid recurrence mode";
        }
    }

    function initializeDatetimepicker(element) {
        element.datetimepicker({
            format: "dd/mm/yyyy HH:mm",
            stepping: 30
        });

        element.data("DateTimePicker").locale("pt-br");
    }

    function initializeDatetimepickerFormat(element, format) {
        initializeDatetimepicker(element);
        element.data("DateTimePicker").format(format);
    }

    function resetModalContent() {
        addEventModal
            .find(".modal-title")
            .text(addEventModal.data("title-new"));

        addEventModal
            .modal("show")
            .find(".btn-danger")
            .hide();

        recurenceInterval.find("option")
            .filter(function (i, e) { return $(e).val() === "day" })
            .prop("selected", true);

        allDayCheckBox.prop("checked", false);
        onAllDayCheckBoxChanged();
        recurrenceCheckbox.prop("checked", false).show();
        onRecurrenceCheckBoxChanged();
    }

    initializeDatetimepicker(startDateTimePicker);
    initializeDatetimepicker(endDateTimePicker);
    initializeDatetimepicker(startEventTimePicker);
    initializeDatetimepicker(endEventTimePicker);
    initializeDatetimepickerFormat(eventSettingTimepicker, "HH:mm");

    var maxDuration = moment.duration(maxScheduleTime);
    var maxDate = moment()
        .set("hour", maxDuration.hours())
        .set("minute", maxDuration.minutes());

    eventSettingTimepicker.data("DateTimePicker").date(maxDate);
    onCalendarViewChanged();
});