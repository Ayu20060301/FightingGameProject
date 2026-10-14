
using UnityEngine;

public class PlayerIdleState : PlayerState
{

    public PlayerIdleState(Player player) : base(player)
    {

    }


    public override void Enter()
    {
        m_Player.RPC__PlayAnimation(Player.AnimationState.Idle);
    }

    public override void Update()
    {
        InputState input = m_Player.Input;

        //上にあるほど優先度が高い
        if (IsTryChangeState(input.Direction.IsHorizontal(), m_Player.WalkState) ||    //左右移動
            IsTryChangeState(input.Direction.IsDown(), m_Player.CrouchState) ||  //しゃがみ
            IsTryChangeState(input.IsLightPunch, m_Player.LightPunchState) ||
            IsTryChangeState(input.IsHeavyPunch, m_Player.HeavyPunchState) ||
            IsTryChangeState(input.IsLightKick, m_Player.LightKickState) ||
            IsTryChangeState(input.IsHeavyKick, m_Player.HeavyKickState) ||
            IsTryChangeState(input.IsMidKick, m_Player.MidKickState) ||
            IsTryChangeState(input.IsMidPunch, m_Player.MidPunchState) ||
            IsTryChangeState(input.IsGuard, m_Player.GuardState) ||
            IsTryChangeState(input.IsJump, m_Player.JumpState))
        {
            return;
        }
    }
}
