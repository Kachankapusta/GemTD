using Core;
using Level;
using UnityEngine;
using UnityEngine.AI;

namespace Enemies
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Enemy))]
    public class EnemyPathFollower : MonoBehaviour
    {
        [SerializeField] private float reachedDistance = 0.2f;

        private NavMeshAgent _agent;
        private Enemy _enemy;
        private int _index;
        private Transform[] _waypoints;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _enemy = GetComponent<Enemy>();
        }

        private void Update()
        {
            if (_agent == null || _waypoints == null || _waypoints.Length == 0)
                return;

            if (_agent.pathPending)
                return;

            if (_agent.remainingDistance <= reachedDistance)
                GoToNextWaypoint();
        }

        public void Init(PathController path)
        {
            if (path == null)
                return;

            _waypoints = path.Waypoints;
            if (_waypoints == null || _waypoints.Length == 0)
                return;

            if (_agent == null)
                return;

            _agent.autoRepath = true;
            _agent.Warp(_waypoints[0].position);

            if (_waypoints.Length <= 1)
                return;

            _index = 1;
            _agent.SetDestination(_waypoints[_index].position);
        }

        private void GoToNextWaypoint()
        {
            _index++;

            if (_index < _waypoints.Length)
            {
                _agent.SetDestination(_waypoints[_index].position);
                return;
            }

            var gm = GameManager.Instance;
            if (gm != null)
                gm.Resources.ChangeLives(-1, gm.OnLivesDepleted);

            if (_enemy != null)
                _enemy.OnReachedEnd();
            else
                Destroy(gameObject);
        }
    }
}