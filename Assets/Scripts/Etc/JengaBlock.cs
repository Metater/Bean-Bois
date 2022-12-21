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
        if (isServer &&
            (collision.gameObject.CompareTag("Lava") ||
            collision.gameObject.CompareTag("SpectatorBox")))
        {
            NetworkServer.Destroy(gameObject);
        }
    }
}
