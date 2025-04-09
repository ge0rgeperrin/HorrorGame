using Mirror;
using Subterranea;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    private GameObject offlineplayer;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        
        PlayerController.Instance.Mesh.SetActive(false);
        GetComponentInChildren<CapsuleCollider>().enabled = false; 
    }

    private void Update()
    {
        if (isLocalPlayer)
        {
            this.transform.position = offlineplayer.transform.position;
            this.transform.rotation = offlineplayer.transform.rotation;
        }
    }
}
