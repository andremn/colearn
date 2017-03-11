var XS_WIDTH = 768;
var SM_WIDTH = 768;
var MD_WIDTH = 992;
var LG_WIDTH = 1200;

$(function() {
    var timelineSection = $("#timeline");
    var institutionId = timelineSection.data("inst-id");
    var filterStudentItems = $("#studentItemsCheckbox");
    var filterStudentInstitutionItems = $("#studentInstitutionItemsCheckbox");
    var isLoadingItems = false;
    var hasMore = true;
    var hasFilter = false;

    $("#toggleFilterBoxBtn")
        .on("click",
            function() {
                $("#filterModal").modal("show");
            });

    $(".filter-selected .remove")
        .on("click",
            function () {
                hasFilter = false;
                $("#filterForm")[0].reset();
                timelineSection.html("");
                requestItems(null, onRequestItemsResponse);
                $(".filter-selected").addClass("hidden");
            });

    $("#filterModal")
        .find(".btn.btn-primary")
        .on("click",
            function () {
                hasFilter = true;
                requestItems(false,
                    function(response) {
                        if (response.filterText) {
                            $(".filter-selected")
                                .removeClass("hidden")
                                .find("span")
                                .text(response.filterText);
                        }

                        timelineSection.html("");
                        loadItems(response.items, false);
                    });
            });

    filterStudentItems.on("change",
        function () {
            if ($(this).is(":checked")) {
                filterStudentInstitutionItems.prop("disabled", true);
                filterStudentInstitutionItems.prop("checked", false);
            } else {
                filterStudentInstitutionItems.prop("disabled", false);
            }
        });

    var timelineBlocks = $(".cd-timeline-block");

    timelineBlocks.each(function() {
        $(this).removeClass("is-hidden").addClass("bounce-in");
    });

    startTimelineHub();
    requestItems(null, onRequestItemsResponse);

    $(window)
        .on("scroll",
            function() {
                if (isLoadingItems || !hasMore) {
                    return;
                }

                if ($(window).scrollTop() + $(window).height() === $(document).height()) {
                    $(".cssload-container").removeClass("hidden");

                    var lastItemId = timelineSection.children().last().children().first().attr("id");

                    requestItems(lastItemId, onRequestItemsResponse);
                }
            });

    function onRequestItemsResponse(response) {
        loadItems(response.items, false);
        hasMore = response.hasMore;
    }

    function requestItems(lastId, callback) {
        isLoadingItems = true;
        $(".cssload-container").removeClass("hidden");

        var data = $("#filterForm").serialize();
        var maxItems = getNumberOfItemsToRetrieve();
        var userId = null;

        if (hasFilter) {
            data += "&showOnlyStudentItems=" +
                filterStudentItems.is(":checked");

            data += "&showOnlyStudentInstitutionItems=" +
                filterStudentInstitutionItems.is(":checked");
        } else {
            var itemsType = timelineSection.data("items-type") || "all";

            userId = $("#Id").val() || timelineSection.data("user-id");
            data += "&itemsType=" + itemsType;
        }

        data += "&maxItems=" + maxItems;

        if (lastId) {
            data += "&lastId=" + lastId;
        }

        if (userId) {
            data += "&userId=" + userId;
        }

        $.post("/Timeline/LoadItems",
            data,
            function(response) {
                if (callback) {
                    callback(response);
                }
            });
    }

    function loadItems(items, isNewItem) {
        if (items.length > 0 || $(".cd-timeline-block").length > 0) {
            $(".cd-timeline-empty").addClass("hidden");
        } else {
            $(".cd-timeline-empty").removeClass("hidden");
            $(".cssload-container").addClass("hidden");
            isLoadingItems = false;
            return;
        }

        for (var i = 0; i < items.length; i++) {
            var item = items[i];

            if (isNewItem) {
                timelineSection.prepend(item);
                timelineSection.children().first().on("click", itemClicked);
            } else {
                timelineSection.append(item);
                timelineSection.children().last().on("click", itemClicked);
            }
        }

        $(".cd-timeline-title")
            .ellipsis({
                lines: 3,
                ellipClass: "ellip",
                responsive: true
            });

        $(".cssload-container").addClass("hidden");
        isLoadingItems = false;

        function itemClicked(e) {
            var source = $(e.target);

            if (source.hasClass("cd-timeline-username")) {
                e.stopPropagation();
                return true;
            }

            window.location.href = "/Question/Details/" + $(this).children().first().attr("id");
            return true;
        }
    }

    function startTimelineHub() {
        var timelineHub = $.connection.timelineHub;

        timelineHub.client.onNewItemAdded = function (newItem) {
            if (newItem.InstitutionId === institutionId) {
                createView(newItem);
            }
        };

        $.connection.hub.start();
    }

    function createView(model) {
        $.post("/Timeline/LoadPageForItem",
            { model },
            function(item) {
                loadItems([item], true);
            });
    }

    function getNumberOfItemsToRetrieve() {
        var width = $(window).width();

        if (width < XS_WIDTH ||
            width >= SM_WIDTH && width < MD_WIDTH ||
            width >= MD_WIDTH && width < LG_WIDTH) {
            return 10;
        } else {
            return 12;
        }
    }
});