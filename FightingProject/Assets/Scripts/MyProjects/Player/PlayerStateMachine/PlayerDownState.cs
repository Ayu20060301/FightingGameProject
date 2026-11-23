using UnityEngine;

//ダウンステート
public class PlayerDownState : PlayerState
{
    public PlayerDownState(Player player) : base(player)
    {

    }

    public override void Enter()
    {
        //ダウンアニメーションを再生
        m_Player.RPC_PlayAnimation(AnimationState.Down_In);
    }
}
