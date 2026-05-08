using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI; // Añadido por si usas botones estándar

public class NetworkUI : MonoBehaviour
{
    [Header("Referencias de UI")]
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private TextMeshProUGUI codeDisplayText;

    [Header("Botones (Opcional)")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;

    [SerializeField] private GameObject _menuParent;

    private void Start()
    {
        // Limpiamos el texto al iniciar
        if (codeDisplayText != null) codeDisplayText.text = "Esperando...";
    }

    // Se asigna al evento OnClick del botón "Host" en el Inspector
    public async void ClickHost()
    {
        if (RelayManager.Instance == null)
        {
            Debug.LogError("No se encontró el RelayManager en la escena.");
            return;
        }

        codeDisplayText.text = "Generando código...";

        // El código de 6 letras se pide a los servidores de Unity
        string code = await RelayManager.Instance.CreateRelay();

        if (!string.IsNullOrEmpty(code))
        {
            codeDisplayText.text = "CÓDIGO: " + code;
            Debug.Log("Código para compartir: " + code);
            _menuParent.SetActive(false);
        }
        else
        {
            codeDisplayText.text = "Error al crear Relay";
        }
    }

    // Se asigna al evento OnClick del botón "Client" en el Inspector
    public void ClickClient()
    {
        if (RelayManager.Instance == null) return;

        string code = joinCodeInput.text;

        if (!string.IsNullOrEmpty(code) && code.Length >= 6)
        {
            RelayManager.Instance.JoinRelay(code);
            codeDisplayText.text = "Uniéndose...";
            _menuParent.SetActive(false);
        }
        else
        {
            Debug.LogError("El código debe tener al menos 6 caracteres.");
            codeDisplayText.text = "Código Inválido";
        }
    }
}