using Fusion;
using UnityEngine;

public class RoundManager : NetworkBehaviour
{

    public enum RoundPhase
    {
        Waiting,
        Fight,
        RoundEnd,
    }

    [Header("KOスローモーション")]
    [SerializeField]
    private float m_SlowTimeScale = 0.25f;  //減速後の速度
    [SerializeField]
    private float m_SlowSeconds = 1.0f;  //スローにする時間

    [Networked]
    public TickTimer SlowMotionTimer { get; private set; }

    [Header("制限時間(秒)")]
    [SerializeField]
    private float m_RoundSeconds = 99.0f;

    [Networked]
    public RoundPhase Phase { get; private set; }

    [Networked]
    public TickTimer RoundTimer { get; private set; }

    private const int RequiredPlayers = 2;

    //ラウンド終了時点の残り時間
    private float m_FrozenTime;

    /// <summary>
    /// 残り時間(秒)、UIから読む用
    /// </summary>
    public float RemainingTime
    {
        get
        {
            if (Phase == RoundPhase.Waiting) return m_RoundSeconds;
            if (Phase == RoundPhase.RoundEnd) return m_FrozenTime;
            return RoundTimer.RemainingTime(Runner) ?? 0.0f;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        switch (Phase)
        {
            case RoundPhase.Waiting:
                //2人揃ったら開始
                if (FindObjectsByType<Player>(FindObjectsSortMode.None).Length >= RequiredPlayers)
                {
                    StartRound();
                }
                break;
            case RoundPhase.Fight:
                CheckRoundEnd();
                break;
        }
    }

    private void StartRound()
    {
        RoundTimer = TickTimer.CreateFromSeconds(Runner, m_RoundSeconds);
        Phase = RoundPhase.Fight;
    }

    private void CheckRoundEnd()
    {
        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);

        //KOがでたら終了
        foreach (Player p in players)
        {
            if (p.IsKO)
            {
                EndRound(true);
                return;
            }
        }


        //時間切れ
        if (RoundTimer.Expired(Runner))
        {
            EndRound(false);
        }
    }


    private void EndRound(bool isKO)
    {
        m_FrozenTime = RoundTimer.RemainingTime(Runner) ?? 0.0f;
        Phase = RoundPhase.RoundEnd;

        //KOの時だけスローモーション開始
        if(isKO)
        {
            SlowMotionTimer = TickTimer.CreateFromSeconds(Runner, m_SlowSeconds);
        }
    }

    public override void Render()
    {
        bool isSlow = SlowMotionTimer.IsRunning && !SlowMotionTimer.Expired(Runner);
        Time.timeScale = isSlow ? m_SlowTimeScale : 1.0f;
    }


    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        //シーン離脱時にスローが残らないよう残す
        Time.timeScale = 1.0f;
    }

}