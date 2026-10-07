using UnityEngine;

public class PlayerLightKickState : PlayerState
{

    public PlayerLightKickState(Player player) : base(player)
    {

    }

    public override void Enter()
    {
        //弱キックアニメーション
        m_Player.SetAnimationState(Player.AnimationState.lightKick);
    }

    public override void Update()
    {
        //弱キック終了
        if(m_Player.IsLightKickFinished())
        {
            m_Player.ChangeState(m_Player.IdleState);
        }
    }
}
