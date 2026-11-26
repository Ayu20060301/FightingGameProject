
public abstract class PlayerAttackState : PlayerState
{
    private readonly AnimationState m_Animation;
    private int m_StartTick;
    private bool m_IsHit;

    protected PlayerAttackState(Player player, AnimationState animation) : base(player)
    {
        m_Animation = animation;
    }

    protected abstract bool IsFinished();
    protected abstract AttackData Data { get; }

    protected int ElapsedTicks => m_Player.Runner.Tick - m_StartTick;
    protected bool IsHit => m_IsHit;

    public override void Enter()
    {
        BeginAttack(m_Animation);
    }

    public override void Update()
    {
        UpdateHit();
        IsTryChangeState(IsFinished(), m_Player.IdleState);
    }

    
    protected void UpdateHit()
    {
        if (m_IsHit) return;

        AttackData data = Data;
        if (data == null) return;

        if (!data.IsActive(ElapsedTicks)) return;

        m_IsHit = m_Player.CheckAttackHit(data);
    }

    protected void BeginAttack(AnimationState animation)
    {
        m_StartTick = m_Player.Runner.Tick;
        m_IsHit = false;

        m_Player.RPC_PlayAnimation(animation);
    }
}