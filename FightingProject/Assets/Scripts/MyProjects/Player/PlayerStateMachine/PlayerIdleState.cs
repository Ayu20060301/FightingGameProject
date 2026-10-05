using Unity.VisualScripting;
using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(Player player) : base(player)
    {

    }


    public override void Enter()
    {
        m_Player.SetAnimationState(Player.AnimationState.Idle);
    }

    public override void Update()
    {
      //左右移動
      if(m_Player.Input.Direction == Direction.Left || 
         m_Player.Input.Direction == Direction.Right)
      {
            m_Player.ChangeState(m_Player.WalkState);
            return; ;
      }


      //しゃがみ
      if(m_Player.Input.Direction == Direction.Down ||
         m_Player.Input.Direction == Direction.DownLeft ||
         m_Player.Input.Direction == Direction.DownRight)
        {
            m_Player.ChangeState(m_Player.CrouchState);
            return;
        }


      //ガード
      if(m_Player.Input.IsGuard)
        {
            m_Player.ChangeState(m_Player.GuardState);
            return;
        }

    }
}
