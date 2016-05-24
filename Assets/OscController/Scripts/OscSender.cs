using UnityEngine;
using System.Collections;
using Osc;

public class OscSender : MonoBehaviour
{

    public string path = "/point";
    // Update is called once per frame
    void Update()
    {
        if (!Input.GetMouseButton(0))
            return;
        var pos = Input.mousePosition;
        var osc = new MessageEncoder(path);
        osc.Add(pos.x);
        osc.Add(pos.y);
        OscController.Instance.Send(osc, "localhost", 10000);
    }
}
