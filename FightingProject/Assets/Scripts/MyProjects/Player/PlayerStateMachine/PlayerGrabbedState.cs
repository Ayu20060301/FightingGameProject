using UnityEngine;

//掴まれる側
public class PlayerGrabbedState : PlayerState
{
    private Player m_Attacker;
    private GrabData m_Data;
    private int m_Tick;

    public PlayerGrabbedState(Player player) : base(player)
    {

    }

    //掴まれる情報を設定
    public void Setup(Player attacker,GrabData data)
    {
        m_Attacker = attacker;
        m_Data = data;
    }

    public override void Enter()
    {
        m_Tick = 0;
        m_Player.RPC_PlayAnimation(Player.AnimationState.Grabbed);
    }


    public override void Update()
    {
        m_Tick++;

        //掴み時間が終わったら投げのダメージを適用して喰らい状態へ
        if(m_Tick >= m_Data.grabHoldTicks)
        {
            //m_Player.
        }

    }

}
