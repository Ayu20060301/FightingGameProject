using UnityEngine;

/// <summary>
/// 強パンチ
/// </summary>
public class PlayerHeavyPunchState : PlayerState
{
    public PlayerHeavyPunchState(Player player) : base(player)
    {
    
    }


    public override void Enter()
    {
        //強パンチアニメーション
        m_Player.SetAnimationState(Player.AnimationState.heavyPunch);
    }

    public override void Update()
    {
        //強パンチ終了
        if(m_Player.IsHeavyPunchFinished())
        {
            m_Player.ChangeState(m_Player.IdleState);
        }
    }
}
