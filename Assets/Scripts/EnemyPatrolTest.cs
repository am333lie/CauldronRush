using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyPatrolTest : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;

    private NavMeshAgent agent;
    private int index;
    private int direction = 1; // 1 = forward, -1 = backward (back and forth)

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (waypoints.Length > 0) agent.SetDestination(waypoints[0].position);
    }

    private void Update()
    {
        if (waypoints.Length < 2) return;

        // Arrived at the current waypoint
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (index + direction >= waypoints.Length || index + direction < 0)
                direction *= -1;

            index += direction;
            agent.SetDestination(waypoints[index].position);
        }
    }
}