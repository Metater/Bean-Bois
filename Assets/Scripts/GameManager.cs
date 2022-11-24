using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
    Items will be deleted when client that last touched them disconnects, not good
*/

public class GameManager : MonoBehaviour
{
    public Player LocalPlayer { get; private set; }
    public NetRefLookup<Player> PlayerLookup { get; private set; }
    public NetRefLookup<Item> ItemLookup { get; private set; }
    private bool isCursorVisible = true;
    public Image crosshairImage;
    public Transform generalTransform;
    public Rigidbody ball;

    #region Unity Callbacks
    private void Awake()
    {
        PlayerLookup = new();
        ItemLookup = new();

        UpdateCursorVisibility();
    }
    private void FixedUpdate()
    {
        /*
        foreach (var player in PlayerLookup.Refs)
        {
            print($"Player: {player.netId}");
        }
        */
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isCursorVisible = !isCursorVisible;
            UpdateCursorVisibility();
        }
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
}
