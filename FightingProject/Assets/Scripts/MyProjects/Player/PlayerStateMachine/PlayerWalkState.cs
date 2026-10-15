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
            IsTryChangeState(input.IsLightPunch, m_Player.LightPunchState) ||
            IsTryChangeState(input.IsHeavyPunch, m_Player.HeavyPunchState) ||
            IsTryChangeState(input.IsLightKick, m_Player.LightKickState) ||
            IsTryChangeState(input.IsMidKick, m_Player.MidKickState) ||
            IsTryChangeState(input.IsMidPunch, m_Player.MidPunchState) ||
            IsTryChangeState(input.IsHeavyKick, m_Player.HeavyKickState))
        {
            return;
        }
    }
}
