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
        m_Player.SetAnimationState(Player.AnimationState.Walk);
    }


    public override void Update()
    {
    
        //ガード
        if(m_Player.Input.IsGuard)
        {
            m_Player.ChangeState(m_Player.GuardState);
            return;
        }

        // 弱パンチ
        if (m_Player.Input.IsLightPunch)
        {
            m_Player.ChangeState(m_Player.LightPunchState);
            return;
        }

        // 強パンチ
        if (m_Player.Input.IsHeavyPunch)
        {
            m_Player.ChangeState(m_Player.HeavyPunchState);
            return;
        }

        // 弱キック
        if (m_Player.Input.IsLightKick)
        {
            m_Player.ChangeState(m_Player.LightKickState);
            return;
        }

        // 強キック
        if (m_Player.Input.IsHeavyKick)
        {
            m_Player.ChangeState(m_Player.HeavyKickState);
            return;
        }

        //移動停止
        if (m_Player.Input.Direction != Direction.Left && m_Player.Input.Direction != Direction.Right)
        {
            m_Player.ChangeState(m_Player.IdleState);
            return;
        }
    }
}
