var net = require('net');
var events = require('events');
var {
    promisify
} = require('util');
var bcrypt = require('bcryptjs');

var ServerAddressV4 = "127.0.0.1";
var ServerAddressV6 = "::";
var Port = 6163;
var TimeoutSeconds = 90;

function GetCurrentTime() {
    return Math.floor(new Date() / 1000);
}

function BcryptHash(str) {
    var salt = bcrypt.genSaltSync();
    var hash = bcrypt.hashSync(str, salt);
    return hash;
}

function BcryptVerify(str, hash) {
    return bcrypt.compareSync(str, hash);
}

function GetRequestToken(request) {
    var str = JSON.stringify(request.Payload) + request.SendTime;
    return BcryptHash(str);
}

class ChinoClient {
    constructor(serverV4 = ServerAddressV4, serverV6 = ServerAddressV6, port = Port) {
        this.socket = new net.Socket();
        this.lastReceiveTime = 0;
        this.isConnected = false;
        this.isAuth = false;
        this.events = new events.EventEmitter();

        console.log("Connecting...")
        this.Connect(serverV4, serverV6, port)
    }

    Connect(serverV4, serverV6, port) {
        // no support for v6 now
        this.socket.connect(port, serverV4, () => {
            console.log(`Connected to ${serverV4}:${port}`);
            this.isConnected = true;
            this.socket.on('data', (data) => {
                var obj = JSON.parse(data);
                console.log('Receive: ', obj);
                if (obj !== null) {
                    this.handleIncoming(obj);
                }
            });
            this.checkTimeout();
            this.events.emit('connected');
        });
    }

    checkTimeout() {
        this.timeoutId = setInterval(() => {
            var current = GetCurrentTime();
            var timeout = current - this.lastReceiveTime;
            if (timeout > TimeoutSeconds) {
                this.Disconnect('Timed out');
                clearInterval(this.timeoutId)
            }
        }, 5000);
    }

    Disconnect() {
        console.log('Disconnect');
        this.events.emit('disconnect');
        this.disconnect();
    }

    Disconnect(reason) {
        console.warn(`Disconneted for ${reason}`);
        this.events.emit('disconnect', reason);
        this.disconnect();
    }

    disconnect() {
        this.sendRequest('User_Logout', null);
        this.isConnected = false;
        this.socket.destroy();
        
    }

    send(request) {
        request.SendTime = GetCurrentTime();
        var token = GetRequestToken(request);
        request.Token = token;
        console.log('Send:', request);
        var json = JSON.stringify(request);
        if (json !== '') {
            this.socket.write(json);
        }
    }

    pong() {
        this.sendRequest('Pong', null);
    }

    handleIncoming(request) {
        this.lastReceiveTime = GetCurrentTime();
        if (!this.isAuth && request.Type != 'User_LoginResult') {
            console.warn("Client not login, ignore request");
            return;
        }

        switch (request.Type) {
            case 'Ping':
                this.pong();
                break;
            case 'User_LoginResult':
                if (request.Payload.result > 0) {
                    console.log("Login success");
                    this.isAuth = true;
                }
                break;
        }

        if (request.Type !== 'Pong') {
            this.events.emit('receive', request);
        }
    }

    sendRequest(type, payload) {
        var request = 
        {
            Payload: payload,
            Type: type
        };
        this.SendRequest(request);
    }

    SendRequest(request) {
        if ((!this.isAuth || !this.isConnected) && request.Type !== 'User_Login' && request.Type != 'User_Register') {
            return;
        }

        if (request !== null) {
            this.send(request);
        }
    }
}

module.exports = ChinoClient;
