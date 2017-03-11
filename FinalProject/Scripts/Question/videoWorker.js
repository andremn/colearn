var ws;
var wsReady = false;

self.onmessage = function(e) {
    switch (e.data.command) {
    case "init":
        self.ws = new WebSocket(e.data.url);
        self.ws.onopen = self.wsOpen;
        self.ws.onclose = self.wsClosed;
        self.ws.onmessage = self.wsMessage;
        self.chatId = e.data.chatId;
    case "send":
        if (self.ws && self.wsReady) {
            self.ws.send("video-frame " + self.chatId);
            self.ws.send(e.data.blob);
        }

        break;
    case "stop":
        if (self.ws && self.wsReady) {
            self.ws.send("end " + self.chatId + " " + e.data.duration);
        }
    }
};

function wsOpen() {
    self.ws.send("start " + self.chatId);
}

function wsClosed() {
    self.wsReady = false;
}

function wsMessage(message) {
    if (message.data === "ready") {
        self.wsReady = true;
    }
}