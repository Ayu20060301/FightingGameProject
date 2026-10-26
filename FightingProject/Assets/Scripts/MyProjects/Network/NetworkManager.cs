using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
     //入力データ
     public struct PlayerInputData : INetworkInput
    {
        public float horizontal;
        public float vertical;
        public NetworkBool isJump;

        public NetworkBool isGuard;

        public NetworkBool isNormalAttack;
        public NetworkBool isLowAttack;
        public NetworkBool isUpAttack;
        public NetworkBool isForwardAttack;

    }

    [Header("入力")]
    [SerializeField]
    private InputController m_Input;

    [Header("プレイヤーが操作するプレハブ")]
    [SerializeField]
    private NetworkPrefabRef m_PlayerPrefab;

    [Header("ネットワーク設定")]
    [SerializeField, Min(1)]
    private int m_MaxPlayers = 2;

    //ネットワークシステムの根幹であるランナー
    private NetworkRunner m_Runner;


    public NetworkRunner Runner => m_Runner;

    //スポーン位置と向き
    private static readonly Vector3[] SpawnPositions =
    {
        new Vector3(-2.0f, 0.0f,0.0f),
        new Vector3(2.0f, 0.0f,0.0f),
    };


    private static readonly float[] SpawnYaws = { 90.0f, -90.0f };

    //参加中のプレイヤーが使っているスロット
    private readonly Dictionary<PlayerRef, int> m_Slots = new Dictionary<PlayerRef, int>();


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

            //最大Player人数
            PlayerCount = m_MaxPlayers,
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
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


        //空いている最小のスロットを使う
        int slot = 0;

        while(m_Slots.ContainsValue(slot))
        {
            slot++;
        }

        //定員オーバーの場合は何もしない
        if (slot >= SpawnPositions.Length) return;


        m_Slots[player] = slot;

        Vector3 spawnPos = SpawnPositions[slot];
        Quaternion spawnRot = Quaternion.Euler(0.0f, SpawnYaws[slot], 0.0f);

        //プレイヤーをスポーン
        NetworkObject obj = runner.Spawn(m_PlayerPrefab, spawnPos, spawnRot,player);

        //プレイヤーオブジェクトを登録
        runner.SetPlayerObject(player, obj);


        //スポーンしたプレイヤーを取得
        Player newPlayer = obj.GetComponent<Player>();

        //参加中のプレイヤーから対戦相手を探す
        foreach(PlayerRef otherRef in runner.ActivePlayers)
        {
            //自分自身は除外
            if (otherRef == player) continue;

            NetworkObject otherObj = runner.GetPlayerObject(otherRef);

            if (otherObj == null) continue;

            Player otherPlayer = otherObj.GetComponent<Player>();

            if (otherPlayer == null) continue;

            //お互いに対戦相手を登録
            newPlayer.SetOpponent(otherPlayer);
            otherPlayer.SetOpponent(newPlayer);
        }
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

        data.isNormalAttack = state.IsNormalAttack;
        data.isLowAttack = state.IsLowAttack;

        data.isUpAttack = state.IsUpAttack;
        data.isForwardAttack = state.IsForwardAttack;

        //Fusionへ入力を渡す
        input.Set(data);

        //ボタン入力を消費
        m_Input.ConsumeButtonInput();
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
