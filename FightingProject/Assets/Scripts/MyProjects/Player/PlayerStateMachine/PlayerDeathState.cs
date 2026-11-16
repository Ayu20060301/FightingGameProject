using UnityEngine;

//死亡ステート
public class PlayerDeathState : PlayerState
{
    public PlayerDeathState(Player player) : base(player)
    {
        
    }

    public override void Enter()
    {
        //死亡アニメーションを再生
        m_Player.RPC_PlayAnimation(AnimationState.Death);
        m_Player.RPC_DropWeapon();
    }
}
