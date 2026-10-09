using UnityEngine;

/// <summary>
/// 強キック
/// </summary>
public class PlayerHeavyKickState : PlayerState
{

    public PlayerHeavyKickState(Player player) : base(player)
    {

    }

    public override void Enter()
    {
        //強キックアニメーション
        m_Player.RPC__PlayAnimation(Player.AnimationState.heavyKick);
    }

    public override void Update()
    {
       //強キック終了
       if(m_Player.IsHeavyKickFinished())
        {
            m_Player.ChangeState(m_Player.IdleState);
        }
    }
}
