using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    [SerializeField] GridSnap realPos;

    public float followSpeed;

    private void OnEnable()
    {
        transform.position = realPos.transform.position;
        realPos.transform.localPosition = Vector3.zero;
    }

    private void LateUpdate()
    {
        float distance = Vector3.Magnitude(realPos.transform.position - transform.position);
        Vector3 direction = Vector3.Normalize(realPos.transform.position - transform.position);

        float travelDistance = followSpeed * Time.deltaTime;
        travelDistance = travelDistance < distance ? travelDistance : distance;


        Vector3 trackedOldPos = realPos.transform.position;

        transform.position += travelDistance * direction;

        realPos.transform.position = trackedOldPos;
    }

    public void GoTo(Vector3 position)
    {
        realPos.SetPosition(position);
    }
}
