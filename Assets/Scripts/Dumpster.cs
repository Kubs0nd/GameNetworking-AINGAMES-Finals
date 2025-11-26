using UnityEngine;
using Photon.Pun;

public class Dumpster : MonoBehaviourPun
{
    [Header("References")]
    public BoxCollider boxCollider;
    public GameObject smokeEffectPrefab;

    private Collider[] overlapResults = new Collider[10];

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    void FixedUpdate()
    {
        // use Physics.OverlapBox to detect objects inside the dumpster
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
                // only the Master Client should destroy the object
                if (PhotonNetwork.IsMasterClient)
                {
                    // destroy the object across the network
                    PhotonNetwork.Destroy(col.gameObject);

                    // spawn smoke effect for everyone
                    photonView.RPC("RPC_SpawnSmoke", RpcTarget.All, col.transform.position);
                }
            }
        }
    }

    [PunRPC]
    void RPC_SpawnSmoke(Vector3 position)
    {
        if (smokeEffectPrefab != null)
        {
            Instantiate(smokeEffectPrefab, position, Quaternion.identity);
        }
    }
}
