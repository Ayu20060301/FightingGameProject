using UnityEngine;

//上攻撃
public class PlayerUpAttackState : PlayerAttackState
{
    public PlayerUpAttackState(Player player) : base(player,Player.AnimationState.UpAttack)
    {

    }

    protected override AttackData Data => m_Player.UpAttackData;



    protected override bool IsFinished()
    {
        return m_Player.IsUpAttackFinished();
    }
}
