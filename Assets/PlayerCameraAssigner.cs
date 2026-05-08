using Unity.Netcode;
using UnityEngine;

public class PlayerCameraAssigner : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            CameraFollow cam = FindFirstObjectByType<CameraFollow>();
            if (cam != null) cam.SetTarget(this.transform.GetComponent<PlayerMove>()._camaraPos);
        }
    }
}