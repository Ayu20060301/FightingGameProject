using UnityEngine;

//喰らい判定
public class HurtBox : MonoBehaviour
{
    [SerializeField]
    private Player m_Owner;

    /// <summary>
    /// この喰らい主の持ち主
    /// </summary>
    public Player Owner => m_Owner;

    private void Awake()
    {
        //未設定なら親から探す
        if(m_Owner == null)
        {
            m_Owner = GetComponentInParent<Player>();
        }
    }

    private void Reset()
    {
        //コンポーネント追加時に自動で親のPlayerを設定
        m_Owner = GetComponentInParent<Player>();

        var box = GetComponent<BoxCollider>();
        if(box != null)
        {
            box.isTrigger = true;
        }
    }

}
