using UnityEngine;
using Photon.Pun;

public class Dumpster : MonoBehaviourPun
{
    public BoxCollider boxCollider;
    public GameObject smokeEffectPrefab;
    public float smokeLifetime = 3f;
    private Collider[] overlapResults = new Collider[16];

    void Start()
    {
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider>();
    }

    void FixedUpdate()
    {
        Vector3 center = boxCollider.bounds.center;
        Vector3 halfExtents = boxCollider.bounds.extents;
        Quaternion rotation = transform.rotation;

        int hits = Physics.OverlapBoxNonAlloc(center, halfExtents, overlapResults, rotation);

        for (int i = 0; i < hits; i++)
        {
            Collider col = overlapResults[i];
            if (col == null) continue;

            var ni = col.GetComponent<NetworkInteractable>();
            if (ni == null) continue;

            if (PhotonNetwork.IsMasterClient)
            {
                HandleTrashDestruction(ni);
            }
            else
            {
                photonView.RPC(nameof(RPC_RequestDumpTrash), RpcTarget.MasterClient, ni.photonView.ViewID);
            }
        }
    }

    void HandleTrashDestruction(NetworkInteractable ni)
    {
        Vector3 pos = ni.transform.position;

        if (ni.photonView.IsMine || PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(ni.photonView);
        }

        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null) gm.TrashDumped();

        photonView.RPC(nameof(RPC_SpawnSmoke), RpcTarget.AllBuffered, pos);
    }

    [PunRPC]
    void RPC_RequestDumpTrash(int targetViewID)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonView tv = PhotonView.Find(targetViewID);
        if (tv == null) return;

        var ni = tv.GetComponent<NetworkInteractable>();
        if (ni != null) HandleTrashDestruction(ni);
    }

    [PunRPC]
    void RPC_SpawnSmoke(Vector3 position)
    {
        if (smokeEffectPrefab != null)
        {
            var s = Instantiate(smokeEffectPrefab, position, Quaternion.identity);
            Destroy(s, smokeLifetime);
        }
    }
}
