var ws;
var wsReady = false;

self.onmessage = function(e) {
    switch (e.data.command) {
    case "init":
        self.ws = new WebSocket(e.data.uri);
        self.ws.onopen = self.wsOpen;
        self.ws.onclose = self.wsClosed;
        self.ws.onmessage = self.wsMessage;
        self.chatId = e.data.chatId;
        console.log("INITIALIZED AUDIO WORKER");
        break;
    case "record":
        if (self.ws && self.wsReady) {
            console.log("SENDING FRAME");
            self.ws.send("audio-frame " + self.chatId + " " + e.data.source);
            self.ws.send(e.data.blob);
        }

        break;
    case "stop":
        if (self.ws && self.wsReady) {
            self.ws.send("end " + self.chatId + " " + e.data.duration);
        }
        break;
    }
};

function wsOpen() {
    self.ws.send("start " + self.chatId);
}

function wsClosed() {
    self.wsReady = false;
}

function wsMessage(message) {
    console.log("AUDIO WORKER RECEIVED MESSAGE: " + message.data);

    if (message.data === "ready") {
        self.wsReady = true;
    }
}