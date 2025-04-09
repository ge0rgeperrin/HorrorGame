using Mirror;
using Subterranea;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    private GameObject rig => PlayerController.Instance.gameObject;

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
            transform.position = rig.transform.position;
            transform.rotation = rig.transform.rotation;
        }
    }
}
