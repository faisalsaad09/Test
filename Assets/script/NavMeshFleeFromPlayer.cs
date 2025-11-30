using UnityEngine;
using UnityEngine.AI;

public class NavMeshFleeFromPlayer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    private NavMeshAgent agent;
    
    [Header("Flee Settings")]
    [SerializeField] private float fleeSpeed = 6f;
    [SerializeField] private float fleeDistance = 10f;
    [SerializeField] private float safeDistance = 15f;
    
    [Header("Flee Calculation")]
    [SerializeField] private float fleePointDistance = 10f; // How far to calculate flee point
    [SerializeField] private int fleePointAttempts = 8; // Number of directions to try
    [SerializeField] private float updateInterval = 0.5f; // How often to recalculate flee point
    
    private float nextUpdateTime;
    private Vector3 currentFleePoint;
    
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component required! Please add NavMeshAgent to " + gameObject.name);
            enabled = false;
            return;
        }
        
        agent.speed = fleeSpeed;
        
        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("Player not found! Assign player or tag with 'Player'.");
            }
        }
    }
    
    private void Update()
    {
        if (player == null || agent == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Check if we need to flee
        if (distanceToPlayer < fleeDistance)
        {
            // Update flee point periodically or if we've reached current destination
            if (Time.time >= nextUpdateTime || !agent.hasPath || agent.remainingDistance < 1f)
            {
                CalculateFleePoint();
                nextUpdateTime = Time.time + updateInterval;
            }
        }
        else if (distanceToPlayer >= safeDistance)
        {
            // Stop fleeing when safe
            if (agent.hasPath)
            {
                agent.ResetPath();
            }
        }
    }
    
    private void CalculateFleePoint()
    {
        // Direction away from player
        Vector3 directionAwayFromPlayer = (transform.position - player.position).normalized;
        
        // Try to find a valid point on the NavMesh away from the player
        Vector3 bestFleePoint = transform.position;
        float bestScore = float.MinValue;
        
        for (int i = 0; i < fleePointAttempts; i++)
        {
            // Create points in a cone away from the player
            float angle = (i / (float)fleePointAttempts) * 120f - 60f; // Spread across 120 degrees
            Vector3 rotatedDirection = Quaternion.Euler(0, angle, 0) * directionAwayFromPlayer;
            Vector3 targetPoint = transform.position + rotatedDirection * fleePointDistance;
            
            // Check if point is on NavMesh
            if (NavMesh.SamplePosition(targetPoint, out NavMeshHit hit, fleePointDistance * 0.5f, NavMesh.AllAreas))
            {
                // Score based on distance from player (farther is better)
                float distanceFromPlayer = Vector3.Distance(hit.position, player.position);
                
                // Prefer points that are actually reachable
                NavMeshPath path = new NavMeshPath();
                if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    float score = distanceFromPlayer;
                    
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestFleePoint = hit.position;
                    }
                }
            }
        }
        
        // Set destination to best flee point found
        if (bestScore > float.MinValue)
        {
            currentFleePoint = bestFleePoint;
            agent.SetDestination(currentFleePoint);
        }
        else
        {
            // Fallback: just move directly away
            Vector3 fallbackPoint = transform.position + directionAwayFromPlayer * fleePointDistance;
            if (NavMesh.SamplePosition(fallbackPoint, out NavMeshHit hit, fleePointDistance, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }
    
    
    
}