using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public int _cameraID;
    public Transform target;
    public Vector3 offset = new Vector3(0, 3, -7);

    [Range(1f, 100f)] public float smoothSpeed = 10f;
    public float rotationSpeed = 5f;

    // Usamos FixedUpdate si tu jugador usa RigidBody, 
    // o LateUpdate si el jugador se mueve por Transform/NetworkTransform.
    // LateUpdate suele ser el estándar para cámaras.
    //void LateUpdate()
    //{
    //    if (target == null) return;

    //    // 1. POSICIÓN: Calculamos la posición deseada
    //    // Usar TransformPoint está bien, pero el suavizado debe ser consistente
    //    Vector3 desiredPosition = target.TransformPoint(offset);

    //    // Usamos SmoothDamp o un Lerp corregido. 
    //    // Para evitar vibración, el factor de suavizado no debe ser demasiado alto.
    //    transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

    //    // 2. ROTACIÓN: Mirar hacia el objetivo
    //    // En lugar de calcular un punto adelante, miramos directamente al target 
    //    // con un pequeño ajuste de altura para que no apunte a los pies.
    //    Vector3 lookAtTarget = target.position + Vector3.up * 1.5f;
    //    Vector3 direction = lookAtTarget - transform.position;

    //    if (direction != Vector3.zero)
    //    {
    //        Quaternion targetRotation = Quaternion.LookRotation(direction);
    //        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    //    }
    //}

    public void SetTarget(Transform playerTransform)
    {
        target = playerTransform;
        // Reset inmediato de posición para evitar que la cámara viaje por todo el mapa al iniciar
        if (target != null)
        {
            transform.position = target.TransformPoint(offset);
            transform.LookAt(target.position + Vector3.up * 1.5f);
        }
    }

    public void Update()
    {
        if (target == null) return;
        transform.position = target.transform.position;
    }
}