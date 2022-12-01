using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEvents : PlayerComponent
{
    public event Action OnStartRound;
    [Server]
    public void ServerStartRound()
    {

    }

    #region StopRound
    public event Action OnStopRound;
    [Server]
    public void ServerStopRound()
    {
        if (isServerOnly)
        {
            OnStopRound?.Invoke();
        }
        RpcStopRound();
    }
    [ClientRpc]
    private void RpcStopRound()
    {
        OnStopRound?.Invoke();
    }
    #endregion StopRound
}