using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerOnlyRigidbody : NetworkBehaviour
{
    public override void OnStartClient()
    {
        if (isClientOnly)
        {
            Destroy(GetComponent<Rigidbody>());
        }
    }
}
