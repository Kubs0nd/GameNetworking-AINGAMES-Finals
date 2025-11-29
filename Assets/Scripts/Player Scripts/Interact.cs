using UnityEngine;
using Photon.Pun;
using static UnityEngine.UI.Image;
using Unity.VisualScripting;

public class Interact : MonoBehaviour
{
    [SerializeField] private Transform playerCameraTransform;
    [SerializeField] private Transform itemGrabPointTransform;
    [SerializeField] private Transform itemEquipPointTransform;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float interactRange = 3f;

    private PhotonView pv;
    private ItemGrabable currentItem;
    private ItemEquipable equippedItem;
    private bool isHandEmpty => currentItem == null && equippedItem == null;


    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (!pv.IsMine) return;

        if (!isHandEmpty)
        {
            if (currentItem == null || currentItem.Equals(null))
            {
                currentItem = null;
            }

            if (equippedItem == null || equippedItem.Equals(null))
            {
                equippedItem = null;
            }
        }


        if (Input.GetKeyDown(KeyCode.E) && isHandEmpty)
        {
            Debug.DrawRay(playerCameraTransform.position, playerCameraTransform.forward * interactRange, Color.red);
            if (Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out RaycastHit hit, interactRange, layerMask))
            {
                if (hit.transform.TryGetComponent(out ItemGrabable grabable))
                {
                    grabable.Grab(itemGrabPointTransform);
                    currentItem = grabable;
                    grabable.photonView.RPC("RPC_OnGrab", RpcTarget.OthersBuffered);
                }
                else if (hit.transform.TryGetComponent(out ItemEquipable equipable))
                {
                    equipable.GrabEquip(itemEquipPointTransform);
                    equippedItem = equipable;
                    equipable.photonView.RPC("RPC_OnGrab", RpcTarget.OthersBuffered);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (currentItem != null)
            {
                currentItem.Drop();
                currentItem = null;
            }
            else if (equippedItem != null)
            {
                equippedItem.Drop();
                equippedItem = null;
            }
        }
    }

    public void ForceReleaseItem(ItemGrabable grabable = null, ItemEquipable equipable = null)
    {
        if (grabable != null && currentItem == grabable)
            currentItem = null;

        if (equipable != null && equippedItem == equipable)
            equippedItem = null;
    }
}
