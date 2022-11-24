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
    public PlayerInteraction playerInteraction;
    [SerializeField] private List<GameObject> invisibleToSelf;
    [SerializeField] private float ballThrowSpeed;
    [SerializeField] private float ballThrowDistance;
    #endregion Fields

    #region Unity Callbacks
    private void Awake()
    {
        manager = FindObjectOfType<GameManager>(true);

        playerMovement.PlayerAwake();
        playerInteraction.PlayerAwake();
    }
    private void Start()
    {
        playerMovement.PlayerStart();
        playerInteraction.PlayerStart();
    }
    private void Update()
    {
        playerMovement.PlayerUpdate();
        playerInteraction.PlayerUpdate();

        if (Input.GetMouseButtonDown(2))
        {
            Camera camera = Camera.main;
            Vector3 position = camera.transform.position;
            Vector3 direction = camera.transform.forward;
            CmdThrowBall(position + (direction * ballThrowDistance), direction);
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
    public override void OnStartClient()
    {
        if (!isServer)
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
        if (!isServer)
        {
            manager.PlayerLookup.TryRemoveWithNetId(netId, out _);
        }
    }
    public override void OnStopServer()
    {
        manager.PlayerLookup.TryRemoveWithNetId(netId, out _);
    }
    #endregion Mirror Callbacks

    public interface IPlayerCallbacks
    {
        public void PlayerAwake();
        public void PlayerStart();
        public void PlayerUpdate();
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
