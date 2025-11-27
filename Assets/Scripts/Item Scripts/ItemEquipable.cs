using UnityEngine;
using Photon.Pun;

public class ItemEquipable : MonoBehaviourPun, IPunObservable
{
    // MAKE SURE WHEN YOU ATTACH THIS SCRIPT TO AN OBJECT, SET THE LAYER TO EQUIPABLE ITEM

    private Rigidbody rb;
    private Transform itemEquipPointTransform;

    // networked state 
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private Vector3 networkVelocity;
    private Vector3 networkAngularVelocity;

    private float followSpeed = 20f;
    private bool isEquipped;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        isEquipped = false;
    }

    // called when the player grabs the item
    public void Grab(Transform itemEquipPointTransform)
    {
        this.itemEquipPointTransform = itemEquipPointTransform;

        // photon ownership
        if (photonView != null && !photonView.IsMine)
            photonView.RequestOwnership();

        // Sets the layer of the object to "Equipped" to prevent other players from taking it from your hands
        gameObject.layer = LayerMask.NameToLayer("Equipped");

        isEquipped = true;
        rb.linearDamping = 5f;
        rb.freezeRotation = true;
        rb.useGravity = false;
    }

    // called when the player drops the item
    public void Drop()
    {
        this.itemEquipPointTransform = null;

        // Sets the layer back to a "Equipable Item" where the any player can equip it
        gameObject.layer = LayerMask.NameToLayer("Equipable Item");

        isEquipped = false;
        rb.linearDamping = 0f;
        rb.freezeRotation = false;
        rb.useGravity = true;
        rb.freezeRotation = false;
    }

    // RPC called by Interact to inform ALL clients that this item was grabbed
    [PunRPC]
    void RPC_Grab(int playerViewID)
    {
        // If this client is the owner, the local Grab(...) call already ran and set the follow transform.
        // For non-owning clients we still want to update visible state (layer/physics) so other clients
        // know the item is held and cannot be grabbed.
        if (photonView.IsMine) return;

        // Sets the layer of the object to "Equipped" to prevent other players from taking it from your hands
        gameObject.layer = LayerMask.NameToLayer("Equipped");

        rb.linearDamping = 5f;
        rb.freezeRotation = true;
        rb.useGravity = false;
    }

    // RPC called by Interact to inform ALL clients that this item was dropped
    [PunRPC]
    void RPC_Drop()
    {
        // For non-owning clients, clear any local references and restore physics state.
        this.itemEquipPointTransform = null;

        // Sets the layer back to a "Equipable Item" where the any player can equip it
        gameObject.layer = LayerMask.NameToLayer("Equipable Item");

        rb.linearDamping = 0f;
        rb.freezeRotation = false;
        rb.useGravity = true;
        rb.freezeRotation = false;
    }

    private void LateUpdate()
    {
        if (photonView.IsMine && itemEquipPointTransform != null)
        {
            // smooth follow (network sync)
            Vector3 newPos = Vector3.Lerp(rb.position, itemEquipPointTransform.position, followSpeed * Time.fixedDeltaTime);
            Quaternion newRot = Quaternion.Lerp(rb.rotation, itemEquipPointTransform.rotation, followSpeed * Time.fixedDeltaTime);

            rb.MovePosition(newPos);
            rb.MoveRotation(newRot);
        }
        else if (!photonView.IsMine)
        {
            // remote smoothing
            rb.MovePosition(
                Vector3.Lerp(rb.position, networkPosition, followSpeed * Time.fixedDeltaTime)
            );

            rb.MoveRotation(
                Quaternion.Lerp(rb.rotation, networkRotation, followSpeed * Time.fixedDeltaTime)
            );

            rb.linearVelocity = networkVelocity;
            rb.angularVelocity = networkAngularVelocity;
        }
    }

    // photon sync
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(rb.position);
            stream.SendNext(rb.rotation);
            stream.SendNext(rb.linearVelocity);
            stream.SendNext(rb.angularVelocity);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            networkVelocity = (Vector3)stream.ReceiveNext();
            networkAngularVelocity = (Vector3)stream.ReceiveNext();
        }
    }
}
