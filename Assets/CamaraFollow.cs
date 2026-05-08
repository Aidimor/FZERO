using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public int _cameraID;
    public Transform target;
    public Vector3 offset = new Vector3(0, 3, -7);
    [Range(1f, 20f)] public float smoothSpeed = 10f;
    public float rotationSpeed = 5f;

    public void SetTarget(Transform playerTransform)
    {
        target = playerTransform;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.TransformPoint(offset);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        Vector3 lookAtPos = target.position + target.forward * 5f;
        Quaternion targetRotation = Quaternion.LookRotation(lookAtPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}