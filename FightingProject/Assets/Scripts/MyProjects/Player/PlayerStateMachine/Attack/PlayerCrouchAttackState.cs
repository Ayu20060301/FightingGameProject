using UnityEngine;

public class PlayerCrouchAttackState : PlayerAttackState
{
    public PlayerCrouchAttackState(Player player) : base(player, AnimationState.CrouchAttack)
    {

    }

    protected override AttackData Data => m_Player.CrouchAttackData;


    public override void Update()
    {
        if(m_Player.Input.Direction.IsDown())
        {
            m_Player.ChangeState(m_Player.CrouchState);
        }
        else
        {
            m_Player.ChangeState(m_Player.IdleState);
        }
    }

    protected override bool IsFinished()
    {
        return m_Player.IsCrouchAttackFinished(); 

    }
}
