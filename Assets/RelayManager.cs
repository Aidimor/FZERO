using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    // Singleton para acceder desde otros scripts fácilmente
    public static RelayManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        else { Instance = this; }
    }

    async void Start()
    {
        try
        {
            // Inicialización obligatoria de servicios
            await UnityServices.InitializeAsync();

            // Login anónimo necesario para Relay
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            Debug.Log("Servicios de Unity listos.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al iniciar servicios: " + e.Message);
        }
    }

    /// <summary>
    /// Crea una sala de Relay y devuelve el código de unión.
    /// </summary>
    public async Task<string> CreateRelay()
    {
        try
        {
            // 1. Reservamos espacio para 5 jugadores
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(5);

            // 2. Generamos el código de 6 letras
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // 3. Obtenemos el componente de transporte
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            // 4. Configuración manual ajustada a tu versión (7 parámetros)
            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.ConnectionData, // HostConnectionData es el mismo para el creador
                true // IsSecure (DTLS) al final
            );

            // 5. Arrancamos el Host
            NetworkManager.Singleton.StartHost();

            Debug.Log("Host iniciado con código: " + joinCode);
            return joinCode;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al crear Relay: " + e.Message);
            return null;
        }
    }

    /// <summary>
    /// Se une a una sala existente usando el código.
    /// </summary>
    public async void JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Uniéndose a Relay con código: " + joinCode);

            // 1. Validamos código
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            // 2. Configuramos transporte para el cliente
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            transport.SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData, // Datos del Host
                true // IsSecure (DTLS) al final
            );

            // 3. Arrancamos el Cliente
            NetworkManager.Singleton.StartClient();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al unirse al Relay: " + e.Message);
        }
    }
}