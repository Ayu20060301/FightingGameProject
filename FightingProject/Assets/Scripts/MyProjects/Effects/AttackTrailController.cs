using UnityEngine;

public class AttackTrailController : MonoBehaviour
{
    [SerializeField]
    private TrailRenderer m_Trail;

    private void Awake()
    {
        if(m_Trail == null)
        {
            m_Trail = GetComponent<TrailRenderer>();
        }


        if(m_Trail != null)
        {
            m_Trail.emitting = false;
            m_Trail.Clear();
        }
    }

    /// <summary>
    /// 攻撃開始時
    /// </summary>
    public void StartTrail()
    {
        if (m_Trail == null) return;

        //前回の軌跡を残さない
        m_Trail.Clear();
        m_Trail.emitting = true;
    }

}
