
/// <summary>
/// 歩行ステート
/// </summary>
public class PlayerWalkState : PlayerState
{
    //現在再生中のアニメーション
    private AnimationState m_CurrentAnim;

    public PlayerWalkState(Player player) : base(player)
    {

    }

    public override void Enter()
    {
        m_CurrentAnim = GetWalkAnimation();
        m_Player.RPC_PlayAnimation(AnimationState.Walk);
    }


    public override void Update()
    {
        InputState input = m_Player.Input;

        //上にあるほど優先度が高い
        if (IsTryChangeState(!input.Direction.IsHorizontal(), m_Player.IdleState) ||  //左右入力がなくなったらIdle
            IsTryChangeState(input.IsGuard, m_Player.GuardState) ||
            IsTryChangeState(input.IsNormalAttack,m_Player.NormalAttackState) ||
            IsTryChangeState(input.IsLowAttack,m_Player.LowAttackState) ||
            IsTryChangeState(input.IsUpAttack,m_Player.UpAttackState) ||
            IsTryChangeState(input.IsForwardAttack,m_Player.ForwardAttackState))        {
            return;
        }


        //前進⇔後退が切り替わった時だけアニメーションを再生しなおす
        AnimationState next = GetWalkAnimation();
        if(next != m_CurrentAnim)
        {
            m_CurrentAnim = next;
            m_Player.RPC_PlayAnimation(m_CurrentAnim);
        }
    }


    /// <summary>
    /// 入力方向に応じた歩行アニメーションを返す
    /// </summary>
    private AnimationState GetWalkAnimation()
    {
        return m_Player.IsMovingBackward
            ? AnimationState.BackWalk
            : AnimationState.Walk;
    }

}
