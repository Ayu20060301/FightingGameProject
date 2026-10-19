using Unity.Cinemachine;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GuardBreakCamera : MonoBehaviour
{
   public static GuardBreakCamera Instance { get; private set; }


    [SerializeField]
    private CinemachineCamera m_CloseUpCamera;

    [Header("カメラの向き")]
    [SerializeField, Range(0.0f, 90.0f)]
    private float m_FrontAngle = 30.0f;  //プレイヤーの正面側へ回り込む角度
    [SerializeField]
    private float m_Pitch = 5.0f; //見下ろし角

    [SerializeField]
    private float m_Duration = 0.8f;  //演出の長さ
    [SerializeField, Range(0.05f, 1.0f)]
    private float m_SlowSpeed = 0.25f;         //アニメーションの速度

    private Coroutine m_Routine;
    private readonly List<Player> m_SlowedPlayers = new();

    private void Awake()
    {
        Instance = this;

        if(m_CloseUpCamera != null)
        {
            m_CloseUpCamera.gameObject.SetActive(false);
        }
    }


    private void OnDestroy()
    {
        if(Instance == this)
        {
            Instance = null;
        }
    }

    public void Play(Player target)
    {
        if (m_CloseUpCamera == null || target == null) return;

        //演出中に再度呼ばれたら、前の演出を片づけてからやり直す
        if(m_Routine != null)
        {
            StopCoroutine(m_Routine);
            Restore();
        }

        m_Routine = StartCoroutine(PlayRoutine(target));
    }

    
    private IEnumerator PlayRoutine(Player target)
    {

        //プレイヤーの向き(右向きなら +1、左向きなら -1)
        float facing = target.transform.forward.x >= 0.0f ? 1.0f : -1.0f;

        //アクティブにする前に回転を決める
        m_CloseUpCamera.transform.rotation = Quaternion.Euler(m_Pitch, -facing * m_FrontAngle, 0.0f);

        //ズーム
        m_CloseUpCamera.Target.TrackingTarget = target.transform;
        m_CloseUpCamera.gameObject.SetActive(true);

        //スロー(両プレイヤーのアニメーション)
        foreach(Player p in FindObjectsByType<Player>(FindObjectsSortMode.None))
        {
            p.SetAnimationSpeed(m_SlowSpeed);
            m_SlowedPlayers.Add(p);
        }


        yield return new WaitForSeconds(m_Duration);

        Restore();
        m_Routine = null;
    }
   
    private void Restore()
    {
        foreach(Player p in m_SlowedPlayers)
        {
            if(p != null)
            {
                p.SetAnimationSpeed(1.0f);
            }
        }

        m_SlowedPlayers.Clear();

        if(m_CloseUpCamera != null)
        {
            m_CloseUpCamera.gameObject.SetActive(false);
        }
    }

}
