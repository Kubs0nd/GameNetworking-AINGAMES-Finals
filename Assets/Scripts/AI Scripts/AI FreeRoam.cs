using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;

public class AIFreeRoam : MonoBehaviourPun
{
    NavMeshAgent agent;

    [SerializeField] LayerMask groundlayer;

    Vector3 dest;
    bool walkpointSet;

    [SerializeField] float range;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Roam();
    }

    void Roam()
    {
        if (!walkpointSet) NextDest();
        if (walkpointSet) agent.SetDestination(dest);
        if (Vector3.Distance(transform.position, dest) < 1) walkpointSet = false;
    }

    void NextDest()
    {
        float z = Random.Range(-range, range);
        float x = Random.Range(-range, range);

        dest = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);

        if (Physics.Raycast(dest, Vector3.down, groundlayer))
        {
            walkpointSet = true;
        }
    }
}
