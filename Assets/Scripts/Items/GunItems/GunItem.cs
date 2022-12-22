using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GunItem : Item
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
    }
    protected override void RightClickProtected()
    {
        print($"R: {gameObject.name}");
    }
}
