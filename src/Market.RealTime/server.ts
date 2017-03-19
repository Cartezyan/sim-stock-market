let app = require("express")();
let http = require("http").Server(app);
let io = require("socket.io")(http);
let redis = require("redis");

var port = process.env.port || 1337;
var redisUrl = process.env.redis_url || "redis://localhost";
var bus = redis.createClient(redisUrl);

io.on("connection", function (socket) {
    console.log(`client connected: ${socket}`);
});

bus.on("message", (channel, message) => {
    io.emit(channel, message);
});

bus.subscribe("quote");

app.get("/", function (req, res) {
    res.sendFile(__dirname + "/index.html");
});

http.listen(port, function () {
    console.log(`listening on *:${port}`);
});