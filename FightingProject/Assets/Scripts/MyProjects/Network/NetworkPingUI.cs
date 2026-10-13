using Fusion;
using TMPro;
using UnityEngine;

public class NetworkPingUI : MonoBehaviour
{

    [SerializeField]
    private NetworkManager m_NetworkManager;

    [SerializeField]
    private TMP_Text m_PingText;


    // Update is called once per frame
    void Update()
    {
       if(m_NetworkManager == null || m_NetworkManager.Runner == null || !m_NetworkManager.Runner.IsRunning)
       {
            m_PingText.text = "Ping: -- ms";
            return;
       }

        NetworkRunner runner = m_NetworkManager.Runner;


        //自分のRTTを取得
        double rtt = runner.GetPlayerRtt(runner.LocalPlayer);


        //秒からミリ秒に変換
       double pingMs = rtt * 1000.0f;

        m_PingText.text = $"Ping: {pingMs:F0} ms";
    }
}
