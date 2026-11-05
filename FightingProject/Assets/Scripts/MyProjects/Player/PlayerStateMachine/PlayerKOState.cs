using UnityEngine;

public class PlayerKOState : PlayerState
{
    public PlayerKOState(Player player) : base(player)
    {
        m_Player = player;
    }

    public override void Enter()
    {
        //死亡アニメーションを再生
        m_Player.RPC_PlayAnimation(AnimationState.Death);
        m_Player.RPC_DropWeapon();
    }




}
