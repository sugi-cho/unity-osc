using System.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Linq;
using Osc;
using UnityEngine;
using UnityEngine.Events;

public class OscController : OscPort
{
    public static OscController Instance { get { if (_Instance == null) _Instance = FindObjectOfType<OscController>(); return _Instance; } }
    static OscController _Instance;

    public PathEventPair[] oscEvents;
    Dictionary<string, OscMessageEvent> _oscEventMap;

    Socket _udp;
    byte[] _receiveBuffer;
    Thread _reader;

    public void ShowMessage(Capsule oscCapsule)
    {
        var dataString = "";
        foreach (var o in oscCapsule.message.data)
            dataString += o.ToString() + " ";
        Debug.LogFormat("ip:{0}, path:{1}, data: {2}", oscCapsule.ip, oscCapsule.message.path, dataString);
    }

    protected override void OnEnable()
    {
        _oscEventMap = oscEvents.ToDictionary(b => b.path, b => b.onOsc);
        try
        {
            base.OnEnable();

            _udp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _udp.Bind(new IPEndPoint(IPAddress.Any, localPort));

            _receiveBuffer = new byte[BUFFER_SIZE];

            _reader = new Thread(Reader);
            _reader.Start();
        }
        catch (System.Exception e)
        {
            RaiseError(e);
            enabled = false;
        }
    }
    protected override void OnDisable()
    {
        if (_udp != null)
        {
            _udp.Close();
            _udp = null;
        }
        if (_reader != null)
        {
            _reader.Abort();
            _reader = null;
        }

        base.OnDisable();
    }

    protected override void Update()
    {
        lock (_received)
            while (_received.Count > 0)
            {
                var c = _received.Dequeue();
                OnReceive.Invoke(c);
                if (_oscEventMap.ContainsKey(c.message.path))
                    _oscEventMap[c.message.path].Invoke(c.message.data);
            }
    }
    public void Send(MessageEncoder oscMessage, string address, int port)
    {
        var remote = new IPEndPoint(FindFromHostName(address), port);
        Send(oscMessage.Encode(), remote);
    }
    public override void Send(byte[] oscData, IPEndPoint remote)
    {
        try
        {
            _udp.SendTo(oscData, remote);
        }
        catch (System.Exception e)
        {
            RaiseError(e);
        }
    }

    void Reader()
    {
        while (_udp != null)
        {
            try
            {
                var clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
                var fromendpoint = (EndPoint)clientEndpoint;
                var length = _udp.ReceiveFrom(_receiveBuffer, ref fromendpoint);
                if (length == 0 || (clientEndpoint = fromendpoint as IPEndPoint) == null)
                    continue;

                _oscParser.FeedData(_receiveBuffer, length);
                while (_oscParser.MessageCount > 0)
                {
                    lock (_received)
                    {
                        var msg = _oscParser.PopMessage();
                        Receive(new Capsule(msg, clientEndpoint));
                    }
                }
            }
            catch (Exception e)
            {
                RaiseError(e);
            }
        }
    }

    [Serializable]
    public class OscMessageEvent : UnityEvent<object[]> { }
    [Serializable]
    public struct PathEventPair
    {
        public string path;
        public OscMessageEvent onOsc;
    }

}