using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class PlayerConfigurables : PlayerComponent
{
    public override void PlayerAwake()
    {
        manager = FindObjectOfType<GameManager>(true);
    }
    public override void PlayerLateUpdate()
    {
        // Must occur after player movement update
        if (!isLocalPlayer)
        {
            // Aim username text at camera
            usernameText.transform.LookAt(Camera.main.transform);
            // Only allow rotation about the y axis
            usernameText.transform.eulerAngles = new Vector3(0, usernameText.transform.eulerAngles.y, 0);
        }
    }

    #region Username
    private const int UsernameMaxLength = 16;
    [Header("Username")]
    [SerializeField] private TMP_Text usernameText;
    [SyncVar(hook = nameof(OnUsernameChange))]
    public string username = "";
    [Command]
    public void CmdSetUsername(string newUsername)
    {
        string usernameUntruncated = Regex.Replace(newUsername, @"[^a-zA-Z0-9\s]", string.Empty).Trim();
        if (usernameUntruncated.Length > UsernameMaxLength)
        {
            usernameUntruncated = usernameUntruncated[..UsernameMaxLength];
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
    [Header("Body Color")]
    [SerializeField] private MeshRenderer bodyMeshRenderer;
    [SyncVar(hook = nameof(OnBodyColorChange))]
    public Color bodyColor = Color.white;
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

        // TODO does this work with three people?
        // GetComponent().material? https://mirror-networking.gitbook.io/docs/guides/synchronization/syncvar-hooks
        bodyMeshRenderer.material.color = newBodyColor;
    }
    #endregion Body Color
}
