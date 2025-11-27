using UnityEngine;
using Photon.Pun;

public class Dirt : MonoBehaviourPun, IPunObservable
{
    [Header("Dirt Settings")]
    public GameObject dirt1;
    public float maxCleanProgress = 100f;

    [Header("Clean Progress")]
    [SerializeField] private float cleanProgress = 0f;

    private Vector3 initialScale;

    private void Start()
    {
        cleanProgress = 0f;

        if (dirt1 != null)
        {
            initialScale = dirt1.transform.localScale;
            dirt1.SetActive(true);
            dirt1.transform.localScale = initialScale;
        }
    }

    private void Update()
    {
        if (dirt1 != null && dirt1.activeSelf)
        {
            // scale down dirt1 based on cleanProgress
            float scalePercent = Mathf.Clamp01(1f - (cleanProgress / maxCleanProgress));
            dirt1.transform.localScale = initialScale * scalePercent;

            // if fully clean, disable object
            if (cleanProgress >= maxCleanProgress)
            {
                dirt1.SetActive(false);
            }
        }
    }

    /// increment cleaning progress locally (owner only)
    public void CleanUp(float progress)
    {
        if (!photonView.IsMine) return;

        cleanProgress += progress;
        cleanProgress = Mathf.Clamp(cleanProgress, 0f, maxCleanProgress);
    }

    /// RPC called by remote players to clean this dirt
    [PunRPC]
    public void RPC_CleanUp(float progress)
    {
        CleanUp(progress);
    }

    /// photon PUN synchronization
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(cleanProgress);
        }
        else
        {
            cleanProgress = (float)stream.ReceiveNext();
        }
    }
}
