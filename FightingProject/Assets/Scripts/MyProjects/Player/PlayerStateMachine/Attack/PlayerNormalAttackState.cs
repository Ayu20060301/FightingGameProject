using UnityEngine;

//通常攻撃
public class PlayerNormalAttackState : PlayerAttackState
{
    public PlayerNormalAttackState(Player player) : base(player, Player.AnimationState.NormalAttack)
    {

    }

    protected override AttackData Data => m_Player.NormalAttackData;


    protected override bool IsFinished()
    {
        return m_Player.IsNormalAttackFinished();
    }
}
