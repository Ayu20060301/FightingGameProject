using UnityEngine;

//ジャンプ
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
        m_Player.RPC__PlayAnimation(Player.AnimationState.Jump);
    }

    public override void Update()
    {
        //着地したらIdleへ
        IsTryChangeState(m_Player.IsJumpFinished(), m_Player.IdleState);
    }
}
