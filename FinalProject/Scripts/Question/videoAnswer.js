var ERASER_SIZE = 50;

$(function() {
    var updateDrawingCallback;
    var hub = $.connection.webRtcHub;
    var videoChatHub = $.connection.videoChatHub;
    var chatId = parseInt($("canvas").data("chat-id"));
    var localVideoElement = document.querySelector("#localVideo");
    var noDeviceErrorModal = $("#noDeviceErrorModal");
    var remoteVideoElement = document.querySelector("#remoteVideo");
    var isPresenter = $("canvas").data("is-presenter") === "True" ? true : false;
    var myConnection;
    var myMediaStream;
    var startRecording;
    var stopRecording;
    var videoSelect;

    $.connection.logging = true;

    hub.client.updateDrawing = function (id, data) {
        if (id !== chatId) {
            return;
        }

        if (updateDrawingCallback) {
            updateDrawingCallback(data);
        }
    };

    videoChatHub.client.videoChatEnded = function (id) {
        if (id !== chatId) {
            return;
        }

        endChat();
    };
    
    videoChatHub.client.videoChatError = function (id, error) {
        $("#noDeviceErrorModal")
            .modal("show")
            .find(".modal-body")
            .html("<h6>" + error + "</h6>");

        noDeviceErrorModal
            .on("hidden.bs.modal",
                function() {
                    window.location.href = "/";
                });
    };

    $.connection.hub.start(function() {
        console.log("Connected to signal server.");

        var selectCameraModal = $("#selectCameraModal");

        videoSelect = selectCameraModal.find(".modal-body select");

        function getDevices(devices) {
            for (var i = 0; i !== devices.length; ++i) {
                var device = devices[i];
                var option = document.createElement("option");

                option.value = device.deviceId;

                if (device.kind === "videoinput") {
                    option.text = device.label || "Camêra " + (videoSelect.length + 1);
                    videoSelect.append(option);
                } else {
                    console.log("Some other kind of source: ", device);
                }
            }

            selectCameraModal.find(".btn-primary")
                .on("click",
                    function () {
                        init();
                    });

            selectCameraModal.modal("show");
        }

        navigator.mediaDevices.enumerateDevices()
            .then(getDevices);
    });

    function endChat() {
        if (isPresenter) {
            stopRecording();
            window.location.href = "/";
            return;
        }

        window.location.href = "/VideoChat/Rate/" + chatId;
    }

    function createConnection() {
        console.log("Creating RTCPeerConnection...");

        var connectionConfig = {
             "iceServers": [{
                  "url": "stun:stun.l.google.com:19302"
             }]
        };
        var connection = new window.RTCPeerConnection(connectionConfig);

        connection.onicecandidate = function(event) {
            if (event.candidate) {
                console.log("New ICE Candidate: " + JSON.stringify(event.candidate));
                var data = JSON.stringify({ "candidate": event.candidate });

                hub.server.send(chatId, data);
            }
        };

        connection.onaddstream = function(event) {
            var streamUrl = window.URL.createObjectURL(event.stream);

            remoteVideoElement.onloadeddata = function() {
                if (isPresenter && startRecording) {
                    startRecording();
                }
            };

            remoteVideoElement.src = streamUrl;
            remoteVideoElement.autoplay = "autoplay";
            console.log("Stream URL: " + streamUrl);
        };

        if (myMediaStream) {
            connection.addStream(myMediaStream);
        }

        if (!isPresenter) {
            connection.createOffer(function (desc) {
                connection.setLocalDescription(desc,
                    function () {
                        hub.server.send(chatId, JSON.stringify({ "sdp": desc }));
                    });
            },
            function (error) {
                console.log("Error creating session description: " + error);
            });
        }

        return connection;
    }

    hub.client.newMessage = function (id, data) {
        if (id !== chatId) {
            return;
        }

        var message = JSON.parse(data),
            connection = myConnection || createConnection(null);

        console.log("Received new message: " + JSON.stringify(message));

        if (message.sdp) {
            connection.setRemoteDescription(new window.RTCSessionDescription(message.sdp),
                function() {
                    if (connection.remoteDescription.type === "offer") {
                        console.log("Received offer, sending answer...");

                        connection.createAnswer(function(desc) {
                                connection.setLocalDescription(desc,
                                    function() {
                                        var data = JSON.stringify({ 'sdp': connection.localDescription });

                                        hub.server.send(chatId, data);
                                    });
                            },
                            function(error) { console.log("Error creating session description: " + error); });
                    } else if (connection.remoteDescription.type === "answer") {
                        console.log("Got an answer");
                    }
                });
        } else if (message.candidate) {
            console.log("Adding ice candidate: " + JSON.stringify(message.candidate));
            connection.addIceCandidate(new window.RTCIceCandidate(message.candidate));
        }

        myConnection = connection;
    };

    var formatTime = function(totalSeconds) {
        var hours = Math.floor(totalSeconds / 3600);
        var minutes = Math.floor((totalSeconds - (hours * 3600)) / 60);
        var seconds = totalSeconds % 60;

        if (hours < 10) {
            hours = "0" + hours;
        }

        if (minutes < 10) {
            minutes = "0" + minutes;
        }

        if (seconds < 10) {
            seconds = "0" + seconds;
        }

        return hours + ":" + minutes + ":" + seconds;
    };

    function init() {
        var recording;
        var canvasStreaming;
        var audioStreaming;
        var whiteBoard;
        var isColorPickerOpen = false;
        var selectedColor = "black";
        var isEraserSelected = false;
        var isDrawing;
        var timerInterval;
        var duration = 0;

        function showColorPicker() {
            $(".tool-bar.color-picker").css("opacity", 1);
            isColorPickerOpen = true;
        }

        function hideColorPicker() {
            $(".tool-bar.color-picker").css("opacity", 0);
            isColorPickerOpen = false;
        }

        $("#brushColorPickerBtn")
            .on("click",
                function() {
                    if (isColorPickerOpen) {
                        hideColorPicker();
                        return;
                    }

                    showColorPicker();
                });

        $("#penBtn")
            .on("click",
                function() {
                    hideColorPicker();
                    isEraserSelected = false;
                    $(this).hide();
                    $("#eraserBtn").show();

                });

        $("#eraserBtn")
            .on("click",
                function() {
                    hideColorPicker();
                    isEraserSelected = true;
                    $(this).hide();
                    $("#penBtn").show();
                });

        $(".color-item")
            .on("click",
                function() {
                    selectedColor = $(this).css("background-color");

                    $(".fa.fa-paint-brush").css("color", selectedColor);
                    $(".tool-bar.color-picker").css("opacity", 0);
                    isColorPickerOpen = false;
                });

        $("#endChatBtn")
            .on("click",
                function () {
                    $("#confirmStopChatModal")
                        .modal("show")
                        .find(".btn-danger")
                        .on("click",
                            function() {
                                if (isPresenter) {
                                    recording = false;
                                    canvasStreaming.stop();
                                    audioStreaming.stop();
                                }

                                var data = {
                                    chatId,
                                    duration
                                };

                                $.post("/VideoChat/EndCall/", data);
                                endChat();
                            });
                });

        (function() {
            var canvas = document.querySelector("canvas");
            var canvasContext = canvas.getContext("2d");
            var videoSourceId = videoSelect.val();
            var constraints = {
                video: {
                    optional: [{ sourceId: videoSourceId }]
                },
                audio: true
            };

            navigator.getUserMedia(constraints,
                function(stream) {
                    var streamUrl = window.URL.createObjectURL(stream);
                    var webSocketUri = (document.location.hostname === "localhost" ? "ws://" : "wss://") +
                        window.location.hostname +
                        ":50859/VideoChat/InitVideoChat/" +
                        chatId;

                    myMediaStream = stream;
                    console.log("Got local media stream ? " + (myMediaStream ? "Yes" : "No"));
                    canvas.width = 2 * Math.round(canvas.clientWidth / 2);
                    canvas.height = 2 * Math.round(canvas.clientHeight / 2);

                    canvasContext.save();
                    canvasContext.fillStyle = $("canvas").css("background-color");
                    canvasContext.fillRect(0, 0, canvas.width, canvas.height);
                    canvasContext.restore();

                    localVideoElement.src = streamUrl;
                    localVideoElement.autoplay = "autoplay";

                    whiteBoard = new WhiteBoard(canvas, canvasContext);
                    audioStreaming = new AudioStreaming(webSocketUri, stream, null);
                    canvasStreaming = new CanvasStreaming(canvas,
                        canvasContext,
                        localVideoElement,
                        remoteVideoElement,
                        webSocketUri);
                    canvasStreaming.startPreview();

                    myConnection = createConnection();
                }, function (error) {
                    var modalErrorMsg = "Nenhum dispositivo de aúdio ou viídeo foi detectado em sua máquina.\n" +
                        "Você precisa de um destes dispositivos para realizar uma videoconferência.";

                    noDeviceErrorModal.on("hidden.bs.modal",
                        function() {
                            var data = {
                                chatId,
                                error:
                                    "O outro participante não consegui iniciar a videoconferência por não ter nenhum dispositivo de aúdio ou vídeo."
                            };

                            $.post("/VideoChat/EndCall/",
                                data,
                                function() {
                                    window.location.href = "/";
                                });
                        });

                    noDeviceErrorModal
                        .modal("show")
                        .find(".modal-body")
                        .html("<h6>" + modalErrorMsg + "</h6>");


                    console.log(JSON.stringify(error));
                }
            );

            if (isPresenter) {
                startRecording = function() {
                    console.log("Started recording...");
                    recording = true;
                    audioStreaming.record();
                    canvasStreaming.record();
                };
                stopRecording = function() {
                    recording = false;
                    canvasStreaming.stop();
                    audioStreaming.stop();
                };
            }
            
            timerInterval = setInterval(function() {
                    $("#timer").text(formatTime(duration));
                    duration++;
                }, 1000);
        })();

        function WhiteBoard(canvas, canvasContext) {
            var flag = false;
            var prevX;
            var currX;
            var prevY;
            var currY;
            var dotFlag = false;
            var lineWidht = 3;
            var whiteBoardImage = new Image();
            var whiteBoardYPos;
            var minY;
            var maxY;
            var minX;
            var maxX;

            canvas.addEventListener("mousemove",
                function(e) {
                    findxy("move", e);
                },
                false);
            canvas.addEventListener("mousedown",
                function(e) {
                    findxy("down", e);
                },
                false);
            canvas.addEventListener("mouseup",
                function(e) {
                    findxy("up", e);
                },
                false);
            canvas.addEventListener("mouseout",
                function(e) {
                    findxy("out", e);
                },
                false);

            whiteBoardImage.onload = function() {
                var ch = canvas.height;
                var cw = canvas.width;
                var imageWidth = cw * 0.65;
                var imageHeight = ch * 0.85;

                whiteBoardYPos = ch * 0.1;
                minY = whiteBoardYPos + 43;
                maxY = whiteBoardYPos + imageHeight - 62;
                minX = 45;
                maxX = imageWidth - 49;

                canvasContext.save();
                canvasContext.shadowColor = "black";
                canvasContext.shadowBlur = 10;
                canvasContext.shadowOffsetX = 0;
                canvasContext.shadowOffsetY = 1;
                canvasContext.drawImage(whiteBoardImage, 0, whiteBoardYPos, imageWidth, imageHeight);
                canvasContext.restore();
            };
            whiteBoardImage.src = "../../Content/Images/white-board.svg";

            updateDrawingCallback = function(data) {
                var drawing = JSON.parse(data);
                var oldColor = drawing.color;

                selectedColor = oldColor;
                currX = drawing.endX;
                currY = drawing.endY;

                if (drawing.isPoint) {
                    drawPoint();
                    return;
                }

                var isEraserSelectedBefore = isEraserSelected;

                prevX = drawing.startX;
                prevY = drawing.startY;
                isEraserSelected = drawing.isEreaser;
                draw(false);
                selectedColor = oldColor;
                isEraserSelected = isEraserSelectedBefore;
            };

            function sendDrawing(isPoint) {
                var drawing = JSON.stringify({
                    startX: prevX,
                    endX: currX,
                    startY: prevY,
                    endY: currY,
                    isPoint,
                    isEreaser: isEraserSelected,
                    color: selectedColor
                });

                hub.server.propagateDrawing(chatId, drawing);
            }

            function draw(send) {
                canvasContext.closePath();
                canvasContext.beginPath();

                if (isEraserSelected) {
                    canvasContext.fillStyle = "white";
                    canvasContext.rect(currX, currY, ERASER_SIZE, ERASER_SIZE);
                    canvasContext.fill();
                } else {
                    canvasContext.moveTo(prevX, prevY);
                    canvasContext.lineTo(currX, currY);
                    canvasContext.strokeStyle = selectedColor;
                    canvasContext.lineWidth = lineWidht;
                    canvasContext.stroke();
                }

                canvasContext.closePath();

                if (send) {
                    sendDrawing(false);
                }
            }

            function drawPoint() {
                canvasContext.beginPath();

                if (isEraserSelected) {
                    canvasContext.fillStyle = "white";
                    canvasContext.rect(currX, currY, ERASER_SIZE, ERASER_SIZE);
                    canvasContext.fill();
                } else {
                    canvasContext.fillStyle = selectedColor;
                    canvasContext.fillRect(currX, currY, 3, 3);
                }

                canvasContext.closePath();
            }

            function findxy(res, e) {

                if (res === "down") {
                    prevX = currX;
                    prevY = currY;
                    currX = e.clientX - canvas.offsetLeft;
                    currY = e.clientY - canvas.offsetTop;

                    flag = true;
                    dotFlag = true;

                    if (dotFlag) {
                        if (currY < minY ||
                            currY > maxY ||
                            currX < minX ||
                            currX > maxX) {
                            return;
                        }

                        drawPoint();
                        dotFlag = false;

                        if (prevX && prevY) {
                            sendDrawing(true);
                        }
                    }
                }

                if (res === "up" || res === "out") {
                    if (res === "up") {
                        isDrawing = false;
                    }

                    flag = false;
                }

                if (res === "move") {
                    if (flag) {
                        isDrawing = true;
                        prevX = currX;
                        prevY = currY;
                        currX = e.clientX - canvas.offsetLeft;
                        currY = e.clientY - canvas.offsetTop;

                        if (currY < minY ||
                            currY > maxY ||
                            currX < minX ||
                            currX > maxX) {
                            return;
                        }

                        draw(true);
                    }
                }
            }
        }

        function xhr(url, data) {
            var request = new XMLHttpRequest();

            request.open("POST", url);
            request.send(data);
        }

        function AudioStreaming(webSocketUri, localAudioStream, remoteAudioStream) {
            var isRecording = false;
            var intervalHandler;
            var fileType = "audio";
            var localRecorder = new StereoAudioRecorder(localAudioStream, {
                sampleRate: 48000,
                bufferSize: 4096
            });

            function sendAudioBlob() {
                var fileName = Date.now() + ".wav";
                var formData = new FormData();
                var blob = localRecorder.blob;

                formData.append("chatId", chatId);
                formData.append("frameName", fileName);
                formData.append(fileType + "-blob", blob);

                xhr("/VideoChat/PostAudioFrame", formData);
            }
            
            this.record = function () {
                if (isRecording) {
                    return;
                }

                localRecorder.record();
                isRecording = true;

                intervalHandler = setInterval(function() {
                    localRecorder.stop(function () {
                        localRecorder.record();
                        sendAudioBlob();
                    });
                }, 2000);
            };

            this.stop = function () {
                if (!isRecording) {
                    return;
                }

                clearInterval(intervalHandler);
                localRecorder.stop(sendAudioBlob);
                isRecording = false;
            };
        }

        function CanvasStreaming(canvas, canvasContext, localVideo, remoteVideo, webSocketUri) {
            var recorder = new CanvasRecorder(canvas);
            var isRecording;
            var drawInterval;
            var interval = 1000 / 25;
            var cw = canvas.width;
            var ch = canvas.height;
            var localVideoWidth = cw / 6;
            var localVideoHeight = ch / 4.4;
            var remoteVideoWidth = cw / 3;
            var remoteVideoHeight = ch / 2.2;
            var remoteLeftMargin = cw * 0.35;
            var bottomMargin = ch - (ch * 0.4);
            var remoteYPos = ch * 0.125;
            var localXPos = (cw - remoteLeftMargin) + ((remoteVideoWidth - localVideoWidth) / 2);
            var fileType = "video";
            var recordInterval;

            canvasContext.save();
            canvasContext.shadowColor = "black";
            canvasContext.shadowBlur = 10;
            canvasContext.shadowOffsetX = 0;
            canvasContext.shadowOffsetY = 1;

            canvasContext.beginPath();
            canvasContext.rect(localXPos, bottomMargin + 1, localVideoWidth, localVideoHeight - 2);
            canvasContext.fill();
            canvasContext.closePath();

            canvasContext.beginPath();
            canvasContext.rect(cw - remoteLeftMargin, remoteYPos + 1, remoteVideoWidth, remoteVideoHeight - 2);
            canvasContext.fill();
            canvasContext.closePath();
            canvasContext.restore();

            this.startPreview = function() {
                drawInterval = setInterval(function() {
                        canvasContext.drawImage(
                            localVideo,
                            localXPos,
                            bottomMargin,
                            localVideoWidth,
                            localVideoHeight);

                        canvasContext.drawImage(
                            remoteVideo,
                            cw - remoteLeftMargin,
                            remoteYPos,
                            remoteVideoWidth,
                            remoteVideoHeight);
                    }, interval);
            };

            function sendVideoBlob(blob) {
                var fileName = Date.now() + ".webm";
                var formData = new FormData();

                formData.append("chatId", chatId);
                formData.append("frameName", fileName);
                formData.append(fileType + "-blob", blob);

                xhr("/VideoChat/PostVideoFrame", formData);
            }

            this.record = function () {
                recorder.record();

                recordInterval = setInterval(function () {
                    recorder.stop(function (blob) {
                        recorder.record();
                        sendVideoBlob(blob);
                    });
                }, 2000);

                isRecording = true;
            };

            this.stop = function() {
                isRecording = false;
                
                clearInterval(drawInterval);
                clearInterval(recordInterval);
                recorder.stop(sendVideoBlob);
            };
        }
    }
});