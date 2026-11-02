using UnityEngine;

//ジャンプ
public class PlayerJumpState : PlayerState
{

    public PlayerJumpState(Player player) : base(player)
    {

    }


    public override void Enter()
    {
      
        if(!m_Player.Jump())
        {
            m_Player.ChangeState(m_Player.IdleState);
            return;
        }

        //ジャンプアニメーション
        m_Player.RPC_PlayAnimation(AnimationState.Jump);
    }

    public override void Update()
    {

        //空中で追加ジャンプ
        if(m_Player.IsJumpPressed && m_Player.IsCanJump)
        {
            m_Player.Jump();
            m_Player.RPC_PlayAnimation(AnimationState.Jump);
            return;
        }

        //着地したらIdleへ
        IsTryChangeState(m_Player.IsJumpFinished(), m_Player.IdleState);
    }
}
