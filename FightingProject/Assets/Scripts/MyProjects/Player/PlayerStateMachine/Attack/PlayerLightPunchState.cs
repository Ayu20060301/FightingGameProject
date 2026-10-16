using UnityEngine;

//弱パンチ
public class PlayerLightPunchState : PlayerAttackState
{
    public PlayerLightPunchState(Player player) : base(player, Player.AnimationState.LightPunch)
    {

    }

    protected override AttackData Data => m_Player.LightPunchData;

    protected override bool IsFinished()
    {
        return m_Player.IsLightPunchFinished();
    }
}
