using UnityEngine;
using UnityEngine.AI;

public class EnemyPathFollower : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform[] waypoints;
    private int index;

    public void Init(PathController path)
    {
        if (path == null)
            return;

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
            if (agent == null)
                return;

            agent.autoRepath = true;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }

        waypoints = path.Waypoints;
        if (waypoints == null || waypoints.Length == 0)
            return;

        agent.Warp(waypoints[0].position);

        if (waypoints.Length == 1)
        {
            index = 0;
            agent.SetDestination(waypoints[0].position);
        }
        else
        {
            index = 1;
            agent.SetDestination(waypoints[1].position);
        }
    }

    private void Update()
    {
        if (agent == null)
            return;

        if (waypoints == null || waypoints.Length == 0)
            return;

        if (agent.pathPending)
            return;

        if (agent.remainingDistance > 0.1f)
            return;

        index++;

        if (index < waypoints.Length)
        {
            agent.SetDestination(waypoints[index].position);
        }
        else
        {
            var gameManager = GameManager.Instance;
            if (gameManager != null)
                gameManager.ChangeLives(-1);

            Destroy(gameObject);
        }
    }
}