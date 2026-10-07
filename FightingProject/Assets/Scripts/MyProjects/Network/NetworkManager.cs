using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
     //入力データ
     public struct PlayerInputData : INetworkInput
    {
        public float horizontal;
        public float vertical;
        public NetworkBool isJump;

        public NetworkBool isGuard;

        public NetworkBool isLightKick;
        public NetworkBool isHeavyKick;

        public NetworkBool isLightPunch;
        public NetworkBool isHeavyPunch;
    }

    [SerializeField]
    private InputController m_Input;

    //プレイヤーが操作するプレハブ
    [SerializeField]
    private NetworkPrefabRef m_PlayerPrefab;

    //ネットワークシステムの根幹であるランナー
    private NetworkRunner m_Runner;

    async void Start()
    {
        //ランナーの設定
        m_Runner = gameObject.AddComponent<NetworkRunner>();
        m_Runner.ProvideInput = true;
        m_Runner.AddCallbacks(this);

        //サーバーかクライアントか
        GameMode gameMode = GameMode.Client;

        //実行時のコマンドライン引数でサーバーかどうか判定する
        string[] args = System.Environment.GetCommandLineArgs();

        if(args.Contains("-server"))
        {
            gameMode = GameMode.Server;
        }

        //ゲームを設定して通信開始
        await m_Runner.StartGame(new StartGameArgs()
        {
            GameMode = gameMode, //サーバーかクライアントか
            SessionName = "FightingProject", //セッション名
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>() //ネットワーク用のシーン
        });
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
      
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
      
    }

    /// <summary>
    /// プレイヤーが参加してきたら呼ばれる
    /// </summary>
    /// <param name="runner">ランナー</param>
    /// <param name="player">参加してきたプレイヤー参照</param>
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        //サーバーがプレイヤーをスポーンさせる
        if(!runner.IsServer)
        {
            return;
        }

        //固定スポーン位置
        Vector3 spawnPos = new Vector3(-2.0f, 0.5f, 0.0f);

        Quaternion spawnRot = Quaternion.Euler(0.0f, 90.0f, 0.0f);


        NetworkObject obj = runner.Spawn(m_PlayerPrefab,spawnPos,spawnRot,player);

        runner.SetPlayerObject(player, obj);


    }

    /// <summary>
    /// プレイヤーが通信からいなくなったら呼ばれる
    /// </summary>
    /// <param name="runner">ランナー</param>
    /// <param name="player">いなくなったプレイヤー参照</param>
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        //オブジェクトを取得して退場
        if(runner.TryGetPlayerObject(player,out NetworkObject obj))
        {
            runner.Despawn(obj);
        }
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
       
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
      
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
       
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
  
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
       
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        
    }

    /// <summary>
    /// プレイヤーから入力があるたびに呼ばれる
    /// </summary>
    /// <param name="runner">ランナー</param>
    /// <param name="input">入力システム</param>
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        InputState state = m_Input.State;

        //入力データに入力状況を記録
        PlayerInputData data = new PlayerInputData();
       
        //移動
        switch(state.Direction)
        {
            case Direction.Left:
                data.horizontal = -1.0f;
                data.vertical = 0.0f;
                break;
            case Direction.Right:
                data.horizontal = 1.0f;
                data.vertical = 0.0f;
                break;
            case Direction.Up:
                data.horizontal = 0.0f;
                data.vertical = 1.0f;
                break;
            case Direction.Down:
                data.horizontal = 0.0f;
                data.vertical = -1.0f;
                break;
            case Direction.UpLeft:
                data.horizontal = -1.0f;
                data.vertical = 1.0f;
                break;

            case Direction.UpRight:
                data.horizontal = 1.0f;
                data.vertical = 1.0f;
                break;

            case Direction.DownLeft:
                data.horizontal = -1.0f;
                data.vertical = -1.0f;
                break;

            case Direction.DownRight:
                data.horizontal = 1.0f;
                data.vertical = -1.0f;
                break;

            default:
                data.horizontal = 0.0f;
                data.vertical = 0.0f;
                break;
        }

        //ボタン入力
        data.isJump = state.IsJump;
        data.isGuard = state.IsGuard;

        data.isLightKick = state.IsLightKick;
        data.isHeavyKick = state.IsHeavyKick;

        data.isLightPunch = state.IsLightPunch;
        data.isHeavyPunch = state.IsHeavyPunch;

        //Fusionへ入力を渡す
        input.Set(data);

        state.ConsumeAttackInput();
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
      
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
       
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    { 
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {

    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
      
    }
}
