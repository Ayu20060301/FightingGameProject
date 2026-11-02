

//攻撃ステートの基底クラス
//アニメーション再生と、終了後のIdle復帰を担当する
public abstract class PlayerAttackState : PlayerState
{
    //この攻撃で再生するアニメーション
    private readonly AnimationState m_Animation;

    //攻撃開始時のTick
    private int m_StartTick;

    //この攻撃が既にヒットしたか
    private bool m_IsHit;

    protected PlayerAttackState(Player player, AnimationState animation) : base(player)
    {
        m_Animation = animation;
    }

    /// <summary>
    /// 攻撃アニメーションが終了したか
    /// </summary>
    protected abstract bool IsFinished();

    /// <summary>
    /// この技の攻撃データ
    /// </summary>
    protected abstract AttackData Data { get; }

    /// <summary>
    /// 攻撃開始からの経過Tick
    /// </summary>
    protected int ElapsedTicks => m_Player.Runner.Tick - m_StartTick;

    /// <summary>
    /// この攻撃が既にヒットしたか(ヒット確認コンボ用)
    /// </summary>
    protected bool IsHit => m_IsHit;

    public override void Enter()
    {
        BeginAttack(m_Animation);
    }

    public override void Update()
    {
        //当たり判定
        UpdateHit();

        //攻撃終了
        IsTryChangeState(IsFinished(), m_Player.IdleState);
    }

    /// <summary>
    /// 発生から攻撃の間だけ当たり判定を行う
    /// </summary>
    private void UpdateHit()
    {
        if (m_IsHit) return;

        AttackData data = Data;
        if (data == null) return;

        if (!data.IsActive(ElapsedTicks)) return;

        m_IsHit = m_Player.CheckAttackHit(data);
    }

    /// <summary>
    /// 攻撃開始・再開処理
    /// </summary>
    /// <param name="animation">各アニメーション</param>
    protected void BeginAttack(AnimationState animation)
    {
        m_StartTick = m_Player.Runner.Tick;
        m_IsHit = false;

        m_Player.RPC_PlayAnimation(animation);
    }
}
