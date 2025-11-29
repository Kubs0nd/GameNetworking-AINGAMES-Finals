using UnityEngine;
using Photon.Pun;

public class Dirt : NetworkInteractable
{
    public float cleanliness = 1f;
    private Vector3 initialScale;

    protected override void Awake()
    {
        base.Awake();
        initialScale = transform.localScale;
    }


    public void CleanUp(float amount)
    {
        if (photonView != null && !photonView.IsMine)
        {
            photonView.RPC(nameof(RPC_CleanUp), photonView.Owner, amount);
            return;
        }

        RPC_CleanUp(amount);
    }

    [PunRPC]
    void RPC_CleanUp(float amount)
    {
        cleanliness -= amount;
        cleanliness = Mathf.Clamp01(cleanliness);

        transform.localScale = initialScale * cleanliness;

        if (cleanliness <= 0f)
        {
            PhotonNetwork.Destroy(photonView);
        }
    }

}
