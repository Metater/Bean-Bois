using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IPlayerCallbacks
{
    public void PlayerAwake();
    public void PlayerStart();
    public void PlayerUpdate();
    public void PlayerLateUpdate();
    public void PlayerResetState();
}