using Cysharp.Threading.Tasks;
using Google.Protobuf;
using PitayaClient.Network.Client;
using PitayaGame.MatchmakingSvr;
using QFramework;
using System;
using UnityEditor.PackageManager;
using UnityEngine;

namespace PitayaClient.Network.Manager
{
    /// <summary>
    /// 网络管理器 - Unity 单例封装
    /// 职责：提供简洁的 API，管理 Unity 生命周期，单例模式
    /// 面向应用程序开发者，隐藏底层复杂性
    /// </summary>
    public class NetworkManager : MonoSingleton<NetworkManager>
    {
        private NetworkClient _client;

        [Header("Network Settings")]
        [SerializeField] private float _defaultTimeout = 10f;

        /// <summary>
        /// 连接状态
        /// </summary>
        public bool IsConnected => _client?.IsConnected ?? false;

        // 简洁的业务事件
        public event Action OnConnected;              // 连接成功（握手完成）
        public event Action<string> OnDisconnected;   // 连接断开
        public event Action<Exception> OnError;       // 网络错误

        /// <summary>
        /// 创建单例实例
        /// </summary>
        private static NetworkManager CreateInstance()
        {
            var go = new GameObject("[NetworkManager]");
            var instance = go.AddComponent<NetworkManager>();
            DontDestroyOnLoad(go);
            Debug.Log("🎮 NetworkManager instance created");
            return instance;
        }

        private void Awake()
        {
            InitializeClient();
        }

        /// <summary>
        /// 初始化网络客户端
        /// </summary>
        private void InitializeClient()
        {
            _client = new NetworkClient();
            
            // 转发客户端事件
            _client.OnConnected += () => OnConnected?.Invoke();
            _client.OnDisconnected += (reason) => OnDisconnected?.Invoke(reason);
            _client.OnError += (ex) => OnError?.Invoke(ex);

            RegisterMatchSuccessHandler((notify) =>
            {
                Debug.Log($"匹配成功! Match ID: {notify.MatchId}");
            });

            Debug.Log("🔧 NetworkClient initialized");
        }

        private void Update()
        {
            // 驱动网络客户端的更新逻辑（消息队列处理、心跳等）
            _client?.Update();
        }
        // private void OnDestroy() {
        //     // 清理资源
        //     _client?.Dispose();
        //     Debug.Log("🗑️ NetworkManager destroyed");
        // }
        #region 公有 API

        /// <summary>
        /// 连接到服务器
        /// </summary>
        /// <param name="endpoint">服务器地址（host:port）</param>
        /// <param name="path">WebSocket 路径</param>
        /// <returns>连接是否成功</returns>
        public async UniTask<bool> ConnectAsync(string endpoint, string path = "/ws")
        {
            if (IsConnected)
            {
                Debug.LogWarning("⚠️ Already connected to server");
                return true;
            }

            Debug.Log($"🌐 Connecting to {endpoint}{path}...");
            return await _client.ConnectAsync(endpoint, path);
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public async UniTask DisconnectAsync()
        {
            if (_client != null && _client.IsConnected)
            {
                Debug.Log("🔌 Disconnecting from server...");
                await _client.DisconnectAsync();
            }
        }

        /// <summary>
        /// 发送 Protobuf 请求
        /// </summary>
        /// <typeparam name="TResponse">响应类型</typeparam>
        /// <param name="route">路由地址</param>
        /// <param name="request">请求消息</param>
        /// <param name="timeout">超时时间（秒）</param>
        /// <returns>响应消息</returns>
        public async UniTask<TResponse> RequestAsync<TResponse>(
            string route,
            Google.Protobuf.IMessage request,
            float timeout = 0f)
            where TResponse : Google.Protobuf.IMessage<TResponse>, new()
        {
            if (!IsConnected)
                throw new InvalidOperationException("Not connected to server");

            // 使用默认超时时间
            if (timeout <= 0) timeout = _defaultTimeout;

            try
            {
                // 序列化请求数据
                byte[] requestData = request.ToByteArray();

                // 发送请求
                var response = await _client.RequestAsync(route, requestData, timeout);

                // 反序列化响应数据
                var parser = GetParser<TResponse>();
                return parser.ParseFrom(response.Data);
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Request failed: {route}, Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 注册服务器推送处理器
        /// </summary>
        public void RegisterPushHandler(string route, Action<object> handler)
        {
            _client?.RegisterPushHandler(route, (message) =>
            {
                // 这里可以添加推送消息的反序列化逻辑
                handler?.Invoke(message);
            });
        }

        // 匹配成功通知处理器注册
        public void RegisterMatchSuccessHandler(Action<MatchFoundNotify> handler)
        {
            RegisterPushHandler("matchmakingsvr.matchmaking.matchfound", (message) =>
            {
                var parser = GetParser<MatchFoundNotify>();
                var notify = parser.ParseFrom((byte[])message);
                handler?.Invoke(notify);
            });
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 获取 Protobuf 消息的解析器
        /// </summary>
        private static Google.Protobuf.MessageParser<T> GetParser<T>()
            where T : Google.Protobuf.IMessage<T>, new()
        {
            var parserProperty = typeof(T).GetProperty("Parser",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            if (parserProperty == null)
                throw new InvalidOperationException($"Type {typeof(T).Name} doesn't have Parser property");

            return (Google.Protobuf.MessageParser<T>)parserProperty.GetValue(null);
        }

        #endregion
    }
}