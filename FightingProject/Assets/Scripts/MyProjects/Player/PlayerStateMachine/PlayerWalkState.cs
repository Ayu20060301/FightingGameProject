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

        //移動停止
        if(m_Player.Input.Direction != Direction.Left && m_Player.Input.Direction != Direction.Right)
        {
            m_Player.ChangeState(m_Player.IdleState);
            return;
        }
    }
}
