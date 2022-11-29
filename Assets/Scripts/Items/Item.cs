using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : NetworkBehaviour
{
    private GameManager manager;

    [SerializeField] protected OwnedRigidbody ownedRigidbody;
    // TODO RENAME TO dropForceMultiplier
    [SerializeField] protected float dropForce;
    [SerializeField] protected GameObject modelGameObject;

    public bool IsHeld { get; private set; } = false;

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
        manager.ItemLookup.Add(this);
    }
    public override void OnStartServer()
    {
        manager.ItemLookup.Add(this);

        // Server has responsibility of doing item physics when item has no owners
        ownedRigidbody.Enable();
    }
    public override void OnStopClient()
    {
        manager.ItemLookup.Remove(this);
    }
    public override void OnStopServer()
    {
        manager.ItemLookup.Remove(this);
    }
    public override void OnStartAuthority()
    {
        ownedRigidbody.Enable();
    }
    public override void OnStopAuthority()
    {
        ownedRigidbody.Disable();
    }
    #endregion Mirror Callbacks

    [Server]
    public void DisableOwnedRigidbody()
    {
        ownedRigidbody.Disable();
    }

    // Client and Server
    public void Pickup()
    {
        IsHeld = true;

        // Disable item colliders while picked up
        ownedRigidbody.SetColliders(false);
        // When the object is picked up, disable the rigidbody if it exists
        // Nobody needs physics now
        ownedRigidbody.Disable();

        PickupProtected();
    }
    // Client and Server
    public void Drop(Vector3 dropVector, Vector3 velocity)
    {
        IsHeld = false;

        // When object is dropped, enable item colliders for everyone
        ownedRigidbody.SetColliders(true);

        // When dropped and object is owned
        // It is now the owners responsibilty to control the rigidbody for the item
        // Enable the rigidbody and add the force to drop it
        if (isOwned)
        {
            ownedRigidbody.Enable();
            // Beware: dropVector and velocity are assumed to be trustworthy
            ownedRigidbody.Rigidbody.AddForce((dropVector * dropForce) + velocity, ForceMode.Impulse);
        }

        DropProtected();
    }
    // Client and Server
    public void Select()
    {
        // Enable visual while selected for everyone
        modelGameObject.SetActive(true);

        SelectProtected();
    }
    // Client and Server
    public void Deselect()
    {
        // Disable visual when deselected
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
