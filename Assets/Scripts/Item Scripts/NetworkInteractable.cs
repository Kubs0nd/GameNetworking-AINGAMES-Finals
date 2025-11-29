using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

[RequireComponent(typeof(PhotonView), typeof(Rigidbody))]
public class NetworkInteractable : MonoBehaviourPun, IPunObservable, IPunOwnershipCallbacks
{
    public bool IsBeingHeld => isBeingHeld;
    public bool IsPendingGrab => pendingGrab;

    protected Rigidbody rb;
    protected float followSpeed = 20f;

    protected Vector3 networkPosition;
    protected Quaternion networkRotation;
    protected Vector3 networkLinearVelocity;
    protected Vector3 networkAngularVelocity;

    protected bool isBeingHeld = false;
    protected bool pendingGrab = false;
    protected int requestedByActor = -1;
    protected int requestedGrabPointViewId = -1;
    protected Transform followTransform;

    const float pendingTimeout = 3f;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        PhotonNetwork.AddCallbackTarget(this);
    }

    protected virtual void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    protected virtual void FixedUpdate()
    {
        if (rb == null) return;

        if (isBeingHeld)
        {
            if (photonView.IsMine && followTransform != null)
            {
                // Drive the rigidbody smoothly to follow the target
                rb.MovePosition(Vector3.Lerp(rb.position, followTransform.position, followSpeed * Time.fixedDeltaTime));
                rb.MoveRotation(Quaternion.Lerp(rb.rotation, followTransform.rotation, followSpeed * Time.fixedDeltaTime));

                rb.linearVelocity = Vector3.zero;
            }
            else if (!photonView.IsMine)
            {
                // Remote clients follow networked state
                rb.MovePosition(Vector3.Lerp(rb.position, networkPosition, followSpeed * Time.fixedDeltaTime));
                rb.MoveRotation(Quaternion.Lerp(rb.rotation, networkRotation, followSpeed * Time.fixedDeltaTime));
                rb.linearVelocity = networkLinearVelocity;
            }
        }
        else if (!photonView.IsMine)
        {
            // Not held and remote — apply networked position/rotation
            rb.MovePosition(Vector3.Lerp(rb.position, networkPosition, followSpeed * Time.fixedDeltaTime));
            rb.MoveRotation(Quaternion.Lerp(rb.rotation, networkRotation, followSpeed * Time.fixedDeltaTime));
            rb.linearVelocity = networkLinearVelocity;
            rb.angularVelocity = networkAngularVelocity;
        }
    }

    /// <summary>
    /// Call to request grabbing this object
    /// </summary>
    public virtual void RequestGrab(Transform grabPoint)
    {
        if (grabPoint == null) return;

        PhotonView gpPv = grabPoint.GetComponentInParent<PhotonView>();
        if (gpPv == null)
        {
            Debug.LogWarning("RequestGrab: grabPoint has no PhotonView in parent.");
            return;
        }

        if (photonView.IsMine)
        {
            // Local grab directly
            CompleteGrab(grabPoint);
            return;
        }

        // Already pending for this actor? skip
        if (pendingGrab && requestedByActor == PhotonNetwork.LocalPlayer.ActorNumber) return;

        pendingGrab = true;
        requestedByActor = PhotonNetwork.LocalPlayer.ActorNumber;
        requestedGrabPointViewId = gpPv.ViewID;

        photonView.RPC(nameof(RPC_RequestGrab_Master), RpcTarget.MasterClient, requestedByActor, requestedGrabPointViewId);

        StartCoroutine(ClearPendingTimeout());
    }

    IEnumerator ClearPendingTimeout()
    {
        float t = 0f;
        while (pendingGrab && t < pendingTimeout)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (pendingGrab)
        {
            pendingGrab = false;
            requestedByActor = -1;
            requestedGrabPointViewId = -1;
        }
    }

    void CompleteGrab(Transform grabPoint)
    {
        followTransform = grabPoint;
        isBeingHeld = true;
        pendingGrab = false;
        requestedByActor = -1;
        requestedGrabPointViewId = -1;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearDamping = 10f;
            rb.angularDamping = 10f;
            Collider c = GetComponent<Collider>();
            if (c) c.enabled = false;
        }

        photonView.RPC(nameof(RPC_GrabNotifyOthers), RpcTarget.OthersBuffered);
    }

    public void Drop()
    {
        if (!photonView.IsMine)
        {
            photonView.RPC(nameof(RPC_RequestDrop_Master), RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
            return;
        }

        DoDropLocal();
        photonView.RPC(nameof(RPC_NotifyDrop_All), RpcTarget.OthersBuffered);
    }

    protected void DoDropLocal()
    {
        isBeingHeld = false;
        pendingGrab = false;
        requestedByActor = -1;
        requestedGrabPointViewId = -1;
        followTransform = null;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearDamping = 0.05f;
            rb.angularDamping = 0.05f;
            Collider c = GetComponent<Collider>();
            if (c) c.enabled = true;
        }
    }

    [PunRPC]
    void RPC_GrabNotifyOthers()
    {
        if (photonView.IsMine) return; // owner already updated
        isBeingHeld = true;
        Collider c = GetComponent<Collider>();
        if (c) c.enabled = false;
    }

    [PunRPC]
    void RPC_RequestGrab_Master(int requestingActor, int grabPointViewId)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Player requester = PhotonNetwork.CurrentRoom.GetPlayer(requestingActor);
        if (requester == null) return;

        photonView.TransferOwnership(requester);

        photonView.RPC(nameof(RPC_ApproveGrab_All), RpcTarget.AllBuffered, requestingActor, grabPointViewId);
    }

    [PunRPC]
    void RPC_ApproveGrab_All(int requestingActor, int grabPointViewId)
    {
        pendingGrab = false;
        requestedByActor = -1;
        requestedGrabPointViewId = -1;

        PhotonView gpPv = PhotonView.Find(grabPointViewId);
        followTransform = gpPv != null ? gpPv.transform : null;

        isBeingHeld = true;

        if (rb != null)
        {
            rb.isKinematic = true;
            Collider c = GetComponent<Collider>();
            if (c) c.enabled = false;
        }
    }

    [PunRPC]
    void RPC_RequestDrop_Master(int requestingActor)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        photonView.RPC(nameof(RPC_NotifyDrop_All), RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPC_NotifyDrop_All()
    {
        DoDropLocal();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (rb == null) return;

        if (stream.IsWriting)
        {
            stream.SendNext(rb.position);
            stream.SendNext(rb.rotation);
            stream.SendNext(rb.linearVelocity);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            networkLinearVelocity = (Vector3)stream.ReceiveNext();
        }
    }

    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    {
        if (targetView != photonView) return;
        targetView.TransferOwnership(requestingPlayer);
    }

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {
        // nothing additional required; master already broadcasted
    }

    public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
    {
        if (targetView != photonView) return;
        pendingGrab = false;
        requestedByActor = -1;
        requestedGrabPointViewId = -1;
    }
}
