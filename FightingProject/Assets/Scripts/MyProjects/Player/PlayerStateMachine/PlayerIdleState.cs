using Unity.VisualScripting;
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
        //左右移動
        if(m_Player.Input.Direction == Direction.Left || 
           m_Player.Input.Direction == Direction.Right)
        {
            m_Player.ChangeState(m_Player.WalkState);
            return;
        }


        //しゃがみ
        if  (m_Player.Input.Direction == Direction.Down ||
           m_Player.Input.Direction == Direction.DownLeft ||
           m_Player.Input.Direction == Direction.DownRight)
        {
            m_Player.ChangeState(m_Player.CrouchState);
            return;
        }


        //弱パンチ
        if (m_Player.Input.IsLightPunch)
        {
            m_Player.ChangeState(m_Player.LightPunchState);
            return;
        }

        //強パンチ
        if (m_Player.Input.IsHeavyPunch)
        {
            m_Player.ChangeState(m_Player.HeavyPunchState);
            return;
        }

        //弱キック
        if (m_Player.Input.IsLightKick)
        {
            m_Player.ChangeState(m_Player.LightKickState);
            return;
        }

        //強キック
        if (m_Player.Input.IsHeavyKick)
        {
            m_Player.ChangeState(m_Player.HeavyKickState);
            return;
        }

        //ガード
        if (m_Player.Input.IsGuard)
        {
            m_Player.ChangeState(m_Player.GuardState);
            return;
        }

        //ジャンプ
        if(m_Player.Input.IsJump)
        {
            m_Player.ChangeState(m_Player.JumpState);
            return;
        }
    }
}
