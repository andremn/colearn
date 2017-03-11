var ESCAPE_KEY_CODE = 27;
var SERVER_COMMUNICATION_ERROR_MESSAGE = "Erro ao se comunicar com o servidor.";

$(function() {
    var nodeToDelete;
    var moveOnFail = false;
    var ignoreDeleteEvent = false;
    var sourceMergingTag;
    var targetMergingTag;
    var isMerging = false;

    $.jstree.defaults.core.themes.variant = "large";
    $.jstree.defaults.dnd.touch = true;

    var treeContainer = $(".tags-tree");

    $("#confirmDeletionBtn")
        .on("click",
            function() {
                treeContainer.jstree(true).delete_node(nodeToDelete);
            });

    $("#mergeCancellationBtn")
        .on("click",
            function() {
                cancelMerge();
            });

    $("#mergeConfirmationBtn")
        .on("click",
            function() {
                showStatusBox("Juntando...");
                sendMergeRequest({
                        sourceId: sourceMergingTag.id,
                        targetId: targetMergingTag.id
                    },
                    function(error) {
                        cancelMerge();
                        showPopup(error);
                    });
            });

    function showStatusBox(text) {
        $("#statusBoxContent").html(text);
        $("#statusBoxContainer").slideDown(150);
    }

    function hideStatusBox() {
        $("#statusBoxContainer").slideUp(100);
        $("#statusBoxContent").html("");
    }

    function cancelMerge() {
        isMerging = false;
        hideStatusBox();
    }

    function showPopup(meessage) {
        $(".popup").html(meessage).fadeIn(200).delay(3000).fadeOut(300);
    }

    $(document)
        .keyup(function(e) {
            if (e.keyCode === ESCAPE_KEY_CODE) {
                if (isMerging) {
                    cancelMerge();
                }
            }
        });

    function getMenuItems($node) {
        var tree = treeContainer.jstree(true);

        if ($node.parent.indexOf("pending") > -1) {
            return {
                "Details": {
                    "label": "Detalhes",
                    "icon": "fa fa-info",
                    "action": function() {
                        openRequestDetails($node);
                    }
                },
                "Merge": {
                    "label": "Juntar",
                    "icon": "fa fa-pencil",
                    "action": function() {
                        showStatusBox("Selecione uma tag para juntar com '" +
                            $node.text +
                            "'.<br/><small>Pressione 'ESC' para cancelar.</small>");
                        isMerging = true;
                        sourceMergingTag = $node;
                    }
                },
                "Delete": {
                    "label": "Recusar",
                    "icon": "fa fa-times",
                    "action": function() {
                        rejectTag($node);
                    }
                }
            };
        }

        if ($node.id.indexOf("root") > -1 ||
            $node.id.indexOf("pending") > -1) {
            return [];
        }

        var createItem = {
            "label": "Nova tag",
            "icon": "fa fa-plus",
            "action": function() {
                $node = tree.create_node($node);
                tree.edit($node);
            }
        };
        if ($node.id.indexOf("approved") > -1) {
            return {
                "Create": createItem
            };
        }

        return {
            "Create": createItem,
            "Rename": {
                "label": "Renomear",
                "icon": "fa fa-pencil",
                "action": function() {
                    tree.edit($node);
                }
            },
            "Delete": {
                "label": "Deletar",
                "icon": "fa fa-times",
                "action": function() {
                    nodeToDelete = $node;
                    $("#deletionConfirmation").modal("show");
                }
            }
        };
    }

    treeContainer.jstree({
        'core': {
            strings: {
                "New node": "Nova tag",
                "Loading ...": "Carregando tags..."
            },
            "check_callback": function(operation, node, parent) {
                if (operation === "move_node") {
                    if (node.parent === parent.id ||
                        parent.id.indexOf("root") > -1 ||
                        parent.id.indexOf("pending") > -1) {
                        return false;
                    }

                    return node.parent != null && parent.id !== "#";
                } else if (operation === "create_node") {
                    node.icon = "fa fa-tag";
                }

                return true;
            },
            "data": {
                "url": "/Tag/GetTagsForInstitution?institutionId=0",
                "dataType": "json",
                "contentType": "application/json",
                "data": function(node) {
                    return { 'id': node.id };
                }
            }
        },
        "contextmenu": {
            "items": getMenuItems
        },
        "plugins": [
            "contextmenu", "dnd", "search", "sort",
            "state", "types", "wholerow", "unique"
        ]
    });

    treeContainer.on("ready.jstree",
        function() {
            treeContainer.jstree("close_all");

            var rootNodeId = treeContainer.find(".fa-graduation-cap").parent().attr("id");
            var rootNode = treeContainer.jstree(true).get_node(rootNodeId);

            treeContainer.jstree(true).toggle_node(rootNode);

            var pendingTagNodes = treeContainer.find(".jstree-node li a i.fa-clock-o").parent();

            pendingTagNodes.each(function(index, item) {
                var pendingTagNodeId = $(item).attr("id");
                var node = treeContainer.jstree(true).get_node(pendingTagNodeId);

                treeContainer.jstree(true).toggle_node(node);
            });
        });

    $("#expandAllBtn")
        .on("click",
            function() {
                treeContainer.jstree("open_all");
            });

    $("#collpaseAllBtn")
        .on("click",
            function() {
                treeContainer.jstree("close_all");
            });

    function openRequestDetails(node) {
        var id = node.id.replace("pendingTag_", "");

        $.get("/Tag/GetTagRequestDetails/" + id,
            function (response) {
                var modal;
                var link;
                var modalBody;

                if (response.source === "question") {
                    modal = $("#requestQuestionDetailsModal");
                    link = "Question/Details/" + response.data.id;
                    modalBody = modal.find(".modal-body h6");
                    modalBody.html(modalBody.html().replace("{0}", response.data.title));
                } else {
                    modal = $("#requestStudentDetailsModal");
                    link = "Account/Details/" + response.data.id;
                    modalBody = modal.find(".modal-body h6");
                    modalBody.html(modalBody.html().replace("{0}", response.data.name));
                }

                modal.find(".btn.btn-primary")
                    .on("click",
                        function() {
                            window.open(link, "_blank");
                        });
                modal.modal("show");
            });
    }

    function rejectTag(tag) {
        $.post("/Tag/RejectTag",
                { tagId: tag.id },
                function(args) {
                    if (!args.success) {
                        fail(args.error);
                    } else {
                        ignoreDeleteEvent = true;
                        var tree = treeContainer.jstree(true);

                        tree.delete_node(tag);
                    }
                },
                "json")
            .fail(function() {
                fail(SERVER_COMMUNICATION_ERROR_MESSAGE);
            });
    }

    function sendNodeMoved(data, success, fail) {
        $.post("/Tag/MoveTagToNewParent",
                data,
                function(args) {
                    if (!args.success) {
                        fail(args.error);
                    } else {
                        success(args.data.id);
                    }
                },
                "json")
            .fail(function() {
                fail(SERVER_COMMUNICATION_ERROR_MESSAGE);
            });
    }

    function sendNodeChanged(data, node, fail) {
        $.post("/Tag/CreateOrUpdateTag",
                data,
                function(args) {
                    if (!args.success) {
                        fail(args.error);
                    } else {
                        if (args.data.isNew) {
                            var tree = treeContainer.jstree(true);

                            tree.set_id(node, args.data.id.toString());
                        }
                    }
                },
                "json")
            .fail(function() {
                fail(SERVER_COMMUNICATION_ERROR_MESSAGE);
            });
    }

    function deleteNode(node, fail) {
        $.post("/Tag/DeleteTag",
                { tagId: node.id, parentId: node.parent },
                function(args) {
                    if (!args.success) {
                        fail(args.error);
                    }
                },
                "json")
            .fail(function() {
                fail(SERVER_COMMUNICATION_ERROR_MESSAGE);
            });
    }

    treeContainer.on("rename_node.jstree",
        function(e, data) {
            sendNodeChanged({
                    id: isNaN(parseInt(data.node.id)) ? "-1" : data.node.id,
                    text: data.node.text,
                    parentId: data.node.parent,
                    institutionId: getRootTagId(data.node)
                },
                data.node,
                function(error) {
                    showPopup(error);
                });
        });

    function getRootTagId(node) {
        for (var i = 0; i < node.parents.length; i++) {
            var parent = node.parents[i];

            if (parent.indexOf("root_") !== -1) {
                return parent;
            }
        }

        return "-1";
    }

    treeContainer.on("delete_node.jstree",
        function(e, data) {
            if (ignoreDeleteEvent) {
                ignoreDeleteEvent = false;
                return;
            }

            deleteNode(data.node,
                function(error) {
                    var tree = treeContainer.jstree(true);

                    tree.create_node(data.node.parent, data.node);
                    showPopup(error);
                });
        });

    treeContainer.on("move_node.jstree",
        function(e, data) {
            if (moveOnFail) {
                moveOnFail = false;
                return;
            }

            var tree = treeContainer.jstree(true);

            if (data.parent === data.old_parent) {
                return;
            }

            sendNodeMoved({
                    id: data.node.id,
                    newParentId: data.parent,
                    oldParentId: data.old_parent,
                    institutionId: getRootTagId(data.node)
                },
                function(id) {
                    tree.set_id(data.node, id.toString());
                },
                function(error) {
                    moveOnFail = true;
                    tree.move_node(data.node, data.old_parent);
                    showPopup(error);
                });
        });

    function sendMergeRequest(data, fail) {
        $.post("/Tag/MergeTags",
                data,
                function(args) {
                    if (!args.success) {
                        fail(args.error);
                    } else {
                        var tree = treeContainer.jstree(true);

                        ignoreDeleteEvent = true;
                        tree.delete_node(sourceMergingTag);
                        cancelMerge();
                        $(".popup").html("Tags juntadas com sucesso!").fadeIn(200).delay(3000).fadeOut(300);
                    }
                },
                "json")
            .fail(function() {
                fail(SERVER_COMMUNICATION_ERROR_MESSAGE);
            });
    }

    treeContainer.on("select_node.jstree",
        function(e, data) {
            if (!isMerging) {
                return;
            }

            targetMergingTag = data.node;
            $("#mergeConfirmationContent")
                .html("Deseja juntar a tag '" + sourceMergingTag.text + "' com a tag '" + data.node.text + "'?");
            $("#mergeConfirmation").modal("show");
        });

    $("#searchForm")
        .submit(function(e) {
            e.preventDefault();

            var searchText = $("#tagSearch").val();

            if (!searchText) {
                treeContainer.jstree(true).search(searchText, true, true);
                return;
            }

            treeContainer.jstree(true).search(searchText, true, true);

            var $container = $("html, body");
            var $scrollTo = $(".jstree-anchor.jstree-search");

            $container.animate({ scrollTop: $scrollTo.offset().top - $container.offset().top, scrollLeft: 0 }, 300);
        });
});