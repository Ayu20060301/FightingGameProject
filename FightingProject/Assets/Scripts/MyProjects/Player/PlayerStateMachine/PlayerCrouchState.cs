using UnityEngine;

/// <summary>
/// しゃがみステート
/// </summary>
public class PlayerCrouchState : PlayerState
{
    public PlayerCrouchState(Player player) : base(player)
    {

    }

    public override void Enter()
    {
        m_Player.SetAnimationState(Player.AnimationState.Crouch);
    }


    public override void Update()
    {
        //ガード
        if(m_Player.Input.IsGuard)
        {
            m_Player.ChangeState(m_Player.GuardState);
            return;
        }


        //しゃがみ入力がなくなった
        if(m_Player.Input.Direction != Direction.Down && m_Player.Input.Direction != Direction.DownLeft && m_Player.Input.Direction != Direction.DownRight)
        {
            m_Player.ChangeState(m_Player.IdleState);
        }
    }

}
