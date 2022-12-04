using Mirror;
using Mirror.Examples.Benchmark;
using ParrelSync;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private GameManager manager;
    private PlayerReferences refs;

    [SerializeField] private List<PlayerComponent> playerComponents; 

    [SerializeField] private List<GameObject> invisibleToSelf;

    [SerializeField] private float ballThrowSpeed;
    [SerializeField] private float ballThrowDistance;

    [SyncVar(hook = nameof(OnIsSpectatingChange))]
    public bool isSpectating = true;

    public PlayerMovement Movement => Get<PlayerMovement>();
    public PlayerInteraction Interaction => Get<PlayerInteraction>();
    public PlayerConfigurables Configurables => Get<PlayerConfigurables>();

    public event Action OnStartedSpectating;
    public event Action OnStoppedSpectating;

    public T Get<T>() where T : PlayerComponent
    {
        foreach (var playerComponent in playerComponents)
        {
            if (playerComponent.GetType() == typeof(T))
            {
                return (T)playerComponent;
            }
        }
        return null;
    }

    #region Unity Callbacks
    private void Awake()
    {
        manager = FindObjectOfType<GameManager>(true);
        refs = FindObjectOfType<PlayerReferences>(true);

        playerComponents.ForEach(c => c.Init(manager, refs, this));

        playerComponents.ForEach(c => c.PlayerAwake());

        OnStartedSpectating += () =>
        {
            print("test start spectating");
        };

        OnStoppedSpectating += () =>
        {
            print("test start spectating");
        };
    }
    private void Start() => playerComponents.ForEach(c => c.PlayerStart());
    private void Update()
    {
        playerComponents.ForEach(c => c.PlayerUpdate());

        if (Input.GetMouseButtonDown(2))
        {
            Camera camera = Camera.main;
            Vector3 position = camera.transform.position;
            Vector3 direction = camera.transform.forward;
            CmdThrowBall(position + (direction * ballThrowDistance), direction);
        }
    }
    private void LateUpdate() => playerComponents.ForEach(c => c.PlayerLateUpdate());
    #endregion Unity Callbacks

    #region Mirror Callbacks
    public override void OnStartLocalPlayer()
    {
        // Position own camera
        Camera.main.transform.SetParent(transform);
        Camera.main.transform.localPosition = new Vector3(0, 1.6f, 0);

        // Make own GameObjects invisible
        invisibleToSelf.ForEach(go => go.SetActive(false));

        // Maintain local player reference
        manager.SetLocalPlayer(this);
    }
    public override void OnStartClient()
    {
        // Maintain player lookup
        manager.PlayerLookup.Add(this);
    }
    public override void OnStartServer()
    {
        // Maintain player lookup
        manager.PlayerLookup.Add(this);
    }
    public override void OnStopLocalPlayer()
    {
        // Reset transform of own camera
        Camera.main.transform.SetParent(refs.generalTransform);

        // Maintain local player reference
        manager.SetLocalPlayer(null);
    }
    public override void OnStopClient()
    {
        // Maintain player lookup
        manager.PlayerLookup.Remove(this);
    }
    public override void OnStopServer()
    {
        // Maintain player lookup
        manager.PlayerLookup.Remove(this);
    }
    #endregion Mirror Callbacks

    #region Round Lifecycle
    public void RoundInit() => playerComponents.ForEach(c => c.PlayerRoundInit());
    [Command]
    public void CmdStartSpectating()
    {
        if (!isSpectating)
        {
            isSpectating = true;
            Movement.GeneralTeleport(refs.spectatorBoxTransform.position);
        }
    }
    private void OnIsSpectatingChange(bool _, bool newIsSpectating)
    {
        if (newIsSpectating) // Started spectating
        {
            OnStartedSpectating?.Invoke();
        }
        else // Stopped spectating
        {
            OnStoppedSpectating?.Invoke();
        }
    }
    // TODO could just use isSpectating sync var, then subscribe to the event in
    // Movement, to replace movement.GeneralTeleport()
    // TODO dont need, spawn, round start, spectatic, round stop
    [Server]
    public void ServerStopSpectating(Vector3 teleportPosition)
    {
        if (isSpectating)
        {
            isSpectating = false;
            Movement.GeneralTeleport(teleportPosition);
        }
    }
    #endregion Round Lifecycle

    [Command(requiresAuthority = false)]
    public void CmdThrowBall(Vector3 position, Vector3 direction)
    {
        refs.ball.angularVelocity = Vector3.zero;
        refs.ball.velocity = Vector3.zero;

        refs.ball.position = position;

        refs.ball.AddForce(direction.normalized * ballThrowSpeed, ForceMode.VelocityChange);
    }
}
