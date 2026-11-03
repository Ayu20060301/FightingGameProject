

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

        //空中で攻撃入力したら攻撃へ
        if(m_Player.Input.IsNormalAttack)
        {
            m_Player.ChangeState(m_Player.NormalAttackState);
            return;
        }

        if(m_Player.Input.IsLowAttack)
        {
            m_Player.ChangeState(m_Player.LowAttackState);
            return;
        }


        //着地したらIdleへ
        IsTryChangeState(m_Player.IsJumpFinished(), m_Player.IdleState);
    }
}
