using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerActions : PlayerComponent
{
    #region Player Callbacks

    #endregion Player Callbacks

    #region Actions
    [Command]
    public void CmdShoot()
    {
        print("Hey, Im the server and i recieved a shoot message!");
    }
    #endregion Actions
}