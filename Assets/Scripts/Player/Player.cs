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
    [Header("General")]
    private GameManager manager;
    public PlayerMovement playerMovement;
    public PlayerInteraction playerInteraction;
    [SerializeField] private List<GameObject> invisibleToSelf;
    [SerializeField] private float ballThrowSpeed;
    [SerializeField] private float ballThrowDistance;
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private MeshRenderer bodyMeshRenderer;

    [SyncVar(hook = nameof(OnBodyColorChange))]
    public Color bodyColor = Color.white;
    [SyncVar(hook = nameof(OnUsernameChange))]
    public string username = "";
    [SyncVar(hook = nameof(OnTextChange))]
    public string text = "";
    // TODO MAKE IS SPECTATING A SYNC VAR, ONE SOURCE OF TRUTH, have CmdSpecate, clients call, when hit lava or other deadly stuff
    public bool IsSpectating { get; set; } = true;
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

        // TODO remove
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

    public interface IPlayerCallbacks
    {
        public void PlayerAwake();
        public void PlayerStart();
        public void PlayerUpdate();
        public void PlayerResetState();
    }

    public void ResetState()
    {
        playerMovement.PlayerResetState();
        playerInteraction.PlayerResetState();
    }

    public void Spectate()
    {
        if (!IsSpectating)
        {
            IsSpectating = true;
            playerMovement.GeneralTeleport(manager.spectatorBoxTransform.position);
        }
    }
    public void Spawn(Vector3 spawnPosition)
    {
        if (IsSpectating)
        {
            IsSpectating = false;
            playerMovement.GeneralTeleport(spawnPosition);
        }
    }

    public void SetUsername(string newUsername)
    {
        CmdSetUsername(newUsername);
    }

    public void SetBodyColor(Color newBodyColor)
    {
        CmdSetBodyColor(newBodyColor);
    }

    // TODO remove
    [Command(requiresAuthority = false)]
    public void CmdThrowBall(Vector3 position, Vector3 direction)
    {
        manager.ball.angularVelocity = Vector3.zero;
        manager.ball.velocity = Vector3.zero;

        manager.ball.position = position;

        manager.ball.AddForce(direction.normalized * ballThrowSpeed, ForceMode.VelocityChange);
    }

    [Command]
    public void CmdSetUsername(string newUsername)
    {
        string usernameUntruncated = Regex.Replace(newUsername, @"[^a-zA-Z0-9\s]", string.Empty).Trim();
        if (usernameUntruncated.Length > 16)
        {
            usernameUntruncated = usernameUntruncated[..16];
        }
        username = usernameUntruncated;
    }

    private void OnUsernameChange(string _, string newUsername)
    {
        if (isLocalPlayer)
        {
            return;
        }

        usernameText.text = newUsername;
    }

    [Command]
    public void CmdSetBodyColor(Color newBodyColor)
    {
        bodyColor = newBodyColor;
    }

    private void OnBodyColorChange(Color _, Color newBodyColor)
    {
        if (isLocalPlayer)
        {
            return;
        }

        bodyMeshRenderer.material.color = newBodyColor;
    }

    private void OnTextChange(string _, string newText)
    {
        manager.text.text = newText;
    }
}
