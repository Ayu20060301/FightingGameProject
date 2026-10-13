using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 2人のプレイヤーを追従するカメラ
/// </summary>
public class TwoPlayerCamera : MonoBehaviour
{
    [Header("追従設定")]
    [SerializeField]
    private float m_FollowSpeed = 5.0f;
    [SerializeField] 
    private Vector3 m_Offset = new Vector3(0f, 2f, -4f);


    [Header("ズーム設定")]
    
    [SerializeField] 
    private float m_MinDistance = 4f;
    [SerializeField] 
    private float m_MaxDistance = 10f;
    [SerializeField] 
    private float m_MaxPlayerDistance = 8f;
    [SerializeField] 
    private float m_ZoomSpeed = 3f;

    private Transform m_Player1;
    private Transform m_Player2;
    private Camera m_Camera;

    private void Awake()
    {
        m_Camera = GetComponent<Camera>();
    }


 
    private void LateUpdate()
    {
        if (m_Player1 == null || m_Player2 == null)
            return;

        // 2人の中間地点
        Vector3 midpoint = (m_Player1.position + m_Player2.position) * 0.5f;

        // カメラの追従先
        Vector3 targetPosition = midpoint + m_Offset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            1f - Mathf.Exp(-m_FollowSpeed * Time.deltaTime)
        );

        // プレイヤー間の距離からズーム量を計算
        float distance = Vector3.Distance(
            m_Player1.position,
            m_Player2.position
        );

        float t = Mathf.Clamp01(distance / m_MaxPlayerDistance);

        float targetSize = Mathf.Lerp(
            m_MinDistance,
            m_MaxDistance,
            t
        );

        // PerspectiveカメラではFOVでズームを調整
        if (m_Camera != null)
        {
            m_Camera.fieldOfView = Mathf.Lerp(
                m_Camera.fieldOfView,
                targetSize * 6f,
                1f - Mathf.Exp(-m_ZoomSpeed * Time.deltaTime)
            );
        }
    }

   
    /// <summary>
    /// 追従する2人を登録する
    /// </summary>
    /// <param name="player1">プレイヤー1</param>
    /// <param name="player2">プレイヤー2</param>
    public void SetPlayers(Transform player1,Transform player2)
    {
        m_Player1 = player1;
        m_Player2 = player2;
    }
}
