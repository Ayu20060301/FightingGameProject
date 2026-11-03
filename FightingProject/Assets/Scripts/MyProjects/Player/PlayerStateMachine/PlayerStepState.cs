using UnityEngine;

//ステップ(前ステップ・バックステップ)
public class PlayerStepState : PlayerState
{

    private int m_Dir;   //ステップの方向
    private float m_Remaining; //ステップの残り時間

    public int Dir => m_Dir;

    public PlayerStepState(Player player) : base(player)
    {

    }


    public void Setup(int dir,float duration)
    {
        m_Dir = dir;
        m_Remaining = duration;
    }


    public override void Enter()
    {
        bool isForward = m_Dir == (int)m_Player.FacingDir;
        m_Player.RPC_PlayAnimation(isForward ? AnimationState.Step : AnimationState.BackStep);
    }

    public override void Update()
    {
        m_Remaining -= m_Player.Runner.DeltaTime;

        if(m_Remaining <= 0.0f)
        {
            m_Player.ChangeState(m_Player.IdleState);
        }

    }

}
