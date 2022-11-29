using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : PlayerComponent
{
    private const int SlotCount = 3;

    #region Fields
    [Header("Transforms")]
    [SerializeField] private Transform gripTransform;

    [Header("Interaction")]
    [SerializeField] private float reachDistance;
    [SerializeField] private float itemRotationSlerpMultiplier;
    private readonly Item[] slots = new Item[SlotCount];
    private int selectedSlotLocal = 0;
    [SyncVar(hook = nameof(OnSlotChanged))] public int selectedSlotSynced = 0;

    [Header("Crosshair Colors")]
    [SerializeField] private Color crosshairDefaultColor;
    [SerializeField] private Color crosshairHoverItemColor;
    #endregion Fields

    public override void PlayerAwake()
    {
        manager = FindObjectOfType<GameManager>(true);
    }
    public override void PlayerUpdate()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        Item selectedItem = slots[selectedSlotLocal];
        Color crosshairColor = crosshairDefaultColor;
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (selectedItem == null && Physics.Raycast(ray, out var hit, reachDistance))
        {
            Item item = hit.transform.gameObject.GetComponentInParent<Item>();
            if (item != null && !item.IsHeld)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    CmdPickupItem(item.netId);
                }
                crosshairColor = crosshairHoverItemColor;
            }
        }
        refs.crosshairImage.color = crosshairColor;

        UpdateSlot();

        if (selectedItem != null)
        {
            if (Input.GetKeyDown(KeyCode.G) && selectedSlotLocal == selectedSlotSynced)
            {
                CmdDropItem(selectedItem.netId, Camera.main.transform.forward, player.Movement.GetCurrentVelocity());
            }
        }

        // TODO also do this on teleport, make event
        foreach (var item in slots)
        {
            if (item == null || !item.isOwned)
            {
                continue;
            }

            Quaternion rotation = Quaternion.Slerp(item.transform.rotation, gripTransform.rotation, Time.deltaTime * itemRotationSlerpMultiplier);
            item.transform.SetPositionAndRotation(gripTransform.position, rotation);
        }
    }

    #region Slot
    private void OnSlotChanged(int oldSelectedSlot, int newSelectedSlot)
    {
        if (oldSelectedSlot >= 0 && oldSelectedSlot < SlotCount && slots[oldSelectedSlot] != null)
        {
            slots[oldSelectedSlot].Deselect();
        }

        if (newSelectedSlot >= 0 && newSelectedSlot < SlotCount && slots[newSelectedSlot] != null)
        {
            slots[newSelectedSlot].Select();
        }
    }

    private void UpdateSlot()
    {
        int originalSelectedSlotLocal = selectedSlotLocal;

        float mouseScrollDelta = Input.mouseScrollDelta.y;
        if (mouseScrollDelta > 0) // scroll up
        {
            selectedSlotLocal++;
        }
        else if (mouseScrollDelta < 0) // scroll down
        {
            selectedSlotLocal--;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedSlotLocal = 0;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedSlotLocal = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            selectedSlotLocal = 2;
        }

        if (selectedSlotLocal < 0)
        {
            selectedSlotLocal = SlotCount - 1;
        }
        else if (selectedSlotLocal >= SlotCount)
        {
            selectedSlotLocal = 0;
        }

        if (originalSelectedSlotLocal != selectedSlotLocal)
        {
            CmdChangeSelectedSlot(selectedSlotLocal);
        }
    }

    [Command]
    public void CmdChangeSelectedSlot(int newSelectedSlot)
    {
        if (newSelectedSlot < 0 || newSelectedSlot > SlotCount)
        {
            return;
        }

        selectedSlotSynced = newSelectedSlot;
    }
    #endregion Slot

    #region Pickup
    [Command]
    public void CmdPickupItem(uint netId)
    {
        if (manager.ItemLookup.TryGetWithNetId(netId, out var item) && !item.IsHeld)
        {
            int slot = selectedSlotSynced;
            if (slots[slot] == null)
            {
                // item is owned
                if (item.netIdentity.connectionToClient != null)
                {
                    // item is not owned by desired owner
                    if (connectionToClient != item.netIdentity.connectionToClient)
                    {
                        item.netIdentity.RemoveClientAuthority();
                        item.netIdentity.AssignClientAuthority(connectionToClient);
                    }
                }
                else // item is unowned
                {
                    // Server was doing physics for the item, now it doesnt need to
                    if (isServer)
                    {
                        item.DisableOwnedRigidbody();
                    }

                    item.netIdentity.AssignClientAuthority(connectionToClient);
                }

                if (isServerOnly)
                {
                    SharedPickupItem(item, slot);
                }
                RpcPickupItem(netId, slot);
            }
        }
    }

    [ClientRpc]
    public void RpcPickupItem(uint netId, int slot)
    {
        if (manager.ItemLookup.TryGetWithNetId(netId, out var item))
        {
            SharedPickupItem(item, slot);
        }
    }

    private void SharedPickupItem(Item item, int slot)
    {
        slots[slot] = item;
        item.Pickup();

        if (isLocalPlayer)
        {
            item.transform.SetPositionAndRotation(gripTransform.position, gripTransform.rotation);
        }
    }
    #endregion Pickup

    #region Drop
    [Command]
    public void CmdDropItem(uint netId, Vector3 dropVector, Vector3 velocity)
    {
        if (manager.ItemLookup.TryGetWithNetId(netId, out var item))
        {
            for (int i = 0; i < 3; i++)
            {
                Item slot = slots[i];
                if (slot == null)
                {
                    continue;
                }

                if (slot.netId == netId)
                {
                    if (isServerOnly)
                    {
                        SharedDropItem(item, i, dropVector, velocity);
                    }
                    RpcDropItem(netId, i, dropVector, velocity);
                    break;
                }
            }
        }
    }

    [ClientRpc]
    public void RpcDropItem(uint netId, int slot, Vector3 dropVector, Vector3 velocity)
    {
        if (manager.ItemLookup.TryGetWithNetId(netId, out var item))
        {
            SharedDropItem(item, slot, dropVector, velocity);
        }
    }

    private void SharedDropItem(Item item, int slot, Vector3 dropVector, Vector3 velocity)
    {
        slots[slot] = null;
        item.Drop(dropVector, velocity);
    }
    #endregion Drop
}
