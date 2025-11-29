using UnityEngine;

public class EquipmentLitterPicker : EquipmentBase
{
    void Update()
    {
        if (!isEquipped) return;

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Equipment B used!");
        }
    }
}
