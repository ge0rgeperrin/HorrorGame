using Mirror;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    private GameObject offlineplayer;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        offlineplayer = GameObject.FindGameObjectWithTag("Player");
        offlineplayer.GetComponent<FPSController>().Mesh.SetActive(false);
        this.GetComponentInChildren<CapsuleCollider>().enabled = false; //we don't want to collide with our own player
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
