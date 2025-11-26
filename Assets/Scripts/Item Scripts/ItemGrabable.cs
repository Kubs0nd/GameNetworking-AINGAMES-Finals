using UnityEngine;
using Photon.Pun;

public class ItemGrabable : MonoBehaviourPun, IPunObservable
{
    private Rigidbody rb;
    private Transform itemGrabPointTransform;

    // network interpolation
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private Vector3 networkVelocity;
    private Vector3 networkAngularVelocity;
    private float followSpeed = 20f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    // called when the player grabs the item
    public void Grab(Transform itemGrabPointTransform)
    {
        this.itemGrabPointTransform = itemGrabPointTransform;

        // photon ownership
        if (photonView != null && !photonView.IsMine)
            photonView.RequestOwnership();

        // sets the layer of the object to "Equipped" to prevent other players from taking it from your hands
        gameObject.layer = LayerMask.NameToLayer("Grabbed");

        rb.linearDamping = 5f;
        rb.freezeRotation = true;
        rb.useGravity = false;
    }

    // called when the player drops the item
    public void Drop()
    {
        this.itemGrabPointTransform = null;

        gameObject.layer = LayerMask.NameToLayer("Grabable Item");

        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.freezeRotation = false;
        rb.useGravity = true;
    }

    // RPC called by Interact to inform ALL clients that this item was grabbed
    [PunRPC]
    void RPC_Grab(int playerViewID)
    {
        // if owner, local Grab() already set up the follow transform.
        // non-owners only need to update visible state (layer/physics).
        if (photonView.IsMine) return;

        // sets the layer of the object to "Equipped" to prevent other players from taking it from your hands
        gameObject.layer = LayerMask.NameToLayer("Grabbed");

        rb.linearDamping = 5f;
        rb.freezeRotation = true;
        rb.useGravity = false;
    }

    // RPC called by Interact to inform ALL clients that this item was dropped
    [PunRPC]
    void RPC_Drop()
    {
        if (photonView.IsMine) return;

        this.itemGrabPointTransform = null;

        gameObject.layer = LayerMask.NameToLayer("Grabable Item");

        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.freezeRotation = false;
        rb.useGravity = true;
    }

    private void LateUpdate()
    {
        if (itemGrabPointTransform != null && photonView.IsMine)
        {
            // Smoothly move towards grab point
            Vector3 newPos = Vector3.Lerp(rb.position, itemGrabPointTransform.position, followSpeed * Time.fixedDeltaTime);
            Quaternion newRot = Quaternion.Lerp(rb.rotation, itemGrabPointTransform.rotation, followSpeed * Time.fixedDeltaTime);

            rb.MovePosition(newPos);
            rb.MoveRotation(newRot);
        }
        else if (!photonView.IsMine)
        {
            // smoothly interpolate network position and rotation
            rb.MovePosition(Vector3.Lerp(rb.position, networkPosition, followSpeed * Time.fixedDeltaTime));
            rb.MoveRotation(Quaternion.Lerp(rb.rotation, networkRotation, followSpeed * Time.fixedDeltaTime));

            // apply network velocity to keep physics realistic
            rb.linearVelocity = networkVelocity;
            rb.angularVelocity = networkAngularVelocity;
        }
    }

    // Photon sync
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // send full Rigidbody state
            stream.SendNext(rb.position);
            stream.SendNext(rb.rotation);
            stream.SendNext(rb.linearVelocity);
            stream.SendNext(rb.angularVelocity);
        }
        else
        {
            // receive networked state
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            networkVelocity = (Vector3)stream.ReceiveNext();
            networkAngularVelocity = (Vector3)stream.ReceiveNext();
        }
    }
}
