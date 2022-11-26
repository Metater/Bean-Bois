using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerManager : NetworkBehaviour
{
    private GameManager manager;

    private void Awake()
    {
        manager = FindObjectOfType<GameManager>(true);
    }

    [Server]
    public void SetMainText(string newMainText)
    {
        foreach (var player in manager.PlayerLookup.Refs)
        {
            player.configurables.mainText = newMainText;
        }
    }
}
