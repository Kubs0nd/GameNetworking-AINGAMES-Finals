using UnityEngine;
using Photon.Pun;

public class EquipmentBGrabber : EquipmentBase
{
    [Header("Equipment Settings")]
    public float grabRadius = 1f;
    public Transform holdPoint;
    public LayerMask grabMask;
    public float smoothSpeed = 15f;

    private Rigidbody grabbedRB;
    private PhotonView grabbedView;
    private PhotonView ownerView;
    private PhotonRigidbodyView grabbedPRV;

    private Vector3 networkPosition;
    private Quaternion networkRotation;

    void Start()
    {
        ownerView = GetComponentInParent<PhotonView>();
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (ownerView.IsMine)
        {
            HandleOwnerInput();
        }
        else
        {
            SmoothFollowRemote();
        }
    }

    void HandleOwnerInput()
    {
        if (grabbedRB == null)
        {
            if (Input.GetMouseButtonDown(0))
                TryGrab();
        }
        else
        {
            if (Input.GetMouseButtonUp(0))
                Release();
            else
                HoldObject();
        }
    }

    void TryGrab()
    {
        Collider[] hits = Physics.OverlapSphere(holdPoint.position, grabRadius, grabMask);

        foreach (Collider hit in hits)
        {
            Rigidbody rb = hit.attachedRigidbody;
            if (rb == null) continue;

            PhotonView pv = rb.GetComponent<PhotonView>();
            if (pv == null) continue;

            pv.RequestOwnership();

            grabbedRB = rb;
            grabbedView = pv;

            grabbedPRV = grabbedRB.GetComponent<PhotonRigidbodyView>();
            if (grabbedPRV != null) grabbedPRV.enabled = false;

            grabbedRB.isKinematic = true;

            networkPosition = holdPoint.position;
            networkRotation = holdPoint.rotation;

            ownerView.RPC(nameof(RPC_SyncGrabbedObject), RpcTarget.OthersBuffered,
                grabbedView.ViewID, networkPosition, networkRotation);

            return;
        }
    }

    void HoldObject()
    {
        if (grabbedRB == null || grabbedView == null) return;

        grabbedRB.MovePosition(holdPoint.position);
        grabbedRB.MoveRotation(holdPoint.rotation);

        networkPosition = holdPoint.position;
        networkRotation = holdPoint.rotation;

        ownerView.RPC(nameof(RPC_SyncGrabbedObject), RpcTarget.Others,
            grabbedView.ViewID, networkPosition, networkRotation);
    }

    void Release()
    {
        if (grabbedRB == null || grabbedView == null) return;

        grabbedRB.isKinematic = false;

        if (grabbedPRV != null) grabbedPRV.enabled = true;

        ownerView.RPC(nameof(RPC_SyncRelease), RpcTarget.OthersBuffered, grabbedView.ViewID);

        ClearGrabData();
    }

    void ClearGrabData()
    {
        grabbedRB = null;
        grabbedView = null;
        grabbedPRV = null;
    }

    public override void DisableEquipment()
    {
        base.DisableEquipment();
        Release();
    }

    private void OnDrawGizmos()
    {
        if (!holdPoint) return;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(holdPoint.position, grabRadius);
    }

    void SmoothFollowRemote()
    {
        if (grabbedRB == null) return;

        grabbedRB.position = Vector3.Lerp(grabbedRB.position, networkPosition, Time.fixedDeltaTime * smoothSpeed);
        grabbedRB.rotation = Quaternion.Slerp(grabbedRB.rotation, networkRotation, Time.fixedDeltaTime * smoothSpeed);
    }

    [PunRPC]
    void RPC_SyncGrabbedObject(int targetViewID, Vector3 pos, Quaternion rot)
    {
        PhotonView targetView = PhotonView.Find(targetViewID);
        if (targetView == null) return;

        Rigidbody rb = targetView.GetComponent<Rigidbody>();
        if (rb == null) return;

        networkPosition = pos;
        networkRotation = rot;

        rb.isKinematic = true;

        grabbedRB = rb;
        grabbedView = targetView;
    }

    [PunRPC]
    void RPC_SyncRelease(int targetViewID)
    {
        PhotonView targetView = PhotonView.Find(targetViewID);
        if (targetView == null) return;

        Rigidbody rb = targetView.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = false;

        if (grabbedRB == rb)
        {
            grabbedRB = null;
            grabbedView = null;
        }
    }
}
