using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : NetworkBehaviour
{
    #region Fields
    [Header("General")]
    private GameManager manager;

    [SerializeField] protected OwnedRigidbody ownedRigidbody;
    [SerializeField] protected float dropForce;
    [SerializeField] protected GameObject modelGameObject;

    public bool IsPickedUp { get; private set; } = false;
    #endregion Fields

    // TODO when a client leaves and is holding an item, funky stuff happens

    #region Unity Callbacks
    private void Awake()
    {
        manager = FindObjectOfType<GameManager>(true);
    }
    #endregion Unity Callbacks

    #region Mirror Callbacks
    public override void OnStartClient()
    {
        if (isClientOnly)
        {
            manager.ItemLookup.TryAdd(netId, this);
        }
    }
    public override void OnStartServer()
    {
        manager.ItemLookup.TryAdd(netId, this);

        ownedRigidbody.Enable();
    }
    public override void OnStopClient()
    {
        /*
        foreach (var item in manager.ItemLookup.Refs)
        {

        }
        */

        if (isClientOnly)
        {
            print("bye bye client only");

            manager.ItemLookup.TryRemoveWithNetId(netId, out _);
        }
    }
    public override void OnStopServer()
    {
        print("bye bye server");

        manager.ItemLookup.TryRemoveWithNetId(netId, out _);
    }
    public override void OnStartAuthority()
    {
        print("Start authority");
        ownedRigidbody.Enable();
    }
    public override void OnStopAuthority()
    {
        print("Stop authority");
        ownedRigidbody.Disable();
    }
    #endregion Mirror Callbacks

    [Server]
    public void StopServerPhysics()
    {
        ownedRigidbody.Disable();
    }

    public void Pickup()
    {
        IsPickedUp = true;

        ownedRigidbody.SetColliders(false);
        ownedRigidbody.Disable();

        PickupProtected();
    }
    public void Drop(Vector3 dropVector, Vector3 velocity)
    {
        IsPickedUp = false;

        ownedRigidbody.SetColliders(true);
        if (isOwned)
        {
            ownedRigidbody.Enable();
            ownedRigidbody.Rigidbody.AddForce((dropVector * dropForce) + velocity, ForceMode.Impulse);
        }

        DropProtected();
    }
    public void Select()
    {
        modelGameObject.SetActive(true);
        SelectProtected();
    }
    public void Deselect()
    {
        modelGameObject.SetActive(false);
        DeselectProtected();
    }

    protected abstract void PickupProtected();
    protected abstract void DropProtected();
    protected abstract void SelectProtected();
    protected abstract void DeselectProtected();
    protected abstract void LeftClickProtected();
    protected abstract void RightClickProtected();
}
