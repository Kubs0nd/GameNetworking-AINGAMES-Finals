using UnityEngine;
using Photon.Pun;
using UnityEngine.Rendering;

// uses ItemEquipable
public class VacuumEquipment : MonoBehaviour
{
    public float suctionPower = 0.5f;
    public LayerMask vacuumMask;
    public Transform spherePos;
    public float sphereSize = 0.5f;
    public float suctionDistance = 1f;

    void Update()
    {
        if (!Input.GetMouseButton(0)) return;

        Vector3 center = spherePos.position + transform.forward * suctionDistance;
        Collider[] hits = Physics.OverlapSphere(center, sphereSize, vacuumMask, QueryTriggerInteraction.Ignore);

        foreach (var col in hits)
        {
            var dirt = col.GetComponent<Dirt>();
            if (dirt == null) continue;

            if (dirt.photonView != null)
            {
                dirt.photonView.RPC("RPC_CleanUp", RpcTarget.All, suctionPower);
            }

        }
    }

    private void OnDrawGizmos()
    {
        if (spherePos != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(spherePos.position + transform.forward * 0, sphereSize);
        }
    }
}
