using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.Movement
{
    using Pathfinding;
    using UnityEngine.AI;

    public class FollowPathController : MonoBehaviour
    {

        [SerializeField] private PathFinder pathFinder;
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private PathFinding toUsePathFinding = PathFinding.NavMesh;

        [SerializeField] private float speed = 5f;

        private bool isMoving = false;

        private void Start()
        {
            if (agent == null)
                agent = GetComponentInChildren<NavMeshAgent>();
            if (agent == null)
            {
                Debug.LogError("NavMeshAgent component is missing on the GameObject.");
                return;
            }
            agent.updateRotation = false;
        }

        private void Update()
        {
            if (agent.enabled && toUsePathFinding == PathFinding.AStar)
            {
                agent.enabled = false;
            }

            if (!agent.enabled && toUsePathFinding == PathFinding.NavMesh)
            {
                agent.enabled = true;
            }
        }

        public void GoToDestination(Vector3 destination)
        {
            if (!isMoving)
            {
                if (toUsePathFinding == PathFinding.AStar)
                    StartCoroutine(FollowPathCoroutine(pathFinder.CalculatePath(transform.position, destination)));
                else if (toUsePathFinding == PathFinding.NavMesh)
                    agent.SetDestination(destination);
            }
        }

        public void StopMoving()
        {
            if (isMoving)
            {
                StopAllCoroutines();
                isMoving = false;
            }
        }

        IEnumerator FollowPathCoroutine(List<Vector3> path)
        {
            if (path == null || path.Count == 0)
            {
                Debug.Log("No path found");
                yield break;
            }
            isMoving = true;
            for (int i = 0; i < path.Count; i++)
            {
                Vector3 target = path[i];
                // Move towards the target position
                while (Vector3.Distance(transform.position, target) > 0.1f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speed);
                    yield return null;
                }
                //Debug.Log($"Reached target: {target}");
            }
            isMoving = false;
        }

        private enum PathFinding
        {
            NavMesh,
            AStar
        }
    }

}