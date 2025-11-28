using UnityEngine;
using Photon.Pun;

public class Trash : MonoBehaviourPun, IPunObservable
{
    private Rigidbody rb;
    private Transform trashGrabPointTransform; // set only on owner when grabbed

    // network interpolation
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private Vector3 networkVelocity;
    private Vector3 networkAngularVelocity;
    private float followSpeed = 20f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        // keep interpolation for smoother movement
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    // called when the owner grabs the item
    public void TrashGrab(Transform itemGrabPointTransform)
    {
        if (itemGrabPointTransform == null) return;

        trashGrabPointTransform = itemGrabPointTransform;

        // request ownership if not already owner
        if (photonView != null && !photonView.IsMine)
            photonView.RequestOwnership();

        // change layer locally (others will be updated by the RPC)
        gameObject.layer = LayerMask.NameToLayer("Grabbed");

        // make physics stable while being held
        // Use properties compatible with most Unity versions (drag/constraints)
        rb.linearDamping = 10f;
        rb.angularDamping = 10f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = false;

        // zero out velocities so it snaps smoothly
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    // called when the owner drops the item
    public void TrashDrop()
    {
        trashGrabPointTransform = null;

        gameObject.layer = LayerMask.NameToLayer("Grabable Item");

        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;
    }

    // RPC called by owner to inform non-owners the object was grabbed
    [PunRPC]
    void RPC_TrashGrab(int playerViewID)
    {
        // non-owners only need to update visible state (layer/physics)
        if (photonView.IsMine) return;

        gameObject.layer = LayerMask.NameToLayer("Grabbed");

        rb.linearDamping = 10f;
        rb.angularDamping = 10f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    // RPC called by owner to inform non-owners the object was dropped
    [PunRPC]
    void RPC_TrashDrop(int playerViewID)
    {
        if (photonView.IsMine) return;

        trashGrabPointTransform = null;

        gameObject.layer = LayerMask.NameToLayer("Grabable Item");

        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;
    }

    // Physics-driven follow for the owner
    private void FixedUpdate()
    {
        if (trashGrabPointTransform != null && photonView.IsMine)
        {
            // Smoothly move towards grab point using MovePosition/MoveRotation
            //Vector3 targetPos = trashGrabPointTransform.position;
            //Quaternion targetRot = trashGrabPointTransform.rotation;

            // compute smoothed values
            //Vector3 newPos = Vector3.Lerp(rb.position, targetPos, followSpeed * Time.fixedDeltaTime);
            //Quaternion newRot = Quaternion.Lerp(rb.rotation, targetRot, followSpeed * Time.fixedDeltaTime);

            rb.MovePosition(trashGrabPointTransform.position);
            rb.MoveRotation(trashGrabPointTransform.rotation);

            // zero velocities to avoid physics glitches
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        else if (!photonView.IsMine)
        {
            // Smoothly interpolate network position and rotation
            rb.MovePosition(Vector3.Lerp(rb.position, networkPosition, followSpeed * Time.fixedDeltaTime));
            rb.MoveRotation(Quaternion.Lerp(rb.rotation, networkRotation, followSpeed * Time.fixedDeltaTime));

            // apply last known velocities from the network for realism
            rb.linearVelocity = networkVelocity;
            rb.angularVelocity = networkAngularVelocity;
        }
    }

    // Photon sync
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // owner sends full Rigidbody state
            stream.SendNext(rb.position);
            stream.SendNext(rb.rotation);
            stream.SendNext(rb.linearVelocity);
            stream.SendNext(rb.angularVelocity);
        }
        else
        {
            // non-owners receive networked state
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            networkVelocity = (Vector3)stream.ReceiveNext();
            networkAngularVelocity = (Vector3)stream.ReceiveNext();
        }
    }
}
