using UnityEngine;
using Photon.Pun;

public class PlayerInteraction : MonoBehaviourPun
{
    [Header("Interaction Settings")]
    public float interactDistance = 3f;
    public Transform holdPoint;
    public LayerMask interactMask;

    private InteractableItem heldItem;

    void Update()
    {
        if (!photonView.IsMine) return; // Only local player can interact

        // Pickup
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!heldItem)
                TryPickup();
        }

        // Drop
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (heldItem)
            {
                heldItem.Drop();
                heldItem = null;
            }
        }
    }

    void TryPickup()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask))
        {
            InteractableItem item = hit.collider.GetComponent<InteractableItem>();
            if (item != null && !item.isPickedUp)
            {
                item.PickUp(holdPoint);
                heldItem = item;
            }
        }
    }
}
