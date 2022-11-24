using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Items will be deleted when client that last touched them disconnects, not good
*/

public class GameManager : NetworkBehaviour
{
    public Player LocalPlayer { get; private set; }
    public NetRefLookup<Player> PlayerLookup { get; private set; }

    #region Unity Callbacks
    private void Awake()
    {
        PlayerLookup = new();
    }
    #endregion Unity Callbacks

    /*
    [Header("Prefabs")]
    [SerializeField] private ProjectileTrail projectileTrailPrefab;

    public void SpawnProjectileTrail(Vector3 a, Vector3 b, bool hitObject, Vector3 hitNormal)
    {
        var projectileTrail = Instantiate(projectileTrailPrefab, Vector3.zero, Quaternion.identity);
        projectileTrail.Init(a, b, hitObject, hitNormal);
    }
    */

    #region Reference Management
    public void SetLocalPlayer(Player localPlayer)
    {
        LocalPlayer = localPlayer;
    }
    #endregion Reference Management
}
