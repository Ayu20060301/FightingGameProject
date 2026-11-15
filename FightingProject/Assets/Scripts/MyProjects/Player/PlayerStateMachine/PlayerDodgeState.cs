
//回避
public class PlayerDodgeState : PlayerState
{

    private AnimationState m_DodgeAnimation;

    public PlayerDodgeState(Player player) : base(player)
    {

    }

    public override void Enter()
    {
        m_DodgeAnimation = m_Player.IsDodgeForward ? AnimationState.Dodge_Forward
                                                   : AnimationState.Dodge_Back;

        m_Player.StartDodgeMove();

        //回避アニメーションを再生
        m_Player.RPC_PlayAnimation(m_DodgeAnimation);
    }

    public override void Update()
    {
        //回避アニメーション終了後に待機状態へ
        if(m_Player.IsAnimationFinished(m_DodgeAnimation))
        {
            m_Player.ChangeState(m_Player.IdleState);
        }
    }

}
