using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;

[RequireComponent(typeof(CharacterController))]
public class Goober : MonoBehaviour
{
    public static Goober instance;
    [SerializeField] float moveSpeed;

    [field: SerializeField, HideInInspector] public CharacterController Controller { get; private set; }

    Navi.Path path = new();
    NavMeshPath navMeshPath;
    enum GooberState
    {
        Idle,
        Traversing,
        Harvesting,
    }
    GooberState state = GooberState.Idle;

    ResourceNode targetNode;

    Vector3 _destination;
    private Vector3 Destination
    {
        get
        {
            return _destination;
        }
        set
        {
            _destination = value;
            navMeshPath = new();
            if (NaviOrUnity.state == NaviOrUnity.State.Navi)
            {
                path = Navi.FindPath(transform.position, Destination);
            }
            else
            {
                NavMesh.CalculatePath(transform.position, Destination, 0b1111111111111111111111111111111, navMeshPath);
                path.points = navMeshPath.corners;
            }
        }
    }

    public void GoTo(Vector3 position)
    {
        Destination = position;
        state = GooberState.Traversing;
    }
    public void Harvest(Vector3 position, ResourceNode node)
    {
        Destination = position;
        state = GooberState.Harvesting;
        targetNode = node;
    }
    private void Awake()
    {
        instance = this;
    }
    private void Update()
    {
        if (state == GooberState.Idle)
        {
            Move(Vector3.zero);
            return;
        }

        if (state == GooberState.Harvesting && Vector3.Distance(transform.position, Destination) < (NaviOrUnity.state == NaviOrUnity.State.Navi ? 0.9f : 0.5f))
        {
            targetNode.Harvest();
            PlayerInfo.instance.Add(targetNode.Resource, 1);
            state = GooberState.Idle;
            return;
        }

        if (state == GooberState.Traversing && Vector3.Distance(transform.position, Destination) < moveSpeed * Time.deltaTime * 2)
        {
            transform.position = Destination;
            state = GooberState.Idle;
            return;
        }


        Vector3 steer = Vector3.zero;

        if (path != null && path.points != null && path.points.Length != 0)
        {
            int line = -1;
            float distance = 0;
            for (int i = 0; i < path.points.Length - 1; i++)
            {
                Vector3 a = path.points[i];
                Vector3 b = path.points[i + 1];

                //find closest point on the line
                Vector3 dir = Vector3.Normalize(b - a);
                float lineLength = Vector3.Magnitude(b - a);

                float dot = Vector3.Dot(dir, transform.position - a);
                dot = Mathf.Clamp(dot, 0, lineLength);
                Vector3 closestPoint = a + dir * dot;

                float distanceFromPoint = Vector3.Magnitude(transform.position - closestPoint);

                if (dot < lineLength && (line == -1 || distance >= distanceFromPoint))
                {
                    line = i;
                    distance = distanceFromPoint;
                }
            }

            steer += Vector3.Normalize(path.points[line + 1] - transform.position);
        }
        //steer += Vector3.Normalize(Destination - transform.position);

        Move(steer);
    }

    private void Move(Vector3 steerDirection)
    {
        steerDirection.y = 0;
        steerDirection.Normalize();

        Vector3 movement = moveSpeed * steerDirection;
        Vector3 gravity = Physics.gravity;
        Controller.Move((movement + gravity) * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        if (path == null || path.points == null || path.points.Length == 0)
            return;
        for (int i = 0; i < path.points.Length; i++)
        {
            Gizmos.DrawSphere(path.points[i], 0.1f);
        }
        
    }


    public void Find(Resource resource)
    {
        string notificationMessage = $"Did multitarget search using " + (NaviOrUnity.state == NaviOrUnity.State.Navi ? "custom pathfinding solution." : "unity's pathfinding.");
        System.Diagnostics.Stopwatch sw = new();
        sw.Restart();
        if (NaviOrUnity.state == NaviOrUnity.State.Navi)
        {
            List<ResourceNode> nodes = Resource.harvestableNodes[resource];
            Vector3[] positions = new Vector3[nodes.Count];
            int i = 0;
            foreach (var item in nodes)
            {
                positions[i] = item.transform.position;
                i++;
            }
            path = Navi.FindPathToClosest(transform.position, positions);
            targetNode = nodes.Find((x) => x.transform.position == path.points[^1]);
            Destination = path.points[^1];
        }
        else
        {
            float shortestPathLength = -1;
            NavMeshPath shortestPath = null;
            ResourceNode closestNode = null;
            Vector3 dest = Vector3.zero;
            for (int i = 0; i < Resource.harvestableNodes[resource].Count; i++)
            {
                ResourceNode node = Resource.harvestableNodes[resource][i];


                NavMesh.SamplePosition(node.transform.position, out NavMeshHit myNavHit, 100, -1);
                Vector3 nodePosition = myNavHit.position;

                NavMeshPath newPath = new();
                NavMesh.CalculatePath(transform.position, nodePosition, 0b1111111111111111111111111111111, newPath);
                float pathLength = 0;
                for (int ii = 0; ii < newPath.corners.Length - 1; ii++)
                {
                    pathLength += Vector3.Distance(newPath.corners[ii], newPath.corners[ii + 1]);
                }
                
                if (i == 0 || pathLength < shortestPathLength)
                {
                    shortestPath = newPath;
                    shortestPathLength = pathLength;
                    closestNode = node;
                    dest = nodePosition;
                }
            }
            navMeshPath = shortestPath;
            path.points = navMeshPath.corners;
            targetNode = closestNode;
            Destination = dest;
        }
        sw.Stop();
        notificationMessage += $" Taking {sw.ElapsedMilliseconds} milliseconds.";
        Notification.Notify(notificationMessage);
        state = GooberState.Harvesting;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        Controller = GetComponent<CharacterController>();
    }
#endif
}
