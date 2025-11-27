using UnityEngine;
using Photon.Pun;

public class VacuumEquipment : MonoBehaviourPun
{
    [Header("Vacuum Settings")]
    public float suctionPower = 0.1f;

    [Header("Sphere Settings")]
    public LayerMask vacuumMask;
    public Transform spherePos;
    public float sphereSize = 0.5f;
    public float suctionDistance = 1f;

    private void Update()
    {
        // only the local player controls their vacuum
        if (!photonView.IsMine) return;

        if (Input.GetMouseButton(0))
        {
            // detect all dirt within suction area
            Collider[] hits = Physics.OverlapSphere(
                spherePos.position + transform.forward * suctionDistance,
                sphereSize,
                vacuumMask
            );

            foreach (Collider col in hits)
            {
                if (col.TryGetComponent(out Dirt dirt))
                {
                    if (dirt.photonView != null)
                    {
                        if (dirt.photonView.IsMine)
                        {
                            // owner cleans locally
                            dirt.CleanUp(suctionPower);
                        }
                        else
                        {
                            // request owner to clean via RPC
                            dirt.photonView.RPC("RPC_CleanUp", dirt.photonView.Owner, suctionPower);
                        }
                    }
                    else
                    {
                        // non-networked dirt (fall back if everything above fails)
                        dirt.CleanUp(suctionPower);
                    }
                }
            }
        }
    }

    // visualize the vacuum area in editor
    private void OnDrawGizmos()
    {
        if (spherePos != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(spherePos.position + transform.forward * suctionDistance, sphereSize);
        }
    }
}
