using UnityEngine;
using Photon.Pun;

public class ItemEquipable : MonoBehaviourPun, IPunObservable
{
    private Rigidbody rb;
    private Transform itemEquipPointTransform;

    // Network interpolation variables
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private float followSpeed = 20f;

    private void Awake()
    {
        // get the Rigidbody attached to this object
        rb = GetComponent<Rigidbody>();

        // set interpolation for smooth physics movement
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    // called when the player grabs the item
    public void Grab(Transform itemEquipPointTransform)
    {
        this.itemEquipPointTransform = itemEquipPointTransform;

        // take ownership so the local player can control this item
        if (photonView != null)
        {
            photonView.RequestOwnership();
        }

        // add drag so the item doesn't jitter
        rb.linearDamping = 5f;

        // prevents item from rotating while equipped
        rb.freezeRotation = true;

        // disable gravity so the item doesn't jitter and force it to fall
        rb.useGravity = false;
    }

    // called when the player drops the item
    public void Drop()
    {
        this.itemEquipPointTransform = null;

        // unparent the item
        transform.SetParent(null);

        // reset drag to default so it falls normally
        rb.linearDamping = 0f;

        // re-enable gravity so the item drops to the ground
        rb.useGravity = true;

        // allow rotation again
        rb.freezeRotation = false;
    }

    private void LateUpdate()
    {
        // if we own the item, move it toward the equip point
        if (itemEquipPointTransform != null && photonView.IsMine)
        {
            Vector3 newPos = Vector3.Lerp(rb.position, itemEquipPointTransform.position, followSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);

            Quaternion newRot = Quaternion.Lerp(rb.rotation, itemEquipPointTransform.rotation, followSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newRot);
        }
        // if we don't own it, interpolate to networked position/rotation
        else if (!photonView.IsMine)
        {
            rb.MovePosition(Vector3.Lerp(rb.position, networkPosition, followSpeed * Time.deltaTime));
            rb.MoveRotation(Quaternion.Lerp(rb.rotation, networkRotation, followSpeed * Time.deltaTime));
        }
    }

    // Photon sync
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // owner sends position/rotation
            stream.SendNext(rb.position);
            stream.SendNext(rb.rotation);
        }
        else
        {
            // remote clients receive
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
