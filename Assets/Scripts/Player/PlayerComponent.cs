using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerComponent : NetworkBehaviour
{
    protected GameManager manager;
    protected PlayerReferences refs;
    protected Player player;

    public void Init(GameManager manager, PlayerReferences refs, Player player)
    {
        this.manager = manager;
        this.refs = refs;
        this.player = player;
    }

    public virtual void PlayerAwake() { }
    public virtual void PlayerStart() { }
    public virtual void PlayerUpdate() { }
    public virtual void PlayerLateUpdate() { }
    public virtual void PlayerRoundInit() { }
}