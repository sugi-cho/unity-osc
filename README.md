unity-osc
=========
OSC Encoder/Decoder for Unity

# Usage
## Server
```C#
private OscServer _server;

void Start () {
	var serverEndpoint = new IPEndPoint(IPAddress.Any, listenPort);
	_server = new OscServer(serverEndpoint);
	_server.OnError += delegate(System.Exception obj) {
		Debug.Log(obj);
	};	
}

void OnDestroy() {
	if (_server != null)
		_server.Dispose();
}
```

### read Messages
```C#
foreach (var cap in _server)
	ProcessMessage(cap.message);
```

### Send a Message
```C#
var oscEnc = new MessageEncoder("/path");
oscEnc.Add(3.14f);
oscEnc.Add(12345);
//var dest = new IPEndPoint("Client IP Address", "Client Port Number");
_server.Send(oscEnc.Encode(), dest);
```
