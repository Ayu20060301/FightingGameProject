using UnityEngine;

//掴み
public class PlayerGrabState : PlayerState
{
    private int m_Tick;
    private bool m_IsGrabbed;  //掴み成立済みか
    private int m_GrabbedTick; //成立したTick

    public PlayerGrabState(Player player) : base(player)
    {

    }


    public override void Enter()
    {
        m_Tick = 0;
        m_IsGrabbed = false;
        m_GrabbedTick = 0;
        m_Player.RPC_PlayAnimation(Player.AnimationState.Grab);
    }

    public override void Update()
    {
        GrabData data = m_Player.GrabData;

        //接続中に1回だけ判定(成立したら以降は判定しない)
        if(!m_IsGrabbed && data.IsActive(m_Tick))
        {
            m_IsGrabbed = m_Player.CheckGrabHit(data);

            if(m_IsGrabbed)
            {
                m_GrabbedTick = m_Tick;
                m_Player.RPC_PlayAnimation(Player.AnimationState.Throw);
            }

        }

        m_Tick++;

        //成立時は「成立からgrabHoldTickss後」、空振り時は空振り硬直後に終了
        int endTick = m_IsGrabbed
            ? m_GrabbedTick + data.grabHoldTicks
            : data.startupTicks + data.activeTicks + data.whiffRecoveryTicks; 

        if(m_Tick >= endTick)
        {
            m_Player.ChangeState(m_Player.IdleState);
        }
    }
}
