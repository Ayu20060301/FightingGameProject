

//しゃがみ
public class PlayerCrouchState : PlayerState
{
    

    public PlayerCrouchState(Player player) : base(player)
    {

    }

    public override void Enter()
    {
        m_Player.RPC_PlayAnimation(AnimationState.Crouch);
    }


    public override void Update()
    {
        InputState input = m_Player.Input;


        if(m_Player.Input.IsLowAttack)
        {
            m_Player.ChangeState(m_Player.CrouchAttackState);   
        }

        if (IsTryChangeState(input.IsGuard, m_Player.GuardState) ||  //ガード
            IsTryChangeState(!input.Direction.IsDown(), m_Player.IdleState))     //しゃがみ入力がなくなった
        {
            return;
        }
    }

}
