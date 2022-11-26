using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class PlayerConfigurables : NetworkBehaviour, IPlayerCallbacks
{
    #region Fields
    private GameManager manager;

    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private MeshRenderer bodyMeshRenderer;

    [SyncVar(hook = nameof(OnBodyColorChange))]
    public Color bodyColor = Color.white;

    [SyncVar(hook = nameof(OnUsernameChange))]
    public string username = "";

    [SyncVar(hook = nameof(OnMainTextChange))]
    public string mainText = "";
    #endregion Fields

    #region Player Callbacks
    public void PlayerAwake()
    {
        manager = FindObjectOfType<GameManager>(true);
    }
    public void PlayerStart()
    {
        
    }
    public void PlayerUpdate()
    {
        
    }
    public void PlayerLateUpdate()
    {
        if (!isLocalPlayer && !manager.IsLocalPlayerNull)
        {
            usernameText.transform.LookAt(manager.LocalPlayer.transform);
            usernameText.transform.eulerAngles = new Vector3(0, usernameText.transform.eulerAngles.y, 0);
        }
    }
    public void PlayerResetState()
    {
        
    }
    #endregion Player Callbacks

    #region Username
    [Command(requiresAuthority = false)]
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
    #endregion Username

    #region Body Color
    [Command(requiresAuthority = false)]
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
    #endregion Body Color

    #region Misc Sync
    private void OnMainTextChange(string _, string newMainText)
    {
        manager.mainText.text = newMainText;
    }
    #endregion Misc Sync
}
