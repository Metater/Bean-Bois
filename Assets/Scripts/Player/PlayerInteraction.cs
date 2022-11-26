using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : NetworkBehaviour, Player.IPlayerCallbacks
{
    public const int SlotCount = 3;

    #region Fields
    [Header("General")]
    private GameManager manager;
    [SerializeField] private Player player;
    [Header("Transforms")]
    [SerializeField] private Transform gripTransform;
    [Header("Interaction")]
    [SerializeField] private float reachDistance;
    // item should be smoothed relative to player body position???
    [SerializeField] private float itemSmoothTime;
    [SerializeField] private float itemRotationSlerpMultiplier;
    private Item[] slots;
    private int selectedSlotLocal = 0;
    [SyncVar(hook = nameof(OnSlotChanged))] public int selectedSlotSynced = 0;
    [Header("Crosshair Colors")]
    [SerializeField] private Color crosshairDefaultColor;
    [SerializeField] private Color crosshairHoverItemColor;
    #endregion Fields

    #region Player Callbacks
    public void PlayerAwake()
    {
        manager = FindObjectOfType<GameManager>(true);

        slots = new Item[3];
    }

    public void PlayerStart()
    {
        
    }

    public void PlayerUpdate()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        Item selectedItem = slots[selectedSlotLocal];
        Color crosshairColor = crosshairDefaultColor;
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (selectedItem is null && Physics.Raycast(ray, out var hit, reachDistance))
        {
            if (hit.transform.gameObject.TryGetComponent<Item>(out var item) && !item.IsPickedUp)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    CmdPickupItem(item.netId);
                }
                crosshairColor = crosshairHoverItemColor;
            }
        }
        manager.crosshairImage.color = crosshairColor;

        UpdateSlot();

        if (selectedItem is not null)
        {
            if (Input.GetKeyDown(KeyCode.G) && selectedSlotLocal == selectedSlotSynced)
            {
                CmdDropItem(selectedItem.netId, Camera.main.transform.forward, player.playerMovement.Velocity);
            }
        }

        foreach (var item in slots)
        {
            if (item is null || !item.isOwned)
            {
                continue;
            }

            Vector3 position = Vector3.SmoothDamp(item.transform.position, gripTransform.position, ref item.smoothDampVelocity, itemSmoothTime);
            Quaternion rotation = Quaternion.Slerp(item.transform.rotation, gripTransform.rotation, Time.deltaTime * itemRotationSlerpMultiplier);
            item.transform.SetPositionAndRotation(position, rotation);
        }
    }

    public void PlayerResetState()
    {
        
    }
    #endregion Player Callbacks

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
        if (manager.ItemLookup.TryGetWithNetId(netId, out var item) && !item.IsPickedUp)
        {
            int slot = selectedSlotSynced;
            if (slots[slot] is null)
            {
                // Unnessarily removes and reassigns client authority when client picks up same item >= 2 times in a row
                if (item.netIdentity.connectionToClient is not null)
                {
                    item.netIdentity.RemoveClientAuthority();
                }

                item.netIdentity.AssignClientAuthority(connectionToClient);
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
                if (slot is null)
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
