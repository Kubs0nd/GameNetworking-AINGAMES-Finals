using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class GameManager : MonoBehaviourPun
{
    [Header("UI Elements")]
    public Image trashFillImage;
    public TMP_Text trashPercentageText;

    [Header("Trash Settings")]
    public int maxTrashCount = 10;
    public int currentTrashCount = 0;

    void Start()
    {
        UpdateFill();
    }

    public void TrashDumped()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(RPC_IncrementTrashCounter), RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void RPC_IncrementTrashCounter()
    {
        currentTrashCount++;
        currentTrashCount = Mathf.Clamp(currentTrashCount, 0, maxTrashCount);
        UpdateFill();
    }

    void UpdateFill()
    {
        if (trashFillImage != null)
        {
            trashFillImage.fillAmount = (float)currentTrashCount / maxTrashCount;
        }

        if (trashPercentageText != null)
        {
            float percentage = ((float)currentTrashCount / maxTrashCount) * 100f;
            trashPercentageText.text = Mathf.RoundToInt(percentage) + "%";
        }
    }
}
