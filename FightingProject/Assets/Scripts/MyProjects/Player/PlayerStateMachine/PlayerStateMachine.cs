using UnityEngine;

/// <summary>
/// プレイヤーのステートを管理するクラス
/// </summary>
public class PlayerStateMachine
{
    //現在のステート
    private PlayerState m_CurrentState;

    /// <summary>
    /// 現在のステート
    /// </summary>
    public PlayerState CurrentState => m_CurrentState;

    /// <summary>
    /// ステート変更
    /// </summary>
    /// <param name="newState">次に遷移するステート</param>
    public void ChangeState(PlayerState newState)
    {

        if (newState == null) return;

        //同じステートなら変更しない
        if (m_CurrentState == newState) return;

        //現在のステートを終了
        m_CurrentState?.Exit();

        //新しいステートへ変更
        m_CurrentState = newState;

        //新しいステートを開始
        m_CurrentState.Enter();
    }

    /// <summary>
    /// 現在のステートを更新する
    /// </summary>
    public void Update()
    {
        m_CurrentState.Update();
    }
}
