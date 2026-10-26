
using UnityEngine;

public class PlayerIdleState : PlayerState
{

    public PlayerIdleState(Player player) : base(player)
    {

    }


    public override void Enter()
    {
        m_Player.RPC_PlayAnimation(Player.AnimationState.Idle);
    }

    public override void Update()
    {
        InputState input = m_Player.Input;

        //上にあるほど優先度が高い
        if (IsTryChangeState(input.Direction.IsHorizontal(), m_Player.WalkState) ||    //左右移動
            IsTryChangeState(input.Direction.IsDown(), m_Player.CrouchState) ||  //しゃがみ
            IsTryChangeState(input.IsNormalAttack,m_Player.NormalAttackState) ||
            IsTryChangeState(input.IsLowAttack,m_Player.LowAttackState) ||
            IsTryChangeState(input.IsUpAttack,m_Player.UpAttackState) ||
            IsTryChangeState(input.IsForwardAttack,m_Player.ForwardAttackState) ||
            IsTryChangeState(input.IsGuard, m_Player.GuardState) ||
            IsTryChangeState(input.IsJump, m_Player.JumpState))
        {
            return;
        }
    }
}
