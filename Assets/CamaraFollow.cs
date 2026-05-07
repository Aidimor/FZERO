using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 3, -7); // Ajustado para mejor visibilidad
    [Range(1f, 20f)]
    public float smoothSpeed = 10f; // Ahora es una fuerza de seguimiento
    public float rotationSpeed = 5f;

    void FixedUpdate() // Cambiamos a FixedUpdate para seguir al Rigidbody
    {
        if (target == null)
        {
            FindLocalPlayer();
            return;
        }

        // 1. Calculamos la posición deseada en el espacio local del jugador
        Vector3 desiredPosition = target.TransformPoint(offset);

        // 2. Usamos Lerp para suavizar la posición
        // Multiplicamos por Time.fixedDeltaTime para que sea independiente de los FPS
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.fixedDeltaTime);

        // 3. Rotación suave (importante para que no vibre la vista)
        Vector3 lookAtPos = target.position + target.forward * 10f;
        Quaternion targetRotation = Quaternion.LookRotation(lookAtPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
    }

    void FindLocalPlayer()
    {
        PlayerMove[] players = FindObjectsOfType<PlayerMove>();
        foreach (PlayerMove p in players)
        {
            if (p.IsOwner)
            {
                target = p.transform;
                break;
            }
        }
    }
}