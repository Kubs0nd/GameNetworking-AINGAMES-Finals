using UnityEngine;
using Photon.Pun;

public class InteractableItem : MonoBehaviourPun
{
    [HideInInspector] public bool isPickedUp = false;
    private Transform originalParent;

    private Transform holdPoint;
    public float followSpeed = 10f;

    private Rigidbody rb;

    private void Awake()
    {
        originalParent = transform.parent;
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (isPickedUp && holdPoint != null)
        {
            // smoothly move the thing to its hold position
            rb.MovePosition(Vector3.Lerp(rb.position, holdPoint.position, followSpeed * Time.fixedDeltaTime));
            rb.MoveRotation(Quaternion.Lerp(rb.rotation, holdPoint.rotation, followSpeed * Time.fixedDeltaTime));
        }
    }

    public void PickUp(Transform newHoldPoint)
    {
        // pls pls pls work
        if (!photonView.IsMine)
            photonView.RequestOwnership();

        photonView.RPC("RPC_PickUp", RpcTarget.AllBuffered, newHoldPoint.GetComponent<PhotonView>().ViewID);
    }

    public void Drop()
    {
        photonView.RPC("RPC_Drop", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPC_PickUp(int holdPointViewID)
    {
        PhotonView holdPV = PhotonView.Find(holdPointViewID);
        holdPoint = holdPV.transform;

        isPickedUp = true;
        if (rb != null)
            rb.isKinematic = true; // disable physics while held
    }

    [PunRPC]
    void RPC_Drop()
    {
        isPickedUp = false;
        holdPoint = null;
        transform.SetParent(originalParent); // return to original parent
        if (rb != null)
            rb.isKinematic = false; // re-enable physics
    }
}
