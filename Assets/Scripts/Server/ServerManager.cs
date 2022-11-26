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
    public void SetText(string text)
    {
        foreach (var player in manager.PlayerLookup.Refs)
        {
            player.text = text;
        }
    }
}
