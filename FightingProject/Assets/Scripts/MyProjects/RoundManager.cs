using Fusion;
using UnityEngine;

public class RoundManager : NetworkBehaviour
{

    public enum RoundPhase
    {
        Waiting,
        RoundStart, //「Round 1表示など
        Fight,
        RoundEnd,   //KO/タイムアップ後、次ラウンドまでの待ち
        MatchEnd,   //試合終了
    }

    public enum RoundResult
    {
        None,
        KO,
        DoubleKO,
        TimeUp,
        TimeUpDraw,
    }

    [Networked]
    public RoundResult LastResult { get; private set; }

    [Header("KOスローモーション")]
    [SerializeField]
    private float m_SlowTimeScale = 0.25f;  //減速後の速度
    [SerializeField]
    private float m_SlowSeconds = 1.0f;  //スローにする時間

    [Header("制限時間(秒)")]
    [SerializeField]
    private float m_RoundSeconds = 60.0f;


    [Header("ラウンド設定")]
    [SerializeField]
    private int m_WinsRequired = 2;            //先取ラウンド数(2本先取)
    [SerializeField]
    private float m_RoundStartSeconds = 2.0f; //ラウンド表示の時間
    [SerializeField]
    private float m_RoundEndSeconds = 3.0f; //決着後、次のラウンドまでの待ち

    [Networked]
    public TickTimer SlowMotionTimer { get; private set; }
    [Networked]
    public RoundPhase Phase { get; private set; }
    [Networked]
    public TickTimer RoundTimer { get; private set; }
    [Networked]
    public TickTimer PhaseTimer { get; private set; }


    [Networked]
    public int CurrentRound { get; private set; }
    [Networked]
    public int P1Wins { get; private set; }
    [Networked]
    public int P2Wins { get; private set; }
    [Networked]
    public int LastWinnerIndex { get; private set; }
    [Networked]
    public float FrozenTime { get; private set; }

    private const int RequiredPlayers = 2;

  

    /// <summary>
    /// 残り時間(秒)、UIから読む用
    /// </summary>
    public float RemainingTime
    {
        get
        {
            switch(Phase)
            {
                case RoundPhase.Waiting:
                case RoundPhase.RoundStart:
                    return m_RoundSeconds;
                case RoundPhase.RoundEnd:
                case RoundPhase.MatchEnd:
                    return FrozenTime;
                default:
                    return RoundTimer.RemainingTime(Runner) ?? 0.0f;
            }
        }
    }


    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;

        CurrentRound = 1;
        P1Wins = 0;
        P2Wins = 0;
        LastWinnerIndex = -1;
        FrozenTime = m_RoundSeconds;
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
                    AssignPlayerIndices();
                    BeginRoundStart();
                }
                break;
            case RoundPhase.RoundStart:
                if(PhaseTimer.Expired(Runner))
                {
                    StartFight();
                }
                break;
            case RoundPhase.Fight:
                CheckRoundEnd();
                break;
            case RoundPhase.RoundEnd:
                if(PhaseTimer.Expired(Runner))
                {
                    OnORoundEndFinished();
                }
                break;
        }
    }

    private void AssignPlayerIndices()
    {
        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);
        System.Array.Sort(players, (a, b) => a.StartX.CompareTo(b.StartX));

        for(int i = 0; i < players.Length; i++)
        {
            players[i].PlayerIndex = i;
        }
    }


    private void BeginRoundStart()
    {
        ResetPlayers();
        PhaseTimer = TickTimer.CreateFromSeconds(Runner, m_RoundStartSeconds);
        Phase = RoundPhase.RoundStart;
    }

    private void StartFight()
    {
        RoundTimer = TickTimer.CreateFromSeconds(Runner, m_RoundSeconds);
        Phase = RoundPhase.Fight;
    }

    private void CheckRoundEnd()
    {
        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);


        //KO判定
        bool isP1KO = false;
        bool isP2KO = false;
        foreach(Player p in players)
        {
            if (!p.IsKO) continue;
           
            if(p.PlayerIndex == 0)
            {
                isP1KO = true;
            }
            else
            {
                isP2KO = true;
            }
        }

        if(isP1KO  || isP2KO)
        {
            int winner = (isP1KO && isP2KO) ? -1 : (isP1KO ? 1 : 0);
            EndRound(winner, true);
            return;
        }

        //時間切れ:HPが多いほうの勝ち
        if(RoundTimer.Expired(Runner))
        {
            EndRound(JudgeByHp(players), false);
        }
    }


    private void EndRound(int winnerIndex, bool isKO)
    {

        FrozenTime = RoundTimer.RemainingTime(Runner) ?? 0.0f;
        LastWinnerIndex = winnerIndex;

        if(isKO)
        {
            LastResult = winnerIndex < 0 ? RoundResult.DoubleKO : RoundResult.KO;
        }
        else
        {
            LastResult = winnerIndex < 0 ? RoundResult.TimeUpDraw : RoundResult.TimeUp;
        }

        if(winnerIndex == 0)
        {
            P1Wins++;
        }
        else if(winnerIndex == 1)
        {
            P2Wins++;
        }

        PhaseTimer = TickTimer.CreateFromSeconds(Runner, m_RoundEndSeconds);
        Phase = RoundPhase.RoundEnd;

        //KOの時だけスローモーション開始
        if(isKO)
        {
            SlowMotionTimer = TickTimer.CreateFromSeconds(Runner, m_SlowSeconds);
        }
    }

    private void OnORoundEndFinished()
    {
        if(P1Wins >= m_WinsRequired || P2Wins >= m_WinsRequired)
        {
            Phase = RoundPhase.MatchEnd;
            return;
        }

        CurrentRound++;
        BeginRoundStart();
    }


    private int GetPlayerIndex(Player p)
    {
        return p.Object.InputAuthority.RawEncoded <= GetOtherRaw(p) ? 0 : 1;

    }

    private int GetOtherRaw(Player self)
    {
        foreach(Player p in FindObjectsByType<Player>(FindObjectsSortMode.None))
        {
            if(p != self)
            {
                return p.Object.InputAuthority.RawEncoded;
            }
        }
        return int.MaxValue;
    }

    private int JudgeByHp(Player[] players)
    {
        if (players.Length < 2) return -1;
        Player a = players[0], b = players[1];
        int aIdx = GetPlayerIndex(a);
        if (a.HP == b.HP) return -1;
        return a.HP > b.HP ? aIdx : 1 - aIdx;
    }

    private void ResetPlayers()
    {
        foreach(Player p in FindObjectsByType<Player>(FindObjectsSortMode.None))
        {
            p.ResetForNextRound();
        }

        SlowMotionTimer = default;
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