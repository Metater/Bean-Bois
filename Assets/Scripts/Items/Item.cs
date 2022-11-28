using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : NetworkBehaviour
{
    #region Fields
    [Header("General")]
    private GameManager manager;
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected float dropForce;
    [SerializeField] protected GameObject modelGameObject;

    // TODO this isnt necessary or used currently and never will be, only complexity
    [System.NonSerialized] public Vector3 smoothDampVelocity = Vector3.zero;

    public bool IsPickedUp { get; private set; } = false;
    #endregion Fields

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
    }
    public override void OnStopAuthority()
    {
        print("Stop authority");
    }
    #endregion Mirror Callbacks

    public void Pickup()
    {
        IsPickedUp = true;

        rb.useGravity = false;
        rb.detectCollisions = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        smoothDampVelocity = Vector3.zero;

        PickupProtected();
    }
    public void Drop(Vector3 dropVector, Vector3 velocity)
    {
        IsPickedUp = false;

        rb.useGravity = true;
        rb.detectCollisions = true;

        // messes with sync? could have no authority, hasAuthority check????
        rb.AddForce((dropVector * dropForce) + velocity, ForceMode.Impulse);
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
