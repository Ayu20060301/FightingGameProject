using UnityEngine;

//Animatorのルートモーション移動量をPlayerへ中継する
[RequireComponent(typeof(Animator))]
public class RootMotionRelay : MonoBehaviour
{
    private Animator m_Anim;
    private Player m_Player;

    private void Awake()
    {
        m_Anim = GetComponent<Animator>();
        m_Player = GetComponentInParent<Player>();
    }

    private void OnAnimatorMove()
    {
        if (m_Player == null) return;

        m_Player.AddRootMotion(m_Anim.deltaPosition.x);
    }

}
