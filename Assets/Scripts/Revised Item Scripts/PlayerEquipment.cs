using UnityEngine;
using Photon.Pun;

public class PlayerEquipment : MonoBehaviourPun
{
    public EquipmentBase equipmentA;
    public EquipmentBase equipmentB;

    public enum EquipState { None = 3, A = 1, B = 2 }
    public EquipState currentState = EquipState.None;

    public bool CanPickup => currentState == EquipState.None;

    private EquipmentBase currentEquipment;

    void Start()
    {
        if (equipmentA) equipmentA.DisableEquipment();
        if (equipmentB) equipmentB.DisableEquipment();
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        // num key 1
        if (Input.GetKeyDown(KeyCode.Alpha1))
            Equip(EquipState.A);

        // num key 2
        if (Input.GetKeyDown(KeyCode.Alpha2))
            Equip(EquipState.B);

        // num key 3
        if (Input.GetKeyDown(KeyCode.Alpha3))
            Equip(EquipState.None);
    }

    void Equip(EquipState state)
    {
        photonView.RPC("RPC_Equip", RpcTarget.AllBuffered, (int)state);
    }

    [PunRPC]
    void RPC_Equip(int stateValue)
    {
        EquipState newState = (EquipState)stateValue;
        currentState = newState;

        if (currentEquipment)
            currentEquipment.DisableEquipment();

        switch (currentState)
        {
            case EquipState.A:
                currentEquipment = equipmentA;
                break;

            case EquipState.B:
                currentEquipment = equipmentB;
                break;

            case EquipState.None:
                currentEquipment = null;
                break;
        }

        if (currentEquipment)
            currentEquipment.EnableEquipment();
    }
}
