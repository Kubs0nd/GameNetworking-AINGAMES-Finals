using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class GameManager : MonoBehaviourPun
{
    public Image trashFillImage;
    public TMP_Text trashPercentageText;

    public int maxTrashCount = 10;
    public int currentTrashCount = 0;

    void Start()
    {
        UpdateFill();
    }

    [PunRPC]
    public void RPC_IncrementTrashCounter()
    {
        currentTrashCount++;
        currentTrashCount = Mathf.Clamp(currentTrashCount, 0, maxTrashCount);
        UpdateFill();
    }

    void UpdateFill()
    {
        if (trashFillImage != null)
        {
            float fillAmount = (float)currentTrashCount / maxTrashCount;
            trashFillImage.fillAmount = fillAmount;
        }

        if (trashPercentageText != null)
        {
            float percentage = ((float)currentTrashCount / maxTrashCount) * 100f;
            trashPercentageText.text = Mathf.RoundToInt(percentage) + "%";
        }
    }

    public void TrashDumped()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_IncrementTrashCounter", RpcTarget.AllBuffered);
        }
    }
}
