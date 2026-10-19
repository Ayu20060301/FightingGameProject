using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GuardGaugeBar : MonoBehaviour
{

    [Header("対象プレイヤー(0 = 1P / 1 = 2P)")]
    [SerializeField]
    private int m_PlayerIndex = 0;

    [Header("現在のガードゲージのバー")]
    [SerializeField]
    private Image m_GuardGaugeImage;

    [SerializeField]
    private Image m_GradientImage;

    [Header("色のグラデーション(左端=0% / 右端 = 100%)")]
    [SerializeField]
    private Gradient m_NormalGradient;

    [Header("ガードクラッシュ中のグラデーション")]
    [SerializeField]
    private Gradient m_BrokenGradient;

    [Header("演出")]
    [SerializeField]
    private float m_FillSpeed = 4.0f; //バーが追従する速さ(割合/秒)

    private const float FindInterval = 0.5f; //プレイヤーの検索間隔 

    private Player m_Player;
    private float m_FindTimer;

    private float m_DisplayRate = 1.0f; //表示中の割合(0～1)


    //通常用・クラッシュ用のスプライト
    private Texture2D m_NormalTexture;
    private Texture2D m_BrokenTexture;
    private Sprite m_NormalSprite;
    private Sprite m_BrokenSprite;

    //現在表示しているのがクラッシュ用か
    private bool m_IsBrokenShown;

    private void Start()
    {
        if (m_GuardGaugeImage != null && m_GradientImage != null)
        {
            bool isOriginRight = m_GuardGaugeImage.fillOrigin == (int)Image.OriginHorizontal.Right;

            if (m_NormalGradient != null)
            {
                m_NormalSprite = GradientSpriteUtil.Create(m_NormalGradient, isOriginRight, out m_NormalTexture);
            }

            if (m_BrokenGradient != null)
            {
                m_BrokenSprite = GradientSpriteUtil.Create(m_BrokenGradient, isOriginRight, out m_BrokenTexture);
            }

            m_GradientImage.color = Color.white;
            ShowGradient(false);
        }

        Apply(1.0f);
    }

    private void OnDestroy()
    {
        if (m_NormalSprite != null) Destroy(m_NormalSprite);
        if (m_BrokenSprite != null) Destroy(m_BrokenSprite);
        if (m_NormalTexture != null) Destroy(m_NormalTexture);
        if (m_BrokenTexture != null) Destroy(m_BrokenTexture);
    }

    private void Update()
    {
        if(!IsPlayerReady())
        {
            m_FindTimer -= Time.deltaTime;

            if(m_FindTimer <= 0.0f)
            {
                m_FindTimer = FindInterval;
                FindPlayer();
            }

            return;
        }

        UpdateBar();
    }

    /// <summary>
    /// プレイヤーが有効か
    /// </summary>
    private bool IsPlayerReady()
    {
        return m_Player != null && m_Player.Object != null && m_Player.Object.IsValid;
    }

    /// <summary>
    /// 対象のプレイヤーを探す
    /// </summary>
    private void FindPlayer()
    {
        List<Player> players = new List<Player>(FindObjectsByType<Player>(FindObjectsSortMode.None));

        //PlayerIDの小さい順に並べて、m_PlayerIndex番目に選ぶ
        players.Sort((a, b) => a.Object.InputAuthority.PlayerId.CompareTo(b.Object.InputAuthority.PlayerId));

        if (m_PlayerIndex >= players.Count) return;

        m_Player = players[m_PlayerIndex];
        m_DisplayRate = GetTargetRate();
        ShowGradient(m_Player.IsGuardBroken);
        Apply(m_DisplayRate);
    }

    /// <summary>
    /// 現在のガードゲージの割合(0～1)
    /// </summary>
    private float GetTargetRate()
    {
        return Mathf.Clamp01(m_Player.GuardGauge / m_Player.MaxGuardGauge);
    }


    /// <summary>
    /// ゲージの割合へ向けてバーを動かす
    /// </summary>
    private void UpdateBar()
    {

        //クラッシュ状態が変わった時だけグラデーションを差し替える
        bool isBroken = m_Player.IsGuardBroken;

        if(isBroken != m_IsBrokenShown)
        {
            ShowGradient(isBroken);
        }

        m_DisplayRate = Mathf.MoveTowards(m_DisplayRate, GetTargetRate(), m_FillSpeed * Time.deltaTime);

        Apply(m_DisplayRate);
    }

    private void ShowGradient(bool isBroken)
    {
        m_IsBrokenShown = isBroken;

        if (m_GradientImage == null) return;

        Sprite sprite = isBroken ? m_BrokenSprite : m_NormalSprite;

        //クラッシュ用が未設定なら通常用のまま
        if(sprite == null)
        {
            sprite = m_NormalSprite;
        }
        if(sprite != null)
        {
            m_GradientImage.sprite = sprite;
        }

    }

    private void Apply(float rate)
    {
        if (m_GuardGaugeImage == null) return;

        m_GuardGaugeImage.fillAmount = rate;
    }

}
