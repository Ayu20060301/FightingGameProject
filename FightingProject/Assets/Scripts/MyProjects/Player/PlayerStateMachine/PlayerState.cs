
/// <summary>
/// プレイヤーのステート基底クラス
/// </summary>
public abstract class PlayerState
{
    //このStateを所有しているPlayer
    protected Player m_Player;

    /// <summary>
    /// ステートの種類
    /// </summary>
    //public abstract PlayerStateType StateType { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="player">プレイヤー</param>
    protected PlayerState(Player player)
    {
        m_Player = player;
    }

    /// <summary>
    /// ステート開始時
    /// </summary>
    public virtual void Enter()
    {
    }

    /// <summary>
    /// ステート更新
    /// </summary>
    public virtual void Update()
    {
    }

    /// <summary>
    /// ステート終了時
    /// </summary>
    public virtual void Exit()
    {
    }

}