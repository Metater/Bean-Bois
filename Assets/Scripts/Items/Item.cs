using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(OwnedRigidbody))]
public abstract class Item : NetworkBehaviour
{
    protected GameManager manager;
    protected OwnedRigidbody ownedRigidbody;

    [SerializeField] protected float dropForceMultiplier;
    [SerializeField] protected GameObject modelGameObject;

    public bool IsHeld { get; private set; } = false;

    // TODO when a client leaves and is holding an item, funky stuff happens
    // Beware: Owners of this item are completely trusted

    #region Unity Callbacks
    private void Awake()
    {
        manager = FindObjectOfType<GameManager>(true);
        ownedRigidbody = GetComponent<OwnedRigidbody>();
    }
    private void OnValidate()
    {
        // Ensure all items and children of items have the "Item" layer set
        EditorApplication.delayCall += () =>
        {
            if (Application.isPlaying)
            {
                return;
            }

            int layer = LayerMask.NameToLayer("Item");
            gameObject.layer = layer;
            var children = gameObject.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                child.gameObject.layer = layer;
            }
        };
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

    // Everyone
    public void Pickup()
    {
        IsHeld = true;

        // Disable item colliders while picked up
        ownedRigidbody.SetCollidersEnabled(false);
        // When the object is picked up, disable the rigidbody if it exists
        // Nobody needs physics now
        ownedRigidbody.Disable();

        PickupProtected();
    }
    // Everyone
    public void Drop(Vector3 dropVector, Vector3 velocity)
    {
        IsHeld = false;

        // When object is dropped, enable item colliders for everyone
        ownedRigidbody.SetCollidersEnabled(true);

        // When dropped and object is owned
        // It is now the owners responsibilty to control the rigidbody for the item
        // Enable the rigidbody and add the force to drop it
        if (isOwned)
        {
            ownedRigidbody.Enable();
            ownedRigidbody.Rigidbody.AddForce((dropVector * dropForceMultiplier) + velocity, ForceMode.Impulse);
        }

        DropProtected();
    }
    [Client]
    public void Select()
    {
        // Enable visual while selected for everyone
        modelGameObject.SetActive(true);

        SelectProtected();
    }
    [Client]
    public void Deselect()
    {
        // Disable visual when deselected
        modelGameObject.SetActive(false);

        DeselectProtected();
    }
    [Client]
    public void LeftClick()
    {
        LeftClickProtected();
    }
    [Client]
    public void RightClick()
    {
        RightClickProtected();
    }

    protected abstract void PickupProtected();
    protected abstract void DropProtected();
    protected abstract void SelectProtected();
    protected abstract void DeselectProtected();
    protected abstract void LeftClickProtected();
    protected abstract void RightClickProtected();

    [Server]
    public void ServerDisableOwnedRigidbody()
    {
        // Allows owner to control rigidbody now
        ownedRigidbody.Disable();
    }
}
