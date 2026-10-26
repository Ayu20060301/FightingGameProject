using UnityEngine;

//下攻撃(Aボタンで入力)
public class PlayerLowAttackState : PlayerAttackState
{
    public PlayerLowAttackState(Player player) : base(player, Player.AnimationState.LowAttack)
    {

    }


    protected override AttackData Data => m_Player.LowAttackData;


    protected override bool IsFinished()
    {
        return m_Player.IsLowAttackFinished();
    }
 
}
