using UnityEngine;
using Photon.Pun;

public class ItemEquipable : NetworkInteractable
{
    private int originalLayer;

    protected override void Awake()
    {
        base.Awake();
        originalLayer = gameObject.layer;
    }

    /// <summary>
    /// Call this to grab/equip the item
    /// </summary>
    public void GrabEquip(Transform equipPoint)
    {
        RequestGrab(equipPoint);

        if (photonView.IsMine)
        {
            gameObject.layer = LayerMask.NameToLayer("Equipped");
        }
    }

    public new void Drop()
    {
        base.Drop();

        gameObject.layer = originalLayer;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Interact holder = FindFirstObjectByType<Interact>();
        if (holder != null)
        {
            holder.ForceReleaseItem(equipable: this);
        }
    }
}
