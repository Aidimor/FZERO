using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkUI _scriptNetUI;
    [Header("Puntos de Aparición")]
    [SerializeField] private Transform[] spawnPoints;

    // Esta lista ahora se llenará automáticamente
    public List<GameObject> _allPlayers = new List<GameObject>();
    public int _onPlayerID;

    [Header("Ajustes Visuales")]
    public Color[] _playerColors;

    public void Start()
    {
        _scriptNetUI.ClickHostLocal();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            if (IsHost)
            {
                Debug.Log("<color=yellow><b>[SISTEMA]: Se ha iniciado el Servidor. ¡PLAYER 1 (Host) detectado!</b></color>");
                OnClientConnected(NetworkManager.Singleton.LocalClientId);
            }
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        int playerNumber = NetworkManager.Singleton.ConnectedClientsList.Count;

        if (playerNumber > 1)
        {
            Debug.Log($"<color=cyan><b>[SISTEMA]: ¡PLAYER {playerNumber} (Cliente) conectado! ID: {clientId}</b></color>");
        }

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var clientData))
        {
            NetworkObject playerNetworkObject = clientData.PlayerObject;

            if (playerNetworkObject != null)
            {
                // --- AGREGAR A LA LISTA ---
                GameObject playerObj = playerNetworkObject.gameObject;
                if (!_allPlayers.Contains(playerObj)) // Evitamos duplicados
                {
                    _allPlayers.Add(playerObj);
                    SetPlayers();
                    //Debug.Log($"<color=green>Jugador {clientId} añadido a la lista _allPlayers. Total: {_allPlayers.Count}</color>");
                }
                // --------------------------

                int index = playerNumber - 1;

                // Posicionamiento
                if (index < spawnPoints.Length)
                {
                    playerNetworkObject.transform.position = spawnPoints[index].position;
                    playerNetworkObject.transform.rotation = spawnPoints[index].rotation;
                }

                // Color
                if (index < _playerColors.Length)
                {
                    PlayerMove playerScript = playerNetworkObject.GetComponent<PlayerMove>();
                    if (playerScript != null)
                    {
                        playerScript.PlayerColor.Value = _playerColors[index];
                    }
                }
            }
        }
    }

    // Es buena práctica limpiar la lista si el servidor se apaga
    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            _allPlayers.Clear();
        }
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    public void SetPlayers()
    {
        _allPlayers[_onPlayerID].transform.position = spawnPoints[_onPlayerID].transform.position;
        _allPlayers[_onPlayerID].SetActive(false);
    }
}