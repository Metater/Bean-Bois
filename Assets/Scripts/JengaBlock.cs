using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JengaBlock : NetworkBehaviour
{
    private void Update()
    {
        if (!isServer)
        {
            return;
        }

        if (transform.position.y < 0)
        {
            NetworkServer.Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isServer && collision.gameObject.CompareTag("Lava"))
        {
            NetworkServer.Destroy(gameObject);
        }
    }
}
