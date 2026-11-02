using UnityEngine;

//通常攻撃
public class PlayerNormalAttackState : PlayerAttackState
{

    //コンボ受付の開始処理(アニメの正規化時間)
    private const float ComboWindowStart = 0.5f;


    //段ごとのアニメーション
    private static readonly AnimationState[] ComboAnimations =
    {
        AnimationState.NormalAttack,
        AnimationState.NormalAttack2,
        AnimationState.NormalAttack3,
    };


    //ヒットしたときだけ次の段へ進めるか(ヒット確認コンボ)
    private const bool RequireHitToCombo = false;

    private int m_ComboIndex;
    private bool m_IsComboBuffered;


    public PlayerNormalAttackState(Player player) : base(player, AnimationState.NormalAttack)
    {

    }

    protected override AttackData Data => m_Player.GetNormalComboData(m_ComboIndex);


    public override void Enter()
    {
        //先にリセットする
        m_ComboIndex = 0;
        m_IsComboBuffered = false;

        base.Enter();
    }


    public override void Update()
    {
        //攻撃ボタンは先行入力として保存しておく
        if(m_Player.IsNormalAttackPressed)
        {
            m_IsComboBuffered = true;
        }

        //受付時間内なら次の段へ進む
        if(m_IsComboBuffered && IsCanAdvanceCombo())
        {
            AdvanceCombo();
            return;
        }

        //受付期限を過ぎた先行入力は破棄する
        if(m_IsComboBuffered && IsComboWindowExpired())
        {
            m_IsComboBuffered = false;
        }

        base.Update();
    }


    private bool IsCanAdvanceCombo()
    {
        if (!IsHasNextCombo()) return false;
        if (RequireHitToCombo && !IsHit) return false;

        return Data.IsComboWindow(ElapsedTicks);
    }

    private bool IsComboWindowExpired()
    {
        AttackData data = Data;
        return data.comboEndTicks > 0 && ElapsedTicks >= data.comboEndTicks;
    }

    private bool IsHasNextCombo()
    {
        int next = m_ComboIndex + 1;
        return next < ComboAnimations.Length && next < m_Player.NormalComboLength;
    }

    private void AdvanceCombo()
    {
        m_ComboIndex++;
        m_IsComboBuffered = false;


        BeginAttack(ComboAnimations[m_ComboIndex]);
    }

    protected override bool IsFinished()
    {
        //現在の段のアニメーションが終わったか
        switch(m_ComboIndex)
        {
            case 0:
                return m_Player.IsNormalAttackFinished();
            case 1:
                return m_Player.IsNormalAttack2Finished();
            default:
                return m_Player.IsNormalAttack3Finished();
        }
    }
}
