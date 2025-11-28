using UnityEngine;
using Photon.Pun;

// I give up writing comments lmao
public class TrashGrabberEquipment : MonoBehaviourPun
{
    [Header("Trash Grabber Settings")]
    public Transform trashGrabPos;
    private Trash currentTrash;
    private bool isGrabbing;

    [Header("Grab Settings")]
    public LayerMask trashGrabMask;
    public Transform grabOrigin;
    public float grabRadius = 0.5f;
    public float grabDistance = 1f;

    void Start()
    {
        isGrabbing = false;
        currentTrash = null;

        if (grabOrigin == null) grabOrigin = transform;
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        if (currentTrash == null || currentTrash.gameObject == null)
        {
            currentTrash = null;
            isGrabbing = false;
        }

        // Left click: grab
        if (Input.GetMouseButtonDown(0) && !isGrabbing)
        {
            TryGrab();
        }

        // Right click: drop
        if (Input.GetMouseButtonDown(1) && isGrabbing)
        {
            TryDrop();
        }
    }

    private void TryGrab()
    {
        Vector3 center = grabOrigin.position + grabOrigin.forward * grabDistance / 2 + Vector3.up * 0.3f;

        Collider[] colliders = Physics.OverlapSphere(center, grabRadius, trashGrabMask, QueryTriggerInteraction.Ignore);
        Trash closestTrash = null;
        float closestDist = Mathf.Infinity;

        foreach (var col in colliders)
        {
            if (col.TryGetComponent(out Trash trash))
            {
                if (!trash.gameObject.activeInHierarchy) continue;

                float dist = Vector3.Distance(grabOrigin.position, trash.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestTrash = trash;
                }
            }
        }

        if (closestTrash != null)
        {
            if (closestTrash.photonView != null && !closestTrash.photonView.IsMine)
                closestTrash.photonView.RequestOwnership();

            closestTrash.TrashGrab(trashGrabPos);

            Collider col = closestTrash.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            if (closestTrash.photonView != null)
                closestTrash.photonView.RPC("RPC_TrashGrab", RpcTarget.OthersBuffered, photonView.ViewID);

            currentTrash = closestTrash;
            isGrabbing = true;

            Debug.Log("Grabbed trash: " + closestTrash.name);
        }
    }

    private void TryDrop()
    {
        if (currentTrash == null || currentTrash.gameObject == null)
        {
            currentTrash = null;
            isGrabbing = false;
            return;
        }

        currentTrash.TrashDrop();

        Collider col = currentTrash.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        if (currentTrash.photonView != null)
            currentTrash.photonView.RPC("RPC_TrashDrop", RpcTarget.OthersBuffered, photonView.ViewID);

        currentTrash = null;
        isGrabbing = false;
    }

    private void OnDrawGizmos()
    {
        if (grabOrigin != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 center = grabOrigin.position + grabOrigin.forward * grabDistance / 2 + Vector3.up * 0.3f;
            Gizmos.DrawWireSphere(center, grabRadius);
        }
    }
}
