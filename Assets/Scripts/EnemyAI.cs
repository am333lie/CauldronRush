using UnityEngine;
using UnityEngine.AI;

// Replaces EnemyPatrolTest. Patrols the waypoints, chases the player when it
// sees her, searches the last known position when it loses her, then goes
// back to patrolling. Catching is still done by EnemyCatch.
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    private enum State { Patrol, Chase, Search }

    [Header("Patrol")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float waitAtWaypoint = 1f;

    [Header("Vision")]
    [SerializeField] private float viewDistance = 12f;
    [SerializeField, Range(0f, 360f)] private float viewAngle = 110f;
    [Tooltip("Inside this radius the enemy notices the player even from behind.")]
    [SerializeField] private float senseDistance = 2.5f;
    [SerializeField] private float eyeHeight = 1.5f;
    [Tooltip("Layers that block the enemy's view (terrain, walls, ...).")]
    [SerializeField] private LayerMask visionBlockers = ~0;

    [Header("Chase")]
    [SerializeField] private float chaseSpeed = 3.3f;
    [Tooltip("Seconds without seeing the player before the chase is given up.")]
    [SerializeField] private float loseSightTime = 3f;
    [Tooltip("Seconds spent looking around at the last known position.")]
    [SerializeField] private float searchTime = 4f;

    private NavMeshAgent agent;
    private Transform player;
    private Collider playerCollider;

    private State state = State.Patrol;
    private int index;
    private int direction = 1; // 1 = forward, -1 = backward (back and forth)
    private float waitTimer;
    private float lostTimer;
    private float searchTimer;
    private float repathTimer;
    private Vector3 lastKnownPosition;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerCollider = playerObject.GetComponent<Collider>();
        }

        ReturnToPatrol();
    }

    private void Update()
    {
        bool playing = GameManager.Instance == null
                       || GameManager.Instance.State == GameState.Playing;
        bool seesPlayer = playing && CanSeePlayer();

        switch (state)
        {
            case State.Patrol:
                if (seesPlayer) StartChase();
                else UpdatePatrol();
                break;

            case State.Chase:
                UpdateChase(seesPlayer);
                break;

            case State.Search:
                if (seesPlayer) StartChase();
                else UpdateSearch();
                break;
        }
    }

    // ---------- Patrol ----------

    private void UpdatePatrol()
    {
        if (waypoints == null || waypoints.Length == 0 || !HasArrived()) return;

        waitTimer += Time.deltaTime;
        if (waitTimer < waitAtWaypoint || waypoints.Length < 2) return;

        if (index + direction >= waypoints.Length || index + direction < 0)
            direction *= -1;

        index += direction;
        waitTimer = 0f;
        agent.SetDestination(waypoints[index].position);
    }

    private void ReturnToPatrol()
    {
        state = State.Patrol;
        agent.speed = patrolSpeed;
        waitTimer = 0f;

        if (waypoints != null && waypoints.Length > 0)
            agent.SetDestination(waypoints[index].position);
    }

    // ---------- Chase ----------

    private void StartChase()
    {
        state = State.Chase;
        agent.speed = chaseSpeed;
        lostTimer = 0f;
        repathTimer = 0f;
    }

    private void UpdateChase(bool seesPlayer)
    {
        if (seesPlayer)
        {
            lostTimer = 0f;
            lastKnownPosition = player.position;
        }
        else
        {
            lostTimer += Time.deltaTime;
            if (lostTimer >= loseSightTime)
            {
                state = State.Search;
                searchTimer = 0f;
                agent.SetDestination(lastKnownPosition);
                return;
            }
        }

        // Recalculate the path a few times per second, not every frame
        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            repathTimer = 0.2f;
            agent.SetDestination(player.position);
        }
    }

    // ---------- Search ----------

    private void UpdateSearch()
    {
        if (!HasArrived()) return;

        // Look around on the spot
        transform.Rotate(0f, 90f * Time.deltaTime, 0f);

        searchTimer += Time.deltaTime;
        if (searchTimer >= searchTime) ReturnToPatrol();
    }

    // ---------- Helpers ----------

    private bool HasArrived()
    {
        return !agent.pathPending
               && agent.remainingDistance <= agent.stoppingDistance + 0.1f;
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 eyes = transform.position + Vector3.up * eyeHeight;
        Vector3 target = playerCollider != null
            ? playerCollider.bounds.center
            : player.position + Vector3.up;

        Vector3 toPlayer = target - eyes;
        float distance = toPlayer.magnitude;
        if (distance > viewDistance) return false;

        // Outside the "sense" radius the player has to be inside the view cone
        if (distance > senseDistance)
        {
            Vector3 flat = toPlayer;
            flat.y = 0f;
            if (Vector3.Angle(transform.forward, flat) > viewAngle * 0.5f)
                return false;
        }

        // Something between the eyes and the player?
        if (Physics.Linecast(eyes, target, out RaycastHit hit,
                visionBlockers, QueryTriggerInteraction.Ignore))
        {
            // Hitting the player (or the enemy's own body) does not block the view
            return hit.transform.IsChildOf(player) || hit.transform.IsChildOf(transform);
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 eyes = transform.position + Vector3.up * eyeHeight;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyes, viewDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(eyes, senseDistance);

        Gizmos.color = Color.cyan;
        Vector3 left = Quaternion.Euler(0f, -viewAngle * 0.5f, 0f) * transform.forward;
        Vector3 right = Quaternion.Euler(0f, viewAngle * 0.5f, 0f) * transform.forward;
        Gizmos.DrawRay(eyes, left * viewDistance);
        Gizmos.DrawRay(eyes, right * viewDistance);
    }
}
