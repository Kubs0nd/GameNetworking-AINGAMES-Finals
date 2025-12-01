using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.VisualScripting;

public class AISpawn : MonoBehaviourPun
{
    [Header("AI Spawning")]
    public string AI;
    public Transform[] spawnpoint;
    public int AIcount = 0;


    void Start()
    {

    }
    void Update()
    {
        // Only Master Client calls the RPC
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SpawnAI", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void SpawnAI()
    {

        // Only Master Client should instantiate
        if (PhotonNetwork.IsMasterClient)
        {

            int index = Random.Range(0, spawnpoint.Length);
            Transform point = spawnpoint[index];

            if (AIcount != 3)
            {
                PhotonNetwork.Instantiate(AI, point.position, point.rotation);
                Debug.Log("Success");
                ++AIcount;
            }
        }


    }
}