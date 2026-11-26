public class PlayerCrouchIdleState : PlayerState
{
    private enum Phase
    {
        In,     //しゃがみ動作中
        Idle,   //しゃがみ維持
        Out,    //立ち上がり動作中
    }

    private Phase m_Phase;
    private bool m_SkipIn;   

    public PlayerCrouchIdleState(Player player) : base(player)
    {
    }

    public void SetSkipIn() => m_SkipIn = true;

    public override void Enter()
    {
        if (m_SkipIn)
        {
            m_SkipIn = false;
            m_Phase = Phase.Idle;
            m_Player.RPC_PlayAnimation(AnimationState.Crouch_Idle);
            return;
        }

        m_Phase = Phase.In;
        m_Player.RPC_PlayAnimation(AnimationState.Crouch_In);
    }

    public override void Update()
    {
        InputState input = m_Player.Input;
        bool isCrouchHeld = input.Direction.IsDown();

        switch (m_Phase)
        {
            case Phase.In:
                if (m_Player.IsNormalAttackPressed)
                {
                    m_Player.ChangeState(m_Player.CrouchAttackState);
                    return;
                }

                //入り途中で入力を離した → 解除へ
                if (!isCrouchHeld)
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
                //押しっぱなしで連続攻撃にならないよう、押した瞬間だけ判定
                if (m_Player.IsNormalAttackPressed)
                {
                    m_Player.ChangeState(m_Player.CrouchAttackState);
                    return;
                }

                //しゃがみ入力がなくなった
                if (!isCrouchHeld)
                {
                    StartOut();
                }
                break;

            case Phase.Out:
                //立ち上がり中に再入力 → もう一度しゃがむ
                if (isCrouchHeld)
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
