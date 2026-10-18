using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text m_Text;
    [SerializeField]
    private Color m_NormalColor = Color.white;
    [SerializeField]
    private Color m_LowColor = Color.red;
    [SerializeField]
    private float m_LowTimeThreshold = 10.0f;

    private RoundManager m_RoundManager;

    // Update is called once per frame
    void Update()
    {
        if(m_RoundManager == null || !m_RoundManager.Object.IsValid)
        {
            m_RoundManager = FindFirstObjectByType<RoundManager>();
            return;
        }

        float time = m_RoundManager.RemainingTime;

        m_Text.text = Mathf.CeilToInt(time).ToString("00");
        m_Text.color = time <= m_LowTimeThreshold ? m_LowColor : m_NormalColor;
    }
}
