var CocoaClient = require('./CocoaClient');
var client = new CocoaClient();

client.events.on('receive', (request) => {
    // do something
});

setTimeout(() => {
    var request = {
        Type: "User_Login",
        Payload: {
            "UID": "1",
            "Username": "Test",
            "Password": "a"
        }
    };
    
    client.SendRequest(request);
}, 2000);