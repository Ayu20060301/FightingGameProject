using UnityEngine;

//空中攻撃
public class PlayerAirAttackState : PlayerAttackState
{
    public PlayerAirAttackState(Player player) : base(player,AnimationState.AirAttack)
    {

    }

    protected override AttackData Data => m_Player.AirAttackData;

    protected override bool IsFinished()
    {
        return m_Player.IsAirAttackFinished();
    }
}
