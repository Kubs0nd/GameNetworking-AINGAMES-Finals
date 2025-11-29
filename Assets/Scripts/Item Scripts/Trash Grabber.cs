using UnityEngine;
using Photon.Pun;

public class TrashGrabber : MonoBehaviourPun
{
    public Transform grabPoint;
    public LayerMask trashMask;
    public float grabRadius = 0.5f;
    public float grabDistance = 1f;

    private NetworkInteractable held;

    void Update()
    {
        if (!photonView.IsMine) return; // Only local player can grab/drop

        if (Input.GetMouseButtonDown(0) && held == null) TryGrab();
        if (Input.GetMouseButtonDown(1) && held != null) DropHeld();
    }

    void TryGrab()
    {
        Vector3 center = transform.position + transform.forward * grabDistance / 2 + Vector3.up * 0.3f;
        Collider[] hits = Physics.OverlapSphere(center, grabRadius, trashMask, QueryTriggerInteraction.Ignore);

        NetworkInteractable closest = null;
        float closestDist = Mathf.Infinity;
        foreach (var col in hits)
        {
            var ni = col.GetComponent<NetworkInteractable>();
            if (ni == null || ni.IsBeingHeld) continue;
            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = ni;
            }
        }

        if (closest != null)
        {
            closest.RequestGrab(grabPoint);
            held = closest;
        }
    }

    void DropHeld()
    {
        if (held == null) return;
        held.Drop();
        held = null;
    }

    private void OnDrawGizmos()
    {
        if (grabPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(grabPoint.position, grabRadius);
        }
    }
}
