using UnityEngine;

namespace Level
{
    public class PathController : MonoBehaviour
    {
        [SerializeField] private Transform[] waypoints;

        public Transform[] Waypoints => waypoints;

        private void Awake()
        {
            if (waypoints is { Length: > 0 })
                return;

            waypoints = new Transform[transform.childCount];

            for (var i = 0; i < transform.childCount; i++)
                waypoints[i] = transform.GetChild(i);
        }
    }
}