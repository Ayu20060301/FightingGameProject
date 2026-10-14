using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

//攻撃ステートの基底クラス
//アニメーション再生と、終了後のIdle復帰を担当する
public abstract class PlayerAttackState : PlayerState
{
    //この攻撃で再生するアニメーション
    private readonly Player.AnimationState m_Animation;


    protected PlayerAttackState(Player player, Player.AnimationState animation) : base(player)
    {
        m_Animation = animation;
    }


    /// <summary>
    /// 攻撃アニメーションが終了したか
    /// </summary>
    /// <returns></returns>
    protected abstract bool IsFinished();


    public override void Enter()
    {
        m_Player.RPC__PlayAnimation(m_Animation);
    }

    public override void Update()
    {
        //攻撃終了
        IsTryChangeState(IsFinished(), m_Player.IdleState);
    }
}
