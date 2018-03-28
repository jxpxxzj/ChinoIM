var ChinoClient = require('./ChinoClient');
var client = new ChinoClient();

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