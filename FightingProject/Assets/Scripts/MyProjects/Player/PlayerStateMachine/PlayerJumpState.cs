

//ジャンプ
using System.Xml;

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

        if(m_Player.Input.IsNormalAttack || m_Player.Input.IsForwardAttack)
        {
            m_Player.ChangeState(m_Player.NormalAttackState);
        }

        //下入力
        if(m_Player.Input.IsLowAttack)
        {
            m_Player.ChangeState(m_Player.AirAttackState);
            return;
        }


        //着地したらIdleへ
        IsTryChangeState(m_Player.IsJumpFinished(), m_Player.IdleState);
    }
}
