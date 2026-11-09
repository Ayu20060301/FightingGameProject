using UnityEngine;
using TMPro;

public class RoundUI : MonoBehaviour
{

    [SerializeField]
    private RoundManager m_RoundManager;
    [SerializeField]
    private TMP_Text m_CenterText;


    private RoundManager.RoundPhase m_PrevPhase = (RoundManager.RoundPhase)(-1);
    private float m_FightSlownAt;


    private void Update()
    {
        if (m_RoundManager == null || !m_RoundManager.Object || !m_RoundManager.Object.IsValid) return;

        var phase = m_RoundManager.Phase;

        //フェーズが変わった瞬間に文字を更新
        if(phase != m_PrevPhase)
        {
            m_PrevPhase = phase;
            if(phase == RoundManager.RoundPhase.Fight)
            {
                m_FightSlownAt = Time.unscaledDeltaTime;
            }
        }

        switch(phase)
        {
            //case RoundManager.RoundPhase.Waiting:
        }

    }


}
