using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OwnedRigidbody : NetworkBehaviour
{
    [SerializeField] private List<Collider> colliders;

    [SerializeField] private float mass;
    [SerializeField] private float drag;
    [SerializeField] private float angularDrag;
    [SerializeField] private bool useGravity;
    [SerializeField] private bool isKinematic;
    [SerializeField] private RigidbodyInterpolation interpolate;
    [SerializeField] private CollisionDetectionMode collisionDetection;

    public event Action OnOwnerDisconnected;
    public Rigidbody Rigidbody { get; private set; }

    public void Enable()
    {
        if (Rigidbody != null)
        {
            return;
        }

        Rigidbody = ConstructRigidbody();
    }

    public void Disable()
    {
        if (Rigidbody == null)
        {
            return;
        }

        Destroy(Rigidbody);
    }

    public void SetCollidersEnabled(bool enabled)
    {
        colliders.ForEach(c => c.enabled = enabled);
    }

    [Server]
    public void OnOwnerDisconnect()
    {
        // Has choice to let this be destroyed or not
        // May want held items to diseapper when player logs out
        // bc of loadouts?5
        // TODO ignoring this for now
        // netIdentity.RemoveClientAuthority();

        OnOwnerDisconnected?.Invoke();
    }

    public Rigidbody ConstructRigidbody()
    {
        Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();

        rigidbody.mass = mass;
        rigidbody.drag = drag;
        rigidbody.angularDrag = angularDrag;
        rigidbody.useGravity = useGravity;
        rigidbody.isKinematic = isKinematic;
        rigidbody.interpolation = interpolate;
        rigidbody.collisionDetectionMode = collisionDetection;

        return rigidbody;
    }
}
