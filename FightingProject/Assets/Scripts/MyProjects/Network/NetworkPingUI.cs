using Fusion;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.UIElements;

public class NetworkPingUI : MonoBehaviour
{
    [Serializable]
    public class PingLevel
    {
        public string label = "Good";
        [Tooltip("この未満ならこの段階。最後の段階は上限なしで扱う")]
        public float maxPingMs = 60.0f;
        [Tooltip("点灯させるバーの本数")]
        public int lifeBars = 3;
        public Color color = Color.green;
    }

    [Header("参照")]
    [SerializeField]
    private NetworkManager m_NetworkManager;
    [SerializeField]
    private TMP_Text m_PingText;
    [SerializeField]
    private Image[] m_Bars;  //左から順に3本を設定

    [Header("更新")]
    [SerializeField]
    private float m_UpdateInterval = 0.5f;
    [SerializeField]
    private bool m_IsShowText = true;

    [Header("段階設定(Pingの小さい順に並べる)")]
    [SerializeField]
    private PingLevel[] m_Levels =
    {
        new PingLevel {label = "Good",   maxPingMs = 60.0f,lifeBars = 3,color = Color.green},
        new PingLevel {label = "Normal", maxPingMs = 120.0f,lifeBars = 3,color = Color.yellow},
        new PingLevel {label = "Bad",    maxPingMs = 0.0f,lifeBars = 3,color = Color.red},
    };

    [Header("色・特殊状態")]
    [SerializeField]
    private Color m_OffColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
    [SerializeField]
    private string m_HostLavel = "Host";
    [SerializeField]
    private int m_HostLitBars = 3;
    [SerializeField]
    private Color m_HostColor = Color.green;


    private float m_Timer;

    private void Start()
    {
        if(m_PingText != null)
        {
            m_PingText.gameObject.SetActive(m_IsShowText);
        }
    }

    // Update is called once per frame
    void Update()
    {
        m_Timer += Time.unscaledDeltaTime;
        if (m_Timer < m_UpdateInterval) return;
        m_Timer = 0.0f;

        //未接続
       if(m_NetworkManager == null || m_NetworkManager.Runner == null || !m_NetworkManager.Runner.IsRunning)
       {
            SetText("-- ms");
            SetBars(0, m_OffColor);
       }

        NetworkRunner runner = m_NetworkManager.Runner;

        //ホスト(サーバー)自身は通信相手がいないのでRTTは意味を持たない
        if(runner.IsServer)
        {

            SetText("Server");
            SetBars(m_HostLitBars,m_HostColor);
            return;
        }

        //クライアント
        double pingMs = runner.GetPlayerRtt(runner.LocalPlayer) * 1000.0;
        SetText($"{pingMs:F0} ms");


        PingLevel level = FindLevel(pingMs);
        if(level != null)
        {
            SetBars(level.lifeBars, level.color);
        }
        else
        {
            SetBars(0,m_OffColor);
        }
    }

    private PingLevel FindLevel(double pingMs)
    {
        if (m_Levels == null || m_Levels.Length == 0) return null;

        for(int i = 0; i < m_Levels.Length - 1; i++)
        {
            if(pingMs < m_Levels[i].maxPingMs)
            {
                return m_Levels[i];
            }
        }
        return m_Levels[m_Levels.Length - 1]; //最後は上限なし
    }

    private void SetText(string text)
    {
        if(m_PingText != null)
        {
            m_PingText.text = text;
        }
    }


    private void SetBars(int litCount,Color litColor)
    {
        if (m_Bars == null) return;

        for(int i = 0; i < m_Bars.Length; i++)
        {
            if (m_Bars[i] == null) continue;
            m_Bars[i].color = (i < litCount) ? litColor : m_OffColor;
        }
    }

}
