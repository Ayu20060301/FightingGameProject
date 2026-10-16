using Fusion;
using UnityEngine;

//ガード中に攻撃を受けた場合の状態
public class PlayerGuardHitState : PlayerState
{
    private Player.AnimationState m_AnimationState;
    private int m_StunTicks;
    private TickTimer m_Timer;

    public PlayerGuardHitState(Player player) : base(player)
    {

    }

    public void Setup(int stunTicks)
    {
        m_StunTicks = stunTicks;
    }

    public override void Enter()
    {
        m_Player.RPC_PlayAnimation(Player.AnimationState.GuardHit);
        m_Timer = TickTimer.CreateFromTicks(m_Player.Runner, m_StunTicks);
    }

    /// <summary>
    /// ガード中に再度攻撃を受けた際に、アニメーションと硬直をやり直す
    /// </summary>
    public void Restart()
    {
        Enter();
    }

    public override void Update()
    {
        //硬直中は何もしない
        if (!m_Timer.Expired(m_Player.Runner)) return;

        InputState input = m_Player.Input;

        //ガード入力を続けていればガードへ、そうれなければIdleへ
        if (IsTryChangeState(input.IsGuard, m_Player.GuardState)) return;

        m_Player.ChangeState(m_Player.IdleState);
    }
}
