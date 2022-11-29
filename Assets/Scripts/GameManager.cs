using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
    Items will be deleted when client that last touched them disconnects, not good
    Transfer authority back to server ^^^^^^^^^^^^^^^^^^^^ On disconnect
*/

// TODO for using RollbackAndRaycast raycastLayers, do Physics.DefaultRaycastLayers ^ LayerMask.GetMask("Item")

// TODO Make sync vars private??? in some cases it is unclear that it is doing something over the network
//^^^^^^^ Maybe use prefix net_or something

public class GameManager : MonoBehaviour
{
    #region Fields
    private UiManager ui;

    public Player LocalPlayer { get; private set; }
    public bool IsLocalPlayerNull => LocalPlayer == null;

    public bool IsCursorVisable { get; private set; } = true;

    public NetRefLookup<Player> PlayerLookup { get; private set; }
    public NetRefLookup<Item> ItemLookup { get; private set; }
    #endregion Fields

    #region Unity Callbacks
    private void Awake()
    {
        ui = FindObjectOfType<UiManager>(true);

        PlayerLookup = new();
        ItemLookup = new();

        UpdateCursorVisibility();
    }
    private void Update()
    {
        if (!IsLocalPlayerNull && Input.GetKeyDown(KeyCode.Escape))
        {
            IsCursorVisable = !IsCursorVisable;
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

    public void SetLocalPlayer(Player localPlayer)
    {
        LocalPlayer = localPlayer;

        if (IsLocalPlayerNull)
        {
            IsCursorVisable = true;
            UpdateCursorVisibility();
        }

        ui.SetUiDisabled(IsLocalPlayerNull);
    }

    private void UpdateCursorVisibility()
    {
        Cursor.lockState = IsCursorVisable ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = IsCursorVisable;
        ui.ShowUi(IsCursorVisable);
    }
}
