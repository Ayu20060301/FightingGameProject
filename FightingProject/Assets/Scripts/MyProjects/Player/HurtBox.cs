using UnityEngine;

public class HurtBox : MonoBehaviour
{

    [SerializeField]
    private Player m_Owner;
    public Player Owner => m_Owner;
}
