using UnityEngine;

public class PlayerGuardState : PlayerState
{

   

    public PlayerGuardState(Player player) : base(player)
    {

    }

    public override void Enter()
    {
        m_Player.RPC__PlayAnimation(Player.AnimationState.Guard);
    }

    public override void Update()
    {
        //ガード解除
        if(!m_Player.Input.IsGuard)
        {
            m_Player.ChangeState(m_Player.IdleState);
            return;
        }
    }
}
