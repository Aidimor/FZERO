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
    public float hoverHeight = 1.5f;
    public float downForceMultiplier = 10.0f;
    public LayerMask groundLayer;

    [Header("Referencias Visuales")]
    public GameObject _playerSphere;

    // VARIABLE DE RED: Sincroniza el color automáticamente
    // El servidor escribe (el Spawner), todos leen.
    public NetworkVariable<Color> PlayerColor = new NetworkVariable<Color>(
        Color.white,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private Rigidbody rb;
    private bool isBoosting = false;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        // 1. Suscribirse al evento de cambio de color
        PlayerColor.OnValueChanged += OnColorChanged;

        // 2. Aplicar el color que tenga la variable actualmente (importante para late-joiners)
        ApplyColor(PlayerColor.Value);

        // Configuración de autoridad
        if (!IsOwner)
        {
            rb.isKinematic = true;
            enabled = false;
        }
    }

    // Se ejecuta cada vez que la variable cambia en la red
    private void OnColorChanged(Color previousValue, Color newValue)
    {
        ApplyColor(newValue);
    }

    private void ApplyColor(Color colorToApply)
    {
        if (_playerSphere != null)
        {
            Renderer renderer = _playerSphere.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Usamos .material para crear una instancia propia y no pintar a todos los jugadores
                renderer.material.color = colorToApply;
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        // Limpieza de eventos al destruir el objeto
        PlayerColor.OnValueChanged -= OnColorChanged;
    }

    private void Update()
    {
        if (!IsOwner) return;
        isBoosting = Input.GetKey(KeyCode.LeftShift);
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        HandleMovement();
        HandleTurning();
        HandleDownforce();
    }

    private void HandleMovement()
    {
        float moveInput = Input.GetAxis("Vertical");
        float targetVel = isBoosting ? baseSpeed * boostMultiplier : baseSpeed;
        Vector3 desiredVelocity = (moveInput != 0) ? transform.forward * moveInput * targetVel : Vector3.zero;

        // Suavizado de velocidad
        rb.velocity = Vector3.Lerp(rb.velocity, desiredVelocity, acceleration * Time.fixedDeltaTime);
    }

    private void HandleTurning()
    {
        float turnInput = Input.GetAxis("Horizontal");
        float turnAmount = turnInput * turningSpeed * Time.fixedDeltaTime;

        Quaternion turnRotation = Quaternion.Euler(0f, turnAmount, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    private void HandleDownforce()
    {
        RaycastHit hit;
        // Raycast hacia abajo para detectar el suelo
        if (Physics.Raycast(transform.position, -transform.up, out hit, hoverHeight + 2f, groundLayer))
        {
            float distance = hit.distance;
            float error = hoverHeight - distance;
            float force = error * 20f;

            rb.AddForce(Vector3.up * force, ForceMode.Acceleration);
        }
        else
        {
            rb.AddForce(Vector3.down * downForceMultiplier, ForceMode.Acceleration);
        }
    }
}