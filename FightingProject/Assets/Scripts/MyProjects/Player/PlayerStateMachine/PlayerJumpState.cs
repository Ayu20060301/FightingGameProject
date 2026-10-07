using UnityEngine;

public class PlayerJumpState : PlayerState
{

    public PlayerJumpState(Player player) : base(player)
    {

    }


    public override void Enter()
    {
        //ジャンプ開始
        m_Player.Jump();

        //ジャンプアニメーション
        m_Player.SetAnimationState(Player.AnimationState.Jump);
    }

    public override void Update()
    {
        /*
        //着地したらIdleへ
        if(m_Player.IsGrounded)
        {
            m_Player.ChangeState(m_Player.IdleState);
        }
        */

        if(m_Player.IsJumpFinished())
        {
            m_Player.ChangeState(m_Player.IdleState);
        }

    }

}
