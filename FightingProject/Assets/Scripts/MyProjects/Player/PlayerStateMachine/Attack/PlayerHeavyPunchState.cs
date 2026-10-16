using UnityEngine;

//強パンチ
public class PlayerHeavyPunchState : PlayerAttackState
{
    public PlayerHeavyPunchState(Player player) : base(player, Player.AnimationState.HeavyPunch)
    {
    }

    protected override AttackData Data => m_Player.HeavyPunchData;

    protected override bool IsFinished()
    {
        return m_Player.IsHeavyPunchFinished();
    }
}
