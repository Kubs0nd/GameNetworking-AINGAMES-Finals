using UnityEngine;

public class EquipmentVacuum : EquipmentBase
{
    void Update()
    {
        if (!isEquipped) return;

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Equipment A fired!");
        }
    }
}
