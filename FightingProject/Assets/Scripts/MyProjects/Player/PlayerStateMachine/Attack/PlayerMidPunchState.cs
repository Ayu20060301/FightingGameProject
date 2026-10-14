using System.Security.Authentication.ExtendedProtection;
using UnityEngine;

//中パンチ
public class PlayerMidPunchState : PlayerAttackState
{
    public PlayerMidPunchState(Player player) : base(player,Player.AnimationState.MidPunch)
    {

    }

    protected override bool IsFinished()
    {
        return m_Player.IsMidPunchFinished();
    }
}
