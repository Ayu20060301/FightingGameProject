public class PlayerCrouchAttackState : PlayerAttackState
{
    public PlayerCrouchAttackState(Player player)
        : base(player, AnimationState.CrouchAttack)
    {
    }

    protected override AttackData Data => m_Player.CrouchAttackData;

    public override void Update()
    {
        //当たり判定
        UpdateHit();

        //開始直後は前のアニメを見て終了と誤判定しないよう、2Tick待つ
        if (ElapsedTicks < 2 || !IsFinished()) return;

        if (m_Player.Input.Direction.IsDown())
        {
            m_Player.CrouchIdleState.SetSkipIn();
            m_Player.ChangeState(m_Player.CrouchIdleState);
        }
        else
        {
            m_Player.ChangeState(m_Player.IdleState);
        }
    }

    protected override bool IsFinished()
    {
        return m_Player.IsCrouchAttackFinished();
    }
}