using Fusion;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//HPバー
//サーバーから同期されたPlayer.Hpを読み取って表示する
public class HPBar : MonoBehaviour
{
    [Header("対象プレイヤー(0 = 1P / 1 = 2P)")]
    [SerializeField]
    private int m_PlayerIndex = 0;

    [Header("現在HPのバー")]
    [SerializeField]
    private Image m_HPImage;

    [Header("グラデーション表示用の子イメージ")]
    [SerializeField]
    private Image m_GradientImage;

    [Header("ダメージ量を遅れて示すバー")]
    [SerializeField]
    private Image m_HPDamageImage;

    [Header("演出")]
    [SerializeField]
    private float m_FillSpeed = 4.0f;   //現在のHPが追従する速さ

    [SerializeField]
    private float m_DamageDelay = 0.4f; //ダメージバーが減り始めるまでの待ち時間

    [SerializeField]
    private float m_DamageFillSpeed = 1.0f; //ダメージバーが減る速さ

    private const float FindInterval = 0.5f; //プレイヤーの検索間隔(秒)

    [SerializeField]
    private Gradient m_ColorGradient;

    private Player m_Player;
    private float m_FindTimer;

    //表示中の割合(0～1)
    private float m_DisplayRate = 1.0f;
    private float m_DamageRate = 1.0f;
    private float m_DamageTimer;

    //前回のHP
    private int m_LastHP;

    private Texture2D m_GradientTexture;
    private Sprite m_GradientSprite;

    private void Start()
    {
        if(m_HPImage != null && m_GradientImage && m_ColorGradient != null)
        {
            bool isOriginRight = m_HPImage.fillOrigin == (int)Image.OriginHorizontal.Right;

            m_GradientSprite = GradientSpriteUtil.Create(m_ColorGradient, isOriginRight, out m_GradientTexture);

            m_GradientImage.sprite = m_GradientSprite;
            m_GradientImage.color = Color.white;
        }

        Apply(1.0f, 1.0f);
    }

    private void OnDestroy()
    {
        if(m_GradientSprite != null)
        {
            Destroy(m_GradientSprite);
        }

        if(m_GradientTexture != null)
        {
            Destroy(m_GradientTexture);
        }
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
    /// <returns></returns>
    private bool IsPlayerReady()
    {
        return m_Player != null && m_Player.Object != null && m_Player.Object.IsValid;
    }

    /// <summary>
    /// 対象のプレイヤーを探す
    /// </summary>
    private void FindPlayer()
    {
        List<Player> players = new List<Player>(
         FindObjectsByType<Player>(FindObjectsSortMode.None));
        players.RemoveAll(p => p.Object == null || !p.Object.IsValid);

        // PlayerId同士を比較する(InputAuthorityそのものではなく)
        players.Sort((a, b) => a.Object.InputAuthority.PlayerId.CompareTo(b.Object.InputAuthority.PlayerId));

        if (m_PlayerIndex >= players.Count) return;

        m_Player = players[m_PlayerIndex];
        m_LastHP = m_Player.HP;
        m_DisplayRate = m_DamageRate = GetHPRate(m_Player.HP);
        Apply(m_DisplayRate, m_DamageRate);
    }

    /// <summary>
    /// HPの割合へ向けてバーを動かす
    /// </summary>
    private void UpdateBar()
    {
        int hp = m_Player.HP;
        float targetRate = GetHPRate(hp);

        //ダメージを受けた瞬間にダメージバーの待ち時間を開始
        if(hp < m_LastHP)
        {
            m_DamageTimer = m_DamageDelay;
        }

        m_LastHP = hp;

        //現在HPバーは素早く追従
        m_DisplayRate = Mathf.MoveTowards(m_DisplayRate, targetRate, m_FillSpeed * Time.deltaTime);

        //ダメージバーは少し待ってからゆっくり追従
        if(m_DamageTimer > 0.0f)
        {
            m_DamageTimer -= Time.deltaTime;
        }
        else
        {
            m_DamageRate = Mathf.MoveTowards(m_DamageRate, targetRate, m_DamageFillSpeed * Time.deltaTime);
        }


        //回復時はダメージバーが現在HPより下にならないようにする
        m_DamageRate = Mathf.Max(m_DamageRate, m_DisplayRate);

        Apply(m_DisplayRate, m_DamageRate);
    }


    private void Apply(float rate,float damageRate)
    {

        if(m_HPImage != null)
        {
            m_HPImage.fillAmount = rate;
        }

        if(m_HPDamageImage != null)
        {
            m_HPDamageImage.fillAmount = damageRate;
        }
    }


    private float GetHPRate(int hp)
    {
        return Mathf.Clamp01((float)hp / Mathf.Max(1, m_Player.MaxHP));
    }
}
