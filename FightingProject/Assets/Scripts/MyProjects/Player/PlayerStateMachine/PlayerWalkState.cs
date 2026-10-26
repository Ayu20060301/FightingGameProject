using UnityEngine;

/// <summary>
/// 歩行ステート
/// </summary>
public class PlayerWalkState : PlayerState
{
    public PlayerWalkState(Player player) : base(player)
    {

    }

    public override void Enter()
    {
        m_Player.RPC_PlayAnimation(Player.AnimationState.Walk);
    }


    public override void Update()
    {
        InputState input = m_Player.Input;

        //上にあるほど優先度が高い
        if (IsTryChangeState(!input.Direction.IsHorizontal(), m_Player.IdleState) ||  //左右入力がなくなったらIdle
            IsTryChangeState(input.IsGuard, m_Player.GuardState) ||
            IsTryChangeState(input.IsNormalAttack,m_Player.NormalAttackState) ||
            IsTryChangeState(input.IsLowAttack,m_Player.LowAttackState) ||
            IsTryChangeState(input.IsUpAttack,m_Player.UpAttackState) ||
            IsTryChangeState(input.IsForwardAttack,m_Player.ForwardAttackState))        {
            return;
        }
    }
}
