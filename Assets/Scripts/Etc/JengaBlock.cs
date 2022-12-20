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
            NetworkServer.Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        bool isLava = collision.gameObject.CompareTag("Lava");
        bool isSpectatorBox = collision.gameObject.CompareTag("SpectatorBox");
        if (isServer && (isLava || isSpectatorBox))
        {
            NetworkServer.Destroy(gameObject);
        }
    }
}
