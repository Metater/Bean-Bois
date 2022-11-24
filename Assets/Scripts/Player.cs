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

        playerMovement.PlayerAwake();

        UpdateCursorVisibility();
    }
    private void Start()
    {
        playerMovement.PlayerStart();
    }
    private void Update()
    {
        playerMovement.PlayerUpdate();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isCursorVisible = !isCursorVisible;
            UpdateCursorVisibility();
        }
    }
    #endregion Unity Callbacks

    #region Mirror Callbacks
    public override void OnStartLocalPlayer()
    {
        // Position own camera
        Camera.main.transform.SetParent(transform);
        Camera.main.transform.localPosition = new Vector3(0, 1.6f, 0);

        // Make own GameObjects invisible
        invisibleToSelf.ForEach(go => go.SetActive(false));

        manager.SetLocalPlayer(this);
    }
    public override void OnStartServer()
    {
        manager.PlayerLookup.TryAdd(netId, this);
    }
    public override void OnStopClient()
    {
        manager.PlayerLookup.TryRemoveWithNetId(netId, out _);
    }
    public override void OnStopServer()
    {
        manager.PlayerLookup.TryRemoveWithNetId(netId, out _);
    }
    #endregion Mirror Callbacks

    private void UpdateCursorVisibility()
    {
        if (isCursorVisible)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        Cursor.visible = isCursorVisible;
    }

    public interface IPlayerCallbacks
    {
        public void PlayerAwake();
        public void PlayerStart();
        public void PlayerUpdate();
    }
}
