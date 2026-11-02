using Unity.Cinemachine;
using UnityEngine;

public class FightCamera : MonoBehaviour
{

    [SerializeField]
    private CinemachineTargetGroup m_Group;
    [SerializeField]
    private float m_Weight = 1.0f;
    [SerializeField]
    private float m_Radius = 0.8f;

    public void AddTarget(Transform target)
    {
        if (m_Group == null || target == null) return;
        if (m_Group.FindMember(target) >= 0) return;
        m_Group.AddMember(target, m_Weight, m_Radius);
    }


    public void RemoveTarget(Transform target)
    {
        if ((m_Group == null || target == null)) return;
        if (m_Group.FindMember(target) >= 0) return;
        m_Group.RemoveMember(target);
    }
}
