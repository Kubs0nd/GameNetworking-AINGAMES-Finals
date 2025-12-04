using UnityEngine;
using UnityEngine.AI;

public class AITrashAvoidance : MonoBehaviour
{
    private NavMeshAgent agent;

    [Header("Trash Avoidance Settings")]
    [SerializeField] private float forwardCheck = 1.4f;     // small objects = short detection
    [SerializeField] private float sideCheck = 1.0f;
    [SerializeField] private float sideOffset = 0.6f;
    [SerializeField] private float turnStrength = 3.5f;     // gentle turning
    [SerializeField] private LayerMask trashLayer;

    private Vector3 origin;
    private Vector3 forward;
    private Vector3 left;
    private Vector3 right;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        CacheDirections();
        AvoidTrash();
    }

    private void CacheDirections()
    {
        origin = transform.position + Vector3.up * 0.3f;  // lower rays = detect trash better
        forward = transform.forward;
        left = -transform.right;
        right = transform.right;
    }

    private void AvoidTrash()
    {
        Vector3 steer = Vector3.zero;

        // ---- Front Trash Checker ----
        if (Physics.Raycast(origin, forward, forwardCheck, trashLayer))
        {
            bool spaceOnLeft = !Physics.Raycast(origin + (left * sideOffset), left, sideCheck, trashLayer);
            steer += spaceOnLeft ? left : right;  // pick the cleaner side
        }

        // ---- Left Trash Checker ----
        if (Physics.Raycast(origin + (left * sideOffset), left, sideCheck, trashLayer))
            steer += right * 1.2f;  // slight weight to keep AI from hugging trash

        // ---- Right Trash Checker ----
        if (Physics.Raycast(origin + (right * sideOffset), right, sideCheck, trashLayer))
            steer += left * 1.2f;

        if (steer == Vector3.zero)
            return;

        // Smooth, gentle direction change
        steer = steer.normalized * turnStrength;

        Vector3 finalDir = (agent.desiredVelocity + steer).normalized;

        agent.Move(finalDir * agent.speed * Time.deltaTime);

        if (finalDir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(finalDir),
                Time.deltaTime * 4f   // slow turn = natural trash avoidance
            );
        }
    }
}

