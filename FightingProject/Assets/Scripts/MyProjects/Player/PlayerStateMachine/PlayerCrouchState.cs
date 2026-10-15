

//しゃがみ
public class PlayerCrouchState : PlayerState
{
    

    public PlayerCrouchState(Player player) : base(player)
    {

    }

    public override void Enter()
    {
        m_Player.RPC_PlayAnimation(Player.AnimationState.Crouch);
    }


    public override void Update()
    {
        InputState input = m_Player.Input;

        if (IsTryChangeState(input.IsGuard, m_Player.GuardState) ||  //ガード
            IsTryChangeState(!input.Direction.IsDown(), m_Player.IdleState))     //しゃがみ入力がなくなった
        {
            return;
        }
    }

}
