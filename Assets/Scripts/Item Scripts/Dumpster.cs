using UnityEngine;
using Photon.Pun;

public class Dumpster : MonoBehaviourPun
{
    [Header("Detection")]
    public BoxCollider boxCollider;
    private Collider[] overlapResults = new Collider[16];

    [Header("Smoke Effect")]
    public GameObject smokeEffectPrefab;
    public float smokeLifetime = 3f;

    void Start()
    {
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider>();
    }

    void FixedUpdate()
    {
        DetectTrash();
    }

    void DetectTrash()
    {
        Vector3 center = boxCollider.bounds.center;
        Vector3 halfExtents = boxCollider.bounds.extents;
        Quaternion rotation = transform.rotation;

        int hits = Physics.OverlapBoxNonAlloc(center, halfExtents, overlapResults, rotation);

        for (int i = 0; i < hits; i++)
        {
            Collider col = overlapResults[i];
            if (col == null) continue;

            var trash = col.GetComponent<Trash>();
            if (trash == null) continue;

            if (PhotonNetwork.IsMasterClient)
            {
                HandleTrashDestruction(trash);
            }
            else
            {
                photonView.RPC(nameof(RPC_RequestDumpTrash), RpcTarget.MasterClient, trash.photonView.ViewID);
            }
        }
    }

    void HandleTrashDestruction(Trash trash)
    {
        Vector3 position = trash.transform.position;

        if (trash.photonView.IsMine || PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(trash.photonView);
        }

        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.TrashDumped();
        }

        photonView.RPC(nameof(RPC_SpawnSmoke), RpcTarget.AllBuffered, position);
    }

    [PunRPC]
    void RPC_RequestDumpTrash(int targetViewID)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonView targetView = PhotonView.Find(targetViewID);
        if (targetView == null) return;

        var trash = targetView.GetComponent<Trash>();
        if (trash != null)
        {
            HandleTrashDestruction(trash);
        }
    }

    [PunRPC]
    void RPC_SpawnSmoke(Vector3 position)
    {
        if (smokeEffectPrefab != null)
        {
            GameObject smoke = Instantiate(smokeEffectPrefab, position, Quaternion.identity);
            Destroy(smoke, smokeLifetime);
        }
    }
}
