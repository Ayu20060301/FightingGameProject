using UnityEngine;

public class PlayerLightKickState : PlayerAttackState
{
    public PlayerLightKickState(Player player) : base(player, Player.AnimationState.LightKick)
    {
    }

    protected override bool IsFinished()
    {
        return m_Player.IsLightKickFinished();
    }
}
