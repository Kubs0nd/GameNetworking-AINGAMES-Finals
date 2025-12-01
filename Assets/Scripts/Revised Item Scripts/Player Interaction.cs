using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerInteraction : MonoBehaviourPun
{
    [Header("Interaction Settings")]
    public float interactDistance = 3f;
    public Transform holdPoint;
    public LayerMask interactMask;

    [Header("Crosshair Icons")]
    public Image defaultCrosshair;
    public Image GrabIcon;
    public Image HoldingIcon;

    private RaycastHit hit;
    private InteractableItem heldItem;
    private PlayerEquipment equipment;

    private void Start()
    {
        equipment = GetComponent<PlayerEquipment>();
        GrabIcon.enabled = false;
        HoldingIcon.enabled = false;
    }

    void Update()
    {
        if (!photonView.IsMine) return; // Only local player can interact

        if (Physics.Raycast(transform.position, transform.forward, out hit, interactDistance, interactMask))
        {
            GameObject detectedObject = hit.collider.gameObject;

            if (detectedObject.GetComponent<InteractableItem>() != null)
            {
                defaultCrosshair.enabled = false;
                GrabIcon.enabled = true;

                if (Input.GetKeyDown(KeyCode.E) && !heldItem)
                {
                    TryPickup();
                    GrabIcon.enabled = false;
                    HoldingIcon.enabled = true;
                }
            }
            else
            {
                GrabIcon.enabled = false;

                if (!heldItem)
                {
                    HoldingIcon.enabled = false;
                    defaultCrosshair.enabled = true;
                }
                else
                {
                    defaultCrosshair.enabled = false;
                    HoldingIcon.enabled = true;
                }
            }
        }
        else
        {
            GrabIcon.enabled = false;

            if (!heldItem)
            {
                HoldingIcon.enabled = false;
                defaultCrosshair.enabled = true;
            }
            else
            {
                defaultCrosshair.enabled = false;
                HoldingIcon.enabled = true;
            }
        }


        // Drop
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (heldItem)
            {
                heldItem.Drop();
                HoldingIcon.enabled = false;
                defaultCrosshair.enabled = true;
                heldItem = null;

            }
        }
    }

    void TryPickup()
    {
        if (!equipment.CanPickup) return;

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
