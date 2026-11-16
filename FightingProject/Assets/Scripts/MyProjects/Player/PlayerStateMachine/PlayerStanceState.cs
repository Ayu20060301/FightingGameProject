using UnityEngine;

public class PlayerStanceState : PlayerState
{
    public PlayerStanceState(Player player) : base(player)
    {

    }

    public override void Enter()
    {
        m_Player.RPC_PlayAnimation(AnimationState.StanceIdle);
    }

}
