
//プレイヤーのステート基底クラス
public abstract class PlayerState
{
    //このStateを所有しているPlayer
    protected Player m_Player;

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

  
    //条件を満たしていれば遷移する
    protected bool IsTryChangeState(bool condition, PlayerState next)
    {
        if (!condition) return false;

        m_Player.ChangeState(next);
        return true;

    }

}