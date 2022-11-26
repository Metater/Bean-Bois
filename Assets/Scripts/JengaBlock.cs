using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JengaBlock : NetworkBehaviour
{
    private void Update()
    {
        if (isServer && transform.position.y < 0)
        {
            NetworkDestroy();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isServer && (collision.gameObject.CompareTag("Lava") || collision.gameObject.CompareTag("SpectatorBox")))
        {
            NetworkDestroy();
        }
    }

    [Server]
    public void NetworkDestroy()
    {
        NetworkServer.Destroy(gameObject);
    }
}
