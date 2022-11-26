using Mirror;
using Mirror.Examples.Benchmark;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class Player : NetworkBehaviour
{
    #region Fields
    private GameManager manager;

    [Header("General")]
    public PlayerMovement movement;
    public PlayerInteraction interaction;
    public PlayerConfigurables configurables;

    [SerializeField] private List<GameObject> invisibleToSelf;

    [SerializeField] private float ballThrowSpeed;
    [SerializeField] private float ballThrowDistance;

    [SyncVar]
    public bool isSpectating = true;
    #endregion Fields

    #region Unity Callbacks
    private void Awake()
    {
        manager = FindObjectOfType<GameManager>(true);

        movement.PlayerAwake();
        interaction.PlayerAwake();
        configurables.PlayerAwake();
    }
    private void Start()
    {
        movement.PlayerStart();
        interaction.PlayerStart();
        configurables.PlayerStart();
    }
    private void Update()
    {
        movement.PlayerUpdate();
        interaction.PlayerUpdate();
        configurables.PlayerUpdate();

        // TODO remove
        if (Input.GetMouseButtonDown(2))
        {
            Camera camera = Camera.main;
            Vector3 position = camera.transform.position;
            Vector3 direction = camera.transform.forward;
            CmdThrowBall(position + (direction * ballThrowDistance), direction);
        }
    }
    private void LateUpdate()
    {
        movement.PlayerLateUpdate();
        interaction.PlayerLateUpdate();
        configurables.PlayerLateUpdate();
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
    public override void OnStartClient()
    {
        if (isClientOnly)
        {
            manager.PlayerLookup.TryAdd(netId, this);
        }
    }
    public override void OnStartServer()
    {
        manager.PlayerLookup.TryAdd(netId, this);
    }
    public override void OnStopLocalPlayer()
    {
        // Reset transform of own camera
        Camera.main.transform.SetParent(manager.generalTransform);

        manager.SetLocalPlayer(null);
    }
    public override void OnStopClient()
    {
        if (isClientOnly)
        {
            manager.PlayerLookup.TryRemoveWithNetId(netId, out _);
        }
    }
    public override void OnStopServer()
    {
        manager.PlayerLookup.TryRemoveWithNetId(netId, out _);
    }
    #endregion Mirror Callbacks

    public void ResetState()
    {
        movement.PlayerResetState();
        interaction.PlayerResetState();
        configurables.PlayerResetState();
    }

    public void Spectate()
    {
        if (!IsSpectating)
        {
            IsSpectating = true;
            movement.GeneralTeleport(manager.spectatorBoxTransform.position);
        }
    }
    [Server]
    public void Spawn(Vector3 spawnPosition)
    {
        if (IsSpectating)
        {
            IsSpectating = false;
            movement.GeneralTeleport(spawnPosition);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdThrowBall(Vector3 position, Vector3 direction)
    {
        manager.ball.angularVelocity = Vector3.zero;
        manager.ball.velocity = Vector3.zero;

        manager.ball.position = position;

        manager.ball.AddForce(direction.normalized * ballThrowSpeed, ForceMode.VelocityChange);
    }
}
