using UnityEngine;

public abstract class EquipmentBase : MonoBehaviour
{
    protected bool isEquipped = false;

    public virtual void EnableEquipment()
    {
        isEquipped = true;
        gameObject.SetActive(true);
    }

    public virtual void DisableEquipment()
    {
        isEquipped = false;
        gameObject.SetActive(false);
    }
}
