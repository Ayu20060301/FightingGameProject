using UnityEngine;

/// <summary>
/// 弱パンチ
/// </summary>
public class PlayerLightPunchState : PlayerState
{
    public PlayerLightPunchState(Player player) : base(player)
    {

    }

    public override void Enter()
    {
        //弱パンチアニメーション
        m_Player.SetAnimationState(Player.AnimationState.lightPunch);
    }

    public override void Update()
    {
        //弱パンチ終了
       if(m_Player.IsLightPunchFinished())
        {
            m_Player.ChangeState(m_Player.IdleState);
        }
    }
}
