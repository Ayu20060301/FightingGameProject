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
    /// ステートを初期化する
    /// </summary>
    /// <param name="initialState"></param>
    public void Initialize(PlayerState initialState)
    {
        m_CurrentState = initialState;

        m_CurrentState.Enter();
    }

    /// <summary>
    /// ステート変更
    /// </summary>
    /// <param name="newState">次に遷移するステート</param>
    public void ChangeState(PlayerState newState)
    {
        if (m_CurrentState == newState) return;


        //現在のステートを終了
        m_CurrentState.Exit();

        //新しいステートへ変更
        m_CurrentState = newState;

        //新しいステートを開始
        m_CurrentState.Enter();
    }

    public void Update()
    {
        m_CurrentState.Update();
    }

}
