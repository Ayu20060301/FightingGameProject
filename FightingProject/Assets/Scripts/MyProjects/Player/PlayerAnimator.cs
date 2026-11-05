using UnityEngine;
using System.Collections.Generic;

public enum AnimationState
{
    Idle,
    Walk,       //歩行
    BackWalk,   //バック移動
    Guard,      //ガード
    Crouch,     //しゃがみ
    Jump,       //ジャンプ

    //攻撃

    NormalAttack,  //通常攻撃
  
    LowAttack,      //下攻撃
    
    ForwardAttack,  //前攻撃

    AirAttack,     //空中攻撃
    
    CrouchAttack, //しゃがみ攻撃

    //ダメージを受けた際の各アニメーション
    Hit,            //立ち喰らい
    GuardHit,       //立ちガード喰らい


    //ステップ
    Step,
    BackStep,
}

public class PlayerAnimator
{
    //アニメーションのクロスフェード時間
    private const float CrossFadeDuration = 0.05f;

    //Animatorパラメーター
    private static readonly int SpeedParam = Animator.StringToHash("Speed");
    private static readonly int IsGuardParam = Animator.StringToHash("IsGuard");

    private static readonly int IsCrouchParam = Animator.StringToHash("IsCrouch");

    private static Dictionary<AnimationState, string> AnimationNames = new Dictionary<AnimationState, string>
    {
        { AnimationState.Idle,          "Idle"},
        { AnimationState.Walk,          "Walk"},
        { AnimationState.BackWalk,      "BackWalk"},
        { AnimationState.Guard,         "Guard"},
        { AnimationState.Crouch,        "Crouch"},
        { AnimationState.Jump,          "Jump"},
        { AnimationState.NormalAttack,  "NormalAttack"},
        { AnimationState.LowAttack,     "LowAttack"},
        { AnimationState.ForwardAttack, "ForwardAttack"},
        { AnimationState.AirAttack,     "AirAttack"},
        { AnimationState.CrouchAttack,  "CrouchAttack"},
        { AnimationState.Hit,           "Hit"},
        { AnimationState.GuardHit,      "GuardHit"},
        { AnimationState.Step,          "Step"},
        { AnimationState.BackStep,      "BackStep"},
    };

    private readonly Animator m_Anim;


    public PlayerAnimator(Animator anim)
    {
        m_Anim = anim;
    }

    /// <summary>
    /// アニメーションを再生する
    /// </summary>
    public void Play(AnimationState state)
    {
        if (m_Anim == null) return;

        m_Anim.speed = 1.0f;

        ResetPatameters();
        ApplyParameters(state);


        if(AnimationNames.TryGetValue(state,out string stateName))
        {
            m_Anim.CrossFade(stateName, CrossFadeDuration);
        }
    }

    /// <summary>
    /// 再生速度を変更する
    /// </summary>
    /// <param name="speed"></param>
    public void SetSpeed(float speed)
    {
        if(m_Anim != null)
        {
            m_Anim.speed = speed;
        }
    }

    public bool IsFinished(AnimationState state)
    {
        if (m_Anim == null) return false;

        string stateName = AnimationNames[state];

        //Animatorが遷移中なら終了判定しない
        if (m_Anim.IsInTransition(0))
        {
            LogAnimation($"{stateName} : 遷移中");
            return false;
        }

        AnimatorStateInfo stateInfo = m_Anim.GetCurrentAnimatorStateInfo(0);

        if(!stateInfo.IsName(stateName))
        {
            LogAnimationWarning($"ステート名不一致 : 期待= {stateName},現在のhash = {stateInfo.fullPathHash}");
            return false;
        }


        if(stateInfo.loop)
        {
            LogAnimationWarning($"{stateName} + Loop設定がON");
            return false;
        }


        bool isFinished = stateInfo.normalizedTime >= 1.0f;


        if(isFinished)
        {
            LogAnimation($"{stateName} : 再生終了 normalizedTime = {stateInfo.normalizedTime:F2}");

        }

        return isFinished;
    }

    public float GetNormalizedTime(AnimationState state)
    {
        if (m_Anim == null) return 0.0f;
        if(m_Anim.IsInTransition(0)) return 0.0f;
        if (!AnimationNames.TryGetValue(state, out string stateName)) return 0.0f;


        AnimatorStateInfo stateInfo = m_Anim.GetCurrentAnimatorStateInfo(0);
        if(!stateInfo.IsName(stateName)) return 0.0f;

        return stateInfo.normalizedTime;
    }


    private void ResetPatameters()
    {
        m_Anim.SetFloat(SpeedParam, 1.0f);
        m_Anim.SetBool(IsGuardParam, false);
        m_Anim.SetBool(IsCrouchParam, false);
    }


    private void ApplyParameters(AnimationState state)
    {
        switch(state)
        {
            case AnimationState.Walk:
            case AnimationState.BackWalk:
                m_Anim.SetFloat(SpeedParam, 2.0f);
                break;
            case AnimationState.Guard:
                m_Anim.SetBool(IsGuardParam,true);
                break;
            case AnimationState.Crouch:
                m_Anim.SetBool(IsCrouchParam, true);
                break;
        }
    }

    //エディタ上のみログ出力
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private static void LogAnimation(string message) => Debug.Log($"[Animation] {message}");

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private static void LogAnimationWarning(string message) => Debug.LogWarning($"[Animation] {message}");

}
