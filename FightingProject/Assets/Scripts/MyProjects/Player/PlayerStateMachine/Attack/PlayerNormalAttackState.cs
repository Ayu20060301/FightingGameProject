
//通常攻撃
public class PlayerNormalAttackState : PlayerAttackState
{

    //段ごとのスロットとアニメーション
    private static readonly AttackSlot[] ComboSlots =
    {
        AttackSlot.NormalAttack,
        AttackSlot.NormalAttack2,
        AttackSlot.NormalAttack3,
        AttackSlot.NormalAttack4,
    };

    private static readonly AnimationState[] ComboAnimations =
    {
        AnimationState.NormalAttack,
        AnimationState.NormalAttack2,
        AnimationState.NormalAttack3,
        AnimationState.NormalAttack4,
    };

    private int m_ComboIndex;
    private bool m_IsComboQueued;

    private bool IsLastCombo => m_ComboIndex >= ComboSlots.Length - 1;


    public PlayerNormalAttackState(Player player) : base(player, AnimationState.NormalAttack)
    {
    }

    protected override AttackData Data => m_Player.GetAttackData(ComboSlots[m_ComboIndex]);

    protected override bool IsFinished()
    {
        return m_Player.IsAnimationFinished(ComboAnimations[m_ComboIndex]);
    }

    public override void Enter()
    {
        m_ComboIndex = 0;
        m_IsComboQueued = false;

        BeginAttack(ComboAnimations[m_ComboIndex]);
    }

    public override void Update()
    {
        if (IsTryAdvanceCombo()) return;

        base.Update();
    }

    private bool IsTryAdvanceCombo()
    {
        if (IsLastCombo) return false;

        //先行入力を記憶
        if(m_Player.IsNormalAttackPressed)
        {
            m_IsComboQueued = true;
        }

        if (!m_IsComboQueued) return false;

        AttackData data = Data;

        if (data == null || !data.IsComboWindow(ElapsedTicks)) return false;

        m_ComboIndex++;
        m_IsComboQueued = false;

        BeginAttack(ComboAnimations[m_ComboIndex]);
        return true;
    }

}
