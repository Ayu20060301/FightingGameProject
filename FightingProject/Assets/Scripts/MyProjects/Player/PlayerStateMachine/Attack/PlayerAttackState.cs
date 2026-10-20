using System.Data;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

//攻撃ステートの基底クラス
//アニメーション再生と、終了後のIdle復帰を担当する
public abstract class PlayerAttackState : PlayerState
{
    //この攻撃で再生するアニメーション
    private readonly Player.AnimationState m_Animation;

    //攻撃開始時のTick
    private int m_StartTick;

    //この攻撃が既にヒットしたか
    private bool m_IsHit;

    protected PlayerAttackState(Player player, Player.AnimationState animation) : base(player)
    {
        m_Animation = animation;
    }


    /// <summary>
    /// 攻撃アニメーションが終了したか
    /// </summary>
    /// <returns></returns>
    protected abstract bool IsFinished();


    /// <summary>
    /// この技の攻撃データ
    /// </summary>
    protected abstract AttackData Data { get; }

    public override void Enter()
    {
        m_StartTick = m_Player.Runner.Tick;
        m_IsHit = false;

        m_Player.RPC_PlayAnimation(m_Animation);
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
        if (Data == null) return;

        int elapsed = m_Player.Runner.Tick - m_StartTick;

        if (!Data.IsActive(elapsed)) return;

        m_IsHit = m_Player.CheckAttackHit(Data);
    }
}
