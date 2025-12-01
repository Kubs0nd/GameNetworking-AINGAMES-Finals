using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
public class GrabSync : MonoBehaviourPun
{
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    [PunRPC]
    public void SyncPosition(Vector3 pos, Quaternion rot)
    {
        if (rb == null) return;
        rb.MovePosition(pos);
        rb.MoveRotation(rot);
    }
}
