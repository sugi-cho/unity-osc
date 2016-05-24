using UnityEngine;
using System.Collections;
using Osc;

public class ObjectEmitter : MonoBehaviour
{

    public void OnOscPoint(object[] data)
    {
        if (data.Length < 2)
            return;
        var pos = new Vector3((float)data[0], (float)data[1], 10f);
        pos = Camera.main.ScreenToWorldPoint(pos);

        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = pos + Random.insideUnitSphere;
        go.transform.parent = transform;
        go.AddComponent<Rigidbody>().AddTorque(Random.onUnitSphere * 100f);
        Destroy(go, 5f);
    }
    public void OnOscXYPad(object[] data)
    {
        if (data.Length < 2)
            return;
        var pos = new Vector3((float)data[1], (float)data[0], 10f);
        pos = Camera.main.ViewportToWorldPoint(pos);

        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = pos + Random.insideUnitSphere;
        go.transform.parent = transform;
        go.AddComponent<Rigidbody>().AddTorque(Random.onUnitSphere * 100f);
        Destroy(go, 5f);
    }
}
