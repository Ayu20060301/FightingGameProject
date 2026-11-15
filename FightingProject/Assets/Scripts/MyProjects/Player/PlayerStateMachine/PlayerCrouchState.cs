

//しゃがみ
public class PlayerCrouchState : PlayerState
{
    private enum Phase
    {
        In,     //しゃがみ動作中
        Idle,    //しゃがみ維持
        Out,    //立ち上がり動作中
    }

    private Phase m_Phase;

    public PlayerCrouchState(Player player) : base(player)
    {

    }

    public override void Enter()
    {
        m_Phase = Phase.In;
        m_Player.RPC_PlayAnimation(AnimationState.Crouch_In);
    }


    public override void Update()
    {
        InputState input = m_Player.Input;
        bool isCrouchHeld = input.Direction.IsDown();

        switch(m_Phase)
        {
            case Phase.In:
                //しゃがみ攻撃
                if(m_Player.IsNormalAttackPressed)
                {
                    m_Player.ChangeState(m_Player.CrouchAttackState);
                    return;
                }

                //入り途中で入力を話した → 解除へ
                if(!isCrouchHeld)
                {
                    StartOut();
                    return;
                }

                //入りモーション終了
                if (m_Player.IsAnimationFinished(AnimationState.Crouch_In))
                {
                    m_Phase = Phase.Idle;
                    m_Player.RPC_PlayAnimation(AnimationState.Crouch_Idle);
                }
                break;
            case Phase.Idle:
                if(m_Player.IsNormalAttackPressed)
                {
                    m_Player.ChangeState(m_Player.CrouchAttackState);
                    return;
                }

                //しゃがみ入力がなくなった
                if(!isCrouchHeld)
                {
                    StartOut();
                }
                break;
            case Phase.Out:
                //立ち上がり中に再入力 → もう一度しゃがむ
                if(isCrouchHeld)
                {
                    m_Phase = Phase.In;
                    m_Player.RPC_PlayAnimation(AnimationState.Crouch_In);
                    return;
                }

                //立ち上がり終了 → Idleへ
                if (m_Player.IsAnimationFinished(AnimationState.Crouch_Out))
                {
                    m_Player.ChangeState(m_Player.IdleState);
                }
                break;
        }


    }

    private void StartOut()
    {

        m_Phase = Phase.Out;
        m_Player.RPC_PlayAnimation(AnimationState.Crouch_Out);
    }

}
