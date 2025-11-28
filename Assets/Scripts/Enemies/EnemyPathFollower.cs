using Core;
using Level;
using UnityEngine;
using UnityEngine.AI;

namespace Enemies
{
    public class EnemyPathFollower : MonoBehaviour
    {
        [SerializeField] private float reachedDistance = 0.1f;

        private NavMeshAgent _agent;
        private Transform[] _waypoints;
        private int _index;

        public void Init(PathController path)
        {
            if (path == null)
                return;

            if (_agent == null)
            {
                _agent = GetComponent<NavMeshAgent>();
                if (_agent == null)
                    return;

                _agent.autoRepath = true;
            }

            _waypoints = path.Waypoints;
            if (_waypoints == null || _waypoints.Length == 0)
                return;

            _index = 0;

            _agent.Warp(_waypoints[0].position);

            if (_waypoints.Length > 1)
            {
                _index = 1;
                _agent.SetDestination(_waypoints[1].position);
            }
        }

        private void Update()
        {
            if (_agent == null || _waypoints == null || _waypoints.Length == 0)
                return;

            if (_agent.pathPending)
                return;

            if (_agent.remainingDistance > reachedDistance)
                return;

            _index++;

            if (_index < _waypoints.Length)
            {
                _agent.SetDestination(_waypoints[_index].position);
            }
            else
            {
                var gameManager = GameManager.Instance;
                if (gameManager != null)
                {
                    gameManager.ChangeLives(-1);
                    gameManager.NotifyEnemyRemoved();
                }

                Destroy(gameObject);
            }
        }
    }
}