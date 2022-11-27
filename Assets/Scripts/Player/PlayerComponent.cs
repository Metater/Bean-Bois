using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerComponent : NetworkBehaviour
{
    protected GameManager manager;
    protected Player player;
    protected PlayerReferences refs;

    public void Init(Player player, PlayerReferences refs)
    {
        manager = FindObjectOfType<GameManager>(true);
        this.player = player;
        this.refs = refs;
    }

    public virtual void PlayerAwake() { }
    public virtual void PlayerStart() { }
    public virtual void PlayerUpdate() { }
    public virtual void PlayerLateUpdate() { }
    public virtual void PlayerRoundInit() { }
}