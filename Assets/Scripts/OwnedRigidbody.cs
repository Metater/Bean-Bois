using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OwnedRigidbody : NetworkBehaviour
{
    [Server]
    public void OnOwnerDisconnect()
    {
        print("IT WORKED!!!");
    }
}
