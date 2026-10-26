using UnityEngine;

public class PlayerForwardAttackState : PlayerAttackState
{
    public PlayerForwardAttackState(Player player) : base(player, Player.AnimationState.ForwardAttack)
    {

    }

    protected override AttackData Data => m_Player.ForwardAttackData;


    protected override bool IsFinished()
    {
        return m_Player.IsForwardAttackFinished();
    }
}
