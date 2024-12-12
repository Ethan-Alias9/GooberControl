using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


//ToDo:
//Don't out cursor in the ground
//Make sure there is ground for cursor to be on

[DisallowMultipleComponent, RequireComponent(typeof(Camera)), ExecuteAlways]
public class OmniscientController : MonoBehaviour
{
    [field: SerializeField] public List<Collider> ConstrainedColliders { get; private set; }
    [field: SerializeField] public float CameraSpeed { get; private set; }
    [field: SerializeField] public float CameraHeight { get; private set; }
    [field: SerializeField] public float GroundCheckDistance { get; private set; }
    [field: SerializeField] public float CameraRotateSpeed { get; private set; }




    public Cursor cursor;


    [field: SerializeField, HideInInspector] public Camera Camera { get; private set; }

    OmniscientControllerInput input;


    [SerializeField] private LayerMask cursorBlock;

    [SerializeField] Goober controlledGoober;
    [SerializeField] float cursorSpeed;
    [SerializeField] private LayerMask cameraHoverMask;

    enum State
    {
        GoTo,
    }
    State state;

    private void Awake()
    {
        input = new();
        
        input.Camera.Enable();
        

        input.GoTo.Enable();

        input.GoTo.SetDestination.performed += SetDestination;


        input.Cursor.Enable();



        state = State.GoTo;
    }
    private void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            ValidatePosition();
            return;
        }
#endif
        CameraUpdate();
        CursorUpdate();
        switch (state)
        {
            case State.GoTo:
                GoToUpdate();
                break;
        }
    }

    private void CameraUpdate()
    {
        Vector2 inputDirection = input.Camera.MoveDirection.ReadValue<Vector2>();
        Vector3 movementDirection = inputDirection.ToVector3XZ().normalized;
        float moveMagnitude = Mathf.Clamp(inputDirection.magnitude, 0, 1);

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, GroundCheckDistance, cameraHoverMask.value, QueryTriggerInteraction.Ignore) && hit.normal.y > 0)
        {
            float desiredHeight = hit.point.y + CameraHeight;
            transform.position = new Vector3(transform.position.x, Mathf.MoveTowards(transform.position.y, desiredHeight, CameraSpeed * Time.deltaTime), transform.position.z);
        }

        Vector3 forwards = transform.forward;
        forwards.y = 0;
        if (forwards == Vector3.zero)
            forwards = forwards.y < 0 ? transform.up : -transform.up;
        transform.position += Quaternion.FromToRotation(Vector3.forward, forwards) * (CameraSpeed * moveMagnitude * Time.deltaTime * movementDirection);

        ValidatePosition();

        transform.Rotate(Vector3.up, CameraRotateSpeed * input.Camera.Rotate.ReadValue<float>() * Time.deltaTime, Space.World);
    }

    public void ValidatePosition()
    {
        if (ConstrainedColliders == null || ConstrainedColliders.Count == 0)
            return;

        Vector3 closestPoint = ConstrainedColliders[0].ClosestPoint(transform.position);
        float closestDistance = Vector3.Magnitude(closestPoint - transform.position);
        for (int i = 1; i < ConstrainedColliders.Count; i++)
        {
            if (closestPoint == transform.position)
                return;
            Vector3 point = ConstrainedColliders[i].ClosestPoint(transform.position);
            float distance = Vector3.Magnitude(transform.position - point);
            if (distance < closestDistance)
            {
                closestPoint = point;
                closestDistance = distance;
            }
        }
        transform.position = closestPoint;
    }

    void GoToUpdate()
    {
        
    }

    public void CursorUpdate()
    {
        Vector2 cursorMove = input.Cursor.Move.ReadValue<Vector2>();
        if (cursorMove != Vector2.zero)
            Mouse.current.WarpCursorPosition(Mouse.current.position.ReadValue() + cursorMove * (Time.deltaTime * cursorSpeed));

        Vector3Int cursorCell = GetCursorCell(cursorBlock, out bool success, out _);
        Bounds cellBounds = LevelGrid.CellBounds(cursorCell);

        if (!success)
        {
            cursor.gameObject.SetActive(false);
            return;
        }
        
        cursor.GoTo(LevelGrid.CellPosition(cursorCell));
        Ray ray = new(new Vector3(cellBounds.center.x, cellBounds.min.y + LevelGrid.Size.y * 0.05f, cellBounds.center.z), Vector3.down);

        bool validPlace = Physics.Raycast(ray, LevelGrid.Size.y, cursorBlock.value, QueryTriggerInteraction.Ignore)
            && Physics.OverlapBox(LevelGrid.CellPosition(cursorCell), LevelGrid.Size * 0.25f, Quaternion.identity, cursorBlock.value, QueryTriggerInteraction.Ignore).Length == 0;
        cursor.gameObject.SetActive(validPlace);

    }


    private void SetDestination(InputAction.CallbackContext context)
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        Vector3Int destination = GetCursorCell(cursorBlock, out bool success, out ResourceNode resource);
        if (!success)
        {
            Debug.Log("Unsuccessful");
            return;
        }
        Bounds bounds = LevelGrid.CellBounds(destination);
        if (resource == null)
        {
            Debug.Log("No RESOURCE");
            controlledGoober.GoTo(new (bounds.center.x, bounds.min.y, bounds.center.z));
        }
        else
        {
            Debug.Log("HARVEST");
            if (NavMesh.SamplePosition(resource.transform.position, out NavMeshHit myNavHit, 100, -1))
                controlledGoober.Harvest(myNavHit.position, resource);
        }
    }

    private Vector3Int GetCursorCell(LayerMask layerMask, out bool success, out ResourceNode resource)
    {
        Ray cursorRay = Camera.ScreenPointToRay(Input.mousePosition);

        success = Physics.Raycast(cursorRay, out RaycastHit hit, Mathf.Infinity, layerMask.value, QueryTriggerInteraction.Ignore);
        resource = null;
        if (!success)
            return Vector3Int.zero;

        Vector3 cursorPos = hit.point;

        Vector3Int cell = LevelGrid.Cell(cursorPos);
        Bounds cellBounds = LevelGrid.CellBounds(cell);

        Vector3 min = cellBounds.min;
        Vector3 max = cellBounds.max;

        if (cursorRay.direction.x > 0 && Mathf.Approximately(min.x, cursorPos.x))
            cell.x--;
        else if (cursorRay.direction.x < 0 && Mathf.Approximately(max.x, cursorPos.x))
            cell.x++;
        if (cursorRay.direction.y > 0 && Mathf.Approximately(min.y, cursorPos.y))
            cell.y--;
        else if (cursorRay.direction.y < 0 && Mathf.Approximately(max.y, cursorPos.y))
            cell.y++;
        if (cursorRay.direction.z > 0 && Mathf.Approximately(min.z, cursorPos.z))
            cell.z--;
        else if (cursorRay.direction.z < 0 && Mathf.Approximately(max.z, cursorPos.z))
            cell.z++;

        Collider[] colliders = Physics.OverlapBox(cellBounds.center, cellBounds.extents * 0.9f, Quaternion.identity, LayerMask.GetMask("Resource"));
        if (colliders.Length > 0)
        {
            resource = colliders[0].GetComponent<ResourceNode>();
        }

        return cell;
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        Camera = GetComponent<Camera>();


        for (int i = 0; i < ConstrainedColliders.Count; i++)
        {
            if (ConstrainedColliders[i] != null)
                continue;
            ConstrainedColliders.RemoveAt(i);
            i--;
        }
    }
#endif
}
