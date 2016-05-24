#OSC Controller

なかたさんの、[unity-osc](https://github.com/nobnak/unity-osc)を、自分で使うようにOscControllerクラスを作った。

OSCのpath毎に、UnityEventを設定できるようにした。

##Usage

- OscControllerコンポーネントをセット
- localPortで、受けのポートをセット
- defaultRemoteHostは、Sendするホスト名(IP)
- defaultRemotPostは、Sendするポート名(Sendする場合は設定)
- Sendするときに、IP/Portを指定して送ることも可能。(OscController.oscEvents)

###Send OscMessage

OscSender.cs is example code.

```csharp
var osc = new MessageEncoder(path);
osc.Add(val);
osc.Add(val);
OscController.Instance.Send(osc);
//OscController.Instance.Send(osc, remote, port);
```