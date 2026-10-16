using UnityEngine;

//弱キック
public class PlayerLightKickState : PlayerAttackState
{

    private readonly AttackTrailController m_Trail;

    public PlayerLightKickState(Player player) : base(player, Player.AnimationState.LightKick)
    {
    }


    protected override AttackData Data => m_Player.LightKickData;

    protected override bool IsFinished()
    {
        return m_Player.IsLightKickFinished();
    }
}
