using UnityEngine;
using Photon.Pun;

public class ItemGrabable : NetworkInteractable
{
    private int originalLayer;

    protected override void Awake()
    {
        base.Awake();
        originalLayer = gameObject.layer;
    }

    /// <summary>
    /// Call this to grab the item
    /// </summary>
    public void Grab(Transform grabPoint)
    {
        RequestGrab(grabPoint);

        if (photonView.IsMine)
        {
            gameObject.layer = LayerMask.NameToLayer("Grabbed");
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
            holder.ForceReleaseItem(grabable: this);
        }
    }
}
