using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkUI _scriptNetUI;
    [Header("Puntos de Aparición")]
    public Transform[] spawnPoints;

    public List<GameObject> _allPlayers = new List<GameObject>();

    [Header("Ajustes Visuales")]
    public Color[] _playerColors;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            if (IsHost) OnClientConnected(NetworkManager.Singleton.LocalClientId);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var clientData))
        {
            NetworkObject playerNetworkObject = clientData.PlayerObject;
            if (playerNetworkObject != null)
            {
                GameObject playerObj = playerNetworkObject.gameObject;
                if (!_allPlayers.Contains(playerObj))
                {
                    _allPlayers.Add(playerObj);
                    int playerIndex = _allPlayers.Count - 1;
                    SetPlayers(playerIndex);
                    CheckMatchReady();
                }

                // Sincronización de color
                int index = _allPlayers.Count - 1;
                if (index < _playerColors.Length)
                {
                    PlayerMove playerScript = playerObj.GetComponent<PlayerMove>();
                    if (playerScript != null)
                    {
                        playerScript.PlayerColor.Value = _playerColors[index];
                    }
                }
            }
        }
    }

    private void CheckMatchReady()
    {
        if (_allPlayers.Count == 2)
        {
            // Iniciamos la corrutina SOLO en el servidor
            StartCoroutine(StartRaceRoutine());
        }
    }

    // Corrutina que corre en el Servidor para controlar los tiempos
    private IEnumerator StartRaceRoutine()
    {
        // 1. Avisamos a todos que se preparen (Sets)
        GameSetsClientRpc();

        yield return new WaitForSeconds(2f); // Espera de 2 segundos para asegurar carga

        // 2. Avisamos a todos que inicien (Starts)
        GameStartsClientRpc();
    }

    [ClientRpc]
    private void GameSetsClientRpc()
    {
        // Ocultar menú en todos los clientes
        if (_scriptNetUI != null && _scriptNetUI._menuParent != null)
            _scriptNetUI._menuParent.SetActive(false);

        // Cada jugador local configura su propia vista o estado inicial si es necesario
        Debug.Log("<color=yellow>Configurando partida...</color>");
    }

    [ClientRpc]
    private void GameStartsClientRpc()
    {
        Debug.Log("<color=green>¡CARRERA INICIADA!</color>");

        // BUSQUEDA DIRECTA: Como la lista del cliente está vacía, 
        // buscamos todos los PlayerMove que existan en la escena del cliente
        PlayerMove[] allMoves = GameObject.FindObjectsByType<PlayerMove>(FindObjectsSortMode.None);

        foreach (PlayerMove move in allMoves)
        {
            move._available = true;

            // Opcional: Aplicar color visualmente aquí también si no usas NetworkVariable
            // Nota: Es mejor que el color se maneje por NetworkVariable para que sea automático
        }
    }

    public void SetPlayers(int playerID)
    {
        if (playerID < _allPlayers.Count && playerID < spawnPoints.Length)
        {
            _allPlayers[playerID].transform.position = spawnPoints[playerID].position;
            _allPlayers[playerID].transform.rotation = spawnPoints[playerID].rotation;
        }
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
}