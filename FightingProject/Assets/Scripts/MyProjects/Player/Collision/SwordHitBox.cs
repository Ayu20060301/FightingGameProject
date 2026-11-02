using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SwordHitBox : MonoBehaviour
{
    private BoxCollider m_BoxCollider;

    private void Awake()
    {
        m_BoxCollider = GetComponent<BoxCollider>();

        // 物理的な接触イベントには使用しない
        // 判定形状の取得専用
        m_BoxCollider.isTrigger = true;
        m_BoxCollider.enabled = false;
    }

    /// <summary>
    /// BoxColliderのワールド座標を取得
    /// </summary>
    public void GetWorldBox(out Vector3 center,out Vector3 halfExtents,out Quaternion rotation)
    {
        Transform t = m_BoxCollider.transform;

        center = t.TransformPoint(m_BoxCollider.center);

        Vector3 scale = t.lossyScale;

        halfExtents = Vector3.Scale(
            m_BoxCollider.size * 0.5f,
            new Vector3(
                Mathf.Abs(scale.x),
                Mathf.Abs(scale.y),
                Mathf.Abs(scale.z)
            )
        );

        rotation = t.rotation;
    }
}
