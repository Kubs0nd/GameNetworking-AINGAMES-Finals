using UnityEngine;
using Photon.Pun;

public class ItemGrabable : MonoBehaviourPun, IPunObservable
{
    private Rigidbody rb;
    private Transform itemGrabPointTransform;

    // Network interpolation
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private Vector3 networkVelocity;
    private Vector3 networkAngularVelocity;
    private float followSpeed = 15f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    // called when the player grabs the item
    public void Grab(Transform itemGrabPointTransform)
    {
        this.itemGrabPointTransform = itemGrabPointTransform;

        if (photonView != null)
        {
            photonView.RequestOwnership();
        }

        rb.linearDamping = 5f;
        rb.angularDamping = 5f;
        rb.useGravity = false;
    }

    // called when the player drops the item
    public void Drop()
    {
        this.itemGrabPointTransform = null;

        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.useGravity = true;
    }

    private void FixedUpdate()
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
            // Smoothly interpolate network position and rotation
            rb.MovePosition(Vector3.Lerp(rb.position, networkPosition, followSpeed * Time.fixedDeltaTime));
            rb.MoveRotation(Quaternion.Lerp(rb.rotation, networkRotation, followSpeed * Time.fixedDeltaTime));

            // Apply network velocity to keep physics realistic
            rb.linearVelocity = networkVelocity;
            rb.angularVelocity = networkAngularVelocity;
        }
    }

    // Photon sync
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Send full Rigidbody state
            stream.SendNext(rb.position);
            stream.SendNext(rb.rotation);
            stream.SendNext(rb.linearVelocity);
            stream.SendNext(rb.angularVelocity);
        }
        else
        {
            // Receive networked state
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            networkVelocity = (Vector3)stream.ReceiveNext();
            networkAngularVelocity = (Vector3)stream.ReceiveNext();
        }
    }
}
