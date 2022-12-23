using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunItem : Item
{
    protected override void PickupProtected()
    {

    }
    protected override void DropProtected()
    {

    }
    protected override void SelectProtected()
    {

    }
    protected override void DeselectProtected()
    {

    }
    protected override void LeftClickProtected()
    {
        print($"L: {gameObject.name}");

        var actions = manager.LocalPlayer.Get<PlayerActions>();
        actions.CmdShoot();
    }
    protected override void RightClickProtected()
    {
        print($"R: {gameObject.name}");
    }
}
