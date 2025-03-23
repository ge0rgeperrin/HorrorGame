using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicPCMovement : NetworkBehaviour
{
    public float speed = 5;
    public float mouseSensitivity = 2f;

    public override void OnStartClient()
    {
        if (isServer)
            name = $"Player[{netIdentity.connectionToClient.connectionId}|server]";
        else
            name = $"Player[{netIdentity.connectionToClient.connectionId}|{(isLocalPlayer ? "local" : "remote")}]";
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 dir = new Vector3(h, 0, v);
        transform.position += dir.normalized * (Time.deltaTime * speed);

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);
    }
}
