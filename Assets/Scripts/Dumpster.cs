using UnityEngine;
using Photon.Pun;

public class Dumpster : MonoBehaviourPun
{
    [Header("References")]
    public BoxCollider boxCollider;
    public GameObject smokeEffectPrefab;
    public float smokeLifetime = 3f;

    private Collider[] overlapResults = new Collider[10];

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    void Update()
    {
        Vector3 center = boxCollider.bounds.center;
        Vector3 halfExtents = boxCollider.bounds.extents;
        Quaternion rotation = transform.rotation;

        int hits = Physics.OverlapBoxNonAlloc(center, halfExtents, overlapResults, rotation);

        for (int i = 0; i < hits; i++)
        {
            Collider col = overlapResults[i];
            if (col == null) continue;

            Trash trash = col.GetComponent<Trash>();
            if (trash != null)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.Destroy(trash.gameObject);

                    GameManager gm = FindAnyObjectByType<GameManager>();
                    if (gm != null)
                        gm.TrashDumped();

                    photonView.RPC("RPC_SpawnSmoke", RpcTarget.AllBuffered, trash.transform.position);
                }
            }
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
