using UnityEngine;

public class PlayerCrouchAttackState : PlayerAttackState
{
    public PlayerCrouchAttackState(Player player) : base(player, AnimationState.CrouchAttack)
    {

    }

    protected override AttackData Data => m_Player.CrouchAttackData;

    protected override bool IsFinished()
    {
        return m_Player.IsCrouchAttackFinished();   
    }
}
