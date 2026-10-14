using UnityEngine;

//強キック
public class PlayerHeavyKickState : PlayerAttackState
{

    public PlayerHeavyKickState(Player player) : base(player, Player.AnimationState.HeavyKick)
    {
    }

    protected override bool IsFinished()
    {
        return m_Player.IsHeavyKickFinished();
    }
}
