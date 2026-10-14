using UnityEngine;

//中キック
public class PlayerMidKickState : PlayerAttackState
{
    public PlayerMidKickState(Player player) : base(player,Player.AnimationState.MidKick)
    {

    }

    protected override bool IsFinished()
    {
        return m_Player.IsMidKickFinished();
    }
}
