using UnityEngine;

public class Interact : MonoBehaviour
{
    [Header("Item Interaction Settings")]
    [SerializeField] private Transform playerCameraTransform;
    [SerializeField] private Transform itemGrabPointTransform;
    [SerializeField] private Transform itemEquipPointTransform;
    public int interactRange;

    private RaycastHit hit;
    private bool isHandEmpty;
    private ItemGrabable currentItem;
    private ItemEquipable equippedItem;

    private void Start()
    {
        isHandEmpty = true; // hand starts empty
    }

    void Update()
    {
        // cast a ray from the player camera
        // ray length is determined by interactRange
        if (Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out hit, interactRange))
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

                        // stores variable
                        equippedItem = itemEquipable;

                        // hand is occupied
                        isHandEmpty = false;

                        Debug.Log("Equipped: " + currentItem.name);
                    }
                }
            }
        }

        // drop item
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!isHandEmpty)
            {
                if (currentItem != null)
                {
                    // self explanatory
                    currentItem.Drop();
                    currentItem = null;
                    isHandEmpty = true;
                }
                else if (equippedItem != null)
                {
                    equippedItem.Drop();
                    equippedItem = null;
                    isHandEmpty = true;
                }
                
            }
        }
    }
}
