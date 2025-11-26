using UnityEngine;
using UnityEngine.AI;

public class EnemyPathFollower : MonoBehaviour
{
    [SerializeField] private PathController path;

    private NavMeshAgent agent;
    private int index;
    private Transform[] waypoints;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoRepath = true;
    }

    private void Start()
    {
        if (path == null)
        {
            enabled = false;
            return;
        }

        waypoints = path.Waypoints;
        if (waypoints == null || waypoints.Length == 0)
        {
            enabled = false;
            return;
        }

        index = 0;
        agent.Warp(waypoints[index].position);
        agent.SetDestination(waypoints[index].position);
    }

    private void Update()
    {
        if (agent.pathPending)
            return;

        if (agent.remainingDistance > 0.1f)
            return;

        index++;

        if (index < waypoints.Length)
            agent.SetDestination(waypoints[index].position);
        else
            Destroy(gameObject);
    }
}