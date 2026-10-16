using Fusion;
using UnityEngine;

//攻撃を受けた際の状態
public class PlayerHitState : PlayerState
{

    private Player.AnimationState m_AnimationState;
    private int m_StunTicks;
    private TickTimer m_Timer;


    public PlayerHitState(Player player) : base(player)
    {

    }


    public void Setup(Player.AnimationState animation,int stunTicks)
    {
        m_AnimationState = animation;
        m_StunTicks = stunTicks;
    }

    public override void Enter()
    {
        m_Player.RPC_PlayAnimation(m_AnimationState);
        m_Timer = TickTimer.CreateFromTicks(m_Player.Runner, m_StunTicks);
    }

    public override void Update()
    {
        //硬直中は何もしない
        if (!m_Timer.Expired(m_Player.Runner)) return;

        //硬直が終わったらIdleへ
        m_Player.ChangeState(m_Player.IdleState);
    }

    public void Restart()
    {
        Enter();
    }

}
