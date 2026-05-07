using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class NetworkButtons : MonoBehaviour
{
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;

    private void Awake()
    {
        // Al dar clic al botón de Host, arranca el servidor y el jugador local
        hostBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
            HideMenu();
        });

        // Al dar clic al botón de Cliente, busca una partida activa
        clientBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
            HideMenu();
        });
    }

    private void HideMenu()
    {
        // Oculta los botones para que no estorben al jugar
        hostBtn.gameObject.SetActive(false);
        clientBtn.gameObject.SetActive(false);
    }
}