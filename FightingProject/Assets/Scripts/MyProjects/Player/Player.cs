using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [Header("移動")]
    [SerializeField]
    private float m_Speed = 5.0f;

    [Header("ジャンプ")]
    [SerializeField]
    private float m_JumpPower = 5.0f;

    [Header("重力")]
    [SerializeField]
    private float m_Gravity = -9.8f;

    //移動量
    private Vector3 m_Move = Vector3.zero;

    //CharacterController
    private CharacterController m_Controller = null;
    

    private void Awake()
    {
        //オンラインの移動はCharacterControllerが一番無難
        m_Controller = GetComponent<CharacterController>();
    }

    public override void FixedUpdateNetwork()
    {
        //入力データを取得
        if(!GetInput(out NetworkManager.PlayerInputData data))
        {
            return;
        }

        //地面に接地している場合
        if(m_Controller.isGrounded)
        {
            //地面に軽く押し付ける
            if(m_Move.y < 0.0f)
            {
                m_Move.y = -1.0f;
            }

            //ジャンプ
            if(data.isJump)
            {
                m_Move.y = m_JumpPower;
            }
        }

        //左右移動
        m_Move.x = data.horizontal * m_Speed;

        //重力
        m_Move.y += m_Gravity * Runner.DeltaTime;

        //移動
        m_Controller.Move(m_Move * Runner.DeltaTime);
    }
}
