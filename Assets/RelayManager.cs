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
    public static RelayManager Instance { get; private set; }

    [Header("Configuración de Pruebas")]
    [Tooltip("Si está marcado, usa Relay (Internet). Si no, usa IP Local (127.0.0.1)")]
    public bool useRelay = true;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        else { Instance = this; }
    }

    async void Start()
    {
        // Solo inicializamos servicios si vamos a usar Relay
        if (useRelay)
        {
            try
            {
                await UnityServices.InitializeAsync();
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }
                Debug.Log("Servicios Online Listos.");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error de Autenticación: " + e.Message);
            }
        }
        else
        {
            Debug.Log("Modo de Prueba Local Activo (Sin Relay)");
        }
    }

    public async Task<string> CreateRelay()
    {
        if (useRelay)
        {
            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(5);
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetRelayServerData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData,
                    allocation.ConnectionData,
                    true
                );

                NetworkManager.Singleton.StartHost();
                return joinCode;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error en Relay Host: " + e.Message);
                return null;
            }
        }
        else
        {
            // MODO LOCAL
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData("127.0.0.1", 7777); // Forzamos IP local
            NetworkManager.Singleton.StartHost();
            return "MODO_LOCAL";
        }
    }

    public async void JoinRelay(string joinCode)
    {
        if (useRelay)
        {
            try
            {
                string cleanCode = joinCode.Trim().ToUpper();
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(cleanCode);

                UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetRelayServerData(
                    joinAllocation.RelayServer.IpV4,
                    (ushort)joinAllocation.RelayServer.Port,
                    joinAllocation.AllocationIdBytes,
                    joinAllocation.Key,
                    joinAllocation.ConnectionData,
                    joinAllocation.HostConnectionData,
                    true
                );

                NetworkManager.Singleton.StartClient();
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error en Relay Join: " + e.Message);
            }
        }
        else
        {
            // MODO LOCAL
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData("127.0.0.1", 7777);
            NetworkManager.Singleton.StartClient();
        }
    }
}