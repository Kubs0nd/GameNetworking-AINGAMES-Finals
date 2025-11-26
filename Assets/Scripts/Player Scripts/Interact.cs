using UnityEngine;
using Photon.Pun;

public class Interact : MonoBehaviour
{
    [Header("Item Interaction Settings")]
    [SerializeField] private Transform playerCameraTransform;
    [SerializeField] private Transform itemGrabPointTransform;
    [SerializeField] private Transform itemEquipPointTransform;
    [SerializeField] private LayerMask layerMask;
    public int interactRange;

    private RaycastHit hit;
    private bool isHandEmpty;
    private ItemGrabable currentItem;
    private ItemEquipable equippedItem;

    private PhotonView pv;

    private void Start()
    {
        pv = GetComponent<PhotonView>();
        isHandEmpty = true; // hand starts empty
    }

    void Update()
    {
        // only run input on the local player
        if (!pv.IsMine) return;

        PickUp();
        Drop();
    }

    private void Drop()
    {
        // drop item
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!isHandEmpty)
            {
                if (currentItem != null)
                {
                    // self explanatory
                    currentItem.Drop();

                    // notify other players
                    currentItem.photonView.RPC("RPC_Drop", RpcTarget.OthersBuffered);

                    currentItem = null;
                    isHandEmpty = true;
                }
                else if (equippedItem != null)
                {
                    equippedItem.Drop();

                    // notify other players
                    equippedItem.photonView.RPC("RPC_Drop", RpcTarget.OthersBuffered);

                    equippedItem = null;
                    isHandEmpty = true;
                }

            }
        }
    }

    private void PickUp()
    {
        // initialize mask layers eligible for pickup
        int mask = LayerMask.GetMask("Grabable Item", "Equipable Item");

        // cast a ray from the player camera, then shoot that ray forward
        // ray length is determined by interactRange
        // "hit" is whatever the raycast hit
        if (Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out hit, interactRange, mask))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                // checks if the object the ray hits has the ItemGrabable script attached
                if (hit.transform.TryGetComponent(out ItemGrabable itemGrabable))
                {
                    if (isHandEmpty)
                    {
                        // calls the function from the ItemGrabable script this object is attached to
                        itemGrabable.Grab(itemGrabPointTransform);

                        // notify other players
                        itemGrabable.photonView.RPC("RPC_Grab", RpcTarget.OthersBuffered, pv.ViewID);

                        // stores variable
                        currentItem = itemGrabable;

                        // hand is occupied
                        isHandEmpty = false;

                        Debug.Log("Picked up " + currentItem.name);
                    }
                }
                else if (hit.transform.TryGetComponent(out ItemEquipable itemEquipable))
                {
                    if (isHandEmpty)
                    {
                        // calls the function from the ItemGrabable script this object is attached to
                        itemEquipable.Grab(itemEquipPointTransform);

                        // notify other players
                        itemEquipable.photonView.RPC("RPC_Grab", RpcTarget.OthersBuffered, pv.ViewID);

                        // stores variable
                        equippedItem = itemEquipable;

                        // hand is occupied
                        isHandEmpty = false;

                        Debug.Log("Equipped: " + equippedItem.name);
                    }
                }
            }
        }
    }
}
