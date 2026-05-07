using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : NetworkBehaviour
{
    [Header("Configuraciones de Velocidad")]
    public float baseSpeed = 50.0f;
    public float boostMultiplier = 2.0f;
    public float acceleration = 25.0f;
    public float decceleration = 15.0f;
    public float turningSpeed = 100.0f;

    [Header("Física de Flotación")]
    public float hoverHeight = 1.5f; // Súbelo un poco para probar
    public float downForceMultiplier = 10.0f; // Aumentado para estabilidad
    public LayerMask groundLayer; // ASIGNA ESTO EN EL INSPECTOR (Capa de la pista)

    private Rigidbody rb;
    private bool isBoosting = false;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        // Si no somos el dueño, dejamos que NetworkRigidbody/Transform manejen la posición
        if (!IsOwner)
        {
            rb.isKinematic = true;
            enabled = false;
        }


    }

    private void Update()
    {
        // Solo el dueño del objeto lee el teclado
        if (!IsOwner) return;

        isBoosting = Input.GetKey(KeyCode.LeftShift);
    }
    private void FixedUpdate()
    {
        // Solo el dueño del objeto aplica movimiento físico
        if (!IsOwner) return;

        HandleMovement();
        HandleTurning();
        HandleDownforce();
    }

    private void HandleMovement()
    {
        // Ambos usan "Vertical", pero cada quien en su propia computadora
        float moveInput = Input.GetAxis("Vertical");

        float targetVel = isBoosting ? baseSpeed * boostMultiplier : baseSpeed;
        Vector3 desiredVelocity = (moveInput != 0) ? transform.forward * moveInput * targetVel : Vector3.zero;

        rb.velocity = Vector3.Lerp(rb.velocity, desiredVelocity, acceleration * Time.fixedDeltaTime);
    }

    private void HandleTurning()
    {
        float turnInput = Input.GetAxis("Horizontal");
        float turnAmount = turnInput * turningSpeed * Time.fixedDeltaTime;

        // Girar usando rotación física
        Quaternion turnRotation = Quaternion.Euler(0f, turnAmount, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    private void HandleDownforce()
    {
        // Lanzamos el rayo ignorando la capa del jugador
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, hoverHeight + 2f, groundLayer))
        {
            float distance = hit.distance;

            // Fuerza proporcional para mantener la altura
            float error = hoverHeight - distance;
            float force = error * 20f;

            rb.AddForce(Vector3.up * force, ForceMode.Acceleration);
        }
        else
        {
            // Gravedad artificial si está en el aire
            rb.AddForce(Vector3.down * downForceMultiplier, ForceMode.Acceleration);
        }
    }


}