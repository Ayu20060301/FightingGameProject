

//ジャンプ

public class PlayerJumpState : PlayerState
{

    private enum Phase
    {
        Air,     //踏み切り～滞空(In)
        Land,  //着地モーション(Out)
    }

    private Phase m_Phase;
    private AnimationState m_InAnimation;
    private AnimationState m_OutAnimation;
    private bool m_HasLeftGround;   //一度でも地面を離れたか

    //着地モーション中か
    public bool IsLanding => m_Phase == Phase.Land;


    public PlayerJumpState(Player player) : base(player)
    {

    }


    public override void Enter()
    {
        m_Phase = Phase.Air;
       
        //ジャンプアニメーションを選択
        if(m_Player.IsMovingForward)
        {
            m_InAnimation = AnimationState.JumpForward_In;
            m_OutAnimation = AnimationState.JumpForward_Out;
        }
        else if(m_Player.IsMovingBackward)
        {
            m_InAnimation = AnimationState.JumpBack_In;
            m_OutAnimation = AnimationState.JumpBack_Out;
        }
        else
        {
            m_InAnimation = AnimationState.Jump_In;
            m_OutAnimation = AnimationState.Jump_Out;
        }

        //ジャンプ入力を消費し、踏み切り時の横方向を固定
        m_Player.ConsumeJumpInput();
        m_Player.StartJumpMove();


        m_Player.RPC_PlayAnimation(m_InAnimation);

    }

    public override void Update()
    {
        switch(m_Phase)
        {
            case Phase.Air:
                //空中攻撃
                if(m_Player.Input.IsNormalAttack || m_Player.Input.IsNormalAttack)
                {
                    m_Player.ChangeState(m_Player.NormalAttackState);
                    return;
                }

                //下入力
                if(m_Player.Input.IsLowAttack)
                {
                    m_Player.ChangeState(m_Player.AirAttackState);
                    return;
                }

                else if(m_Player.IsAnimationFinished(m_InAnimation))
                {
                    m_Phase = Phase.Land;
                    m_Player.EndJumpMove();
                    m_Player.RPC_PlayAnimation(m_OutAnimation);
                }
                break;
            case Phase.Land:
                //着地モーション終了後にIdleへ
                if(m_Player.IsAnimationFinished(m_OutAnimation))
                {
                    m_Player.ChangeState(m_Player.IdleState);
                }
                break;
        }
    }

    public override void Exit()
    {
        m_Player.EndJumpMove();
    }
}
