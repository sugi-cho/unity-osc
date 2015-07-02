unity-osc
=========
OSC Encoder/Decoder for Unity

# Usage
## Set Up a Server
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

## Set Up a Client
```C#
private OscClient _client;

void Start () {
	var address = Dns.GetHostAddresses(remoteHost)[0];
	var serverEndpoint = new IPEndPoint(address, remotePort);
	_client = new OscClient(serverEndpoint);
	_client.OnError += delegate(System.Exception obj) {
		Debug.LogError(obj);
	};
}
void OnDestroy() {
	if (_client != null)
		_client.Dispose();
}
```

## Read Messages
```C#
foreach (var cap in _server)
	ProcessMessage(cap.message);
```

## Send a Message
```C#
var oscEnc = new MessageEncoder("/path");
oscEnc.Add(3.14f);
oscEnc.Add(12345);
var dest = new IPEndPoint("Client IP Address", "Client Port Number");
_server.Send(oscEnc.Encode(), dest);
```
