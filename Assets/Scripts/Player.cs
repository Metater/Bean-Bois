using Mirror;
using Mirror.Examples.Benchmark;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetworkBehaviour
{
    #region Fields
    [Header("General")]
    private GameManager manager;
    public PlayerMovement playerMovement;
    [SerializeField] private List<GameObject> invisibleToSelf;
    private bool isCursorVisible = false;
    #endregion Fields

    #region Unity Callbacks
    private void Awake()
    {
        manager = FindObjectOfType<GameManager>(true);
    }
    #endregion Unity Callbacks

    #region Mirror Callbacks
    public override void OnStartClient()
    {
        if (isLocalPlayer)
        {
            // Position own camera
            Camera.main.transform.SetParent(transform);
            Camera.main.transform.localPosition = new Vector3(0, 1.6f, 0);

            // Make own GameObjects invisible
            invisibleToSelf.ForEach(go => go.SetActive(false));

            manager.SetLocalPlayer(this);
        }
    }
    public override void OnStartServer()
    {
        manager.PlayerLookup.TryAdd(netId, this);
    }
    public override void OnStopClient()
    {
        base.OnStopClient();
    }
    #endregion Mirror Callbacks
}
