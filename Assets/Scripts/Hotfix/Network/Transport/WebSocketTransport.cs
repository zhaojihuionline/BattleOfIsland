using System;
using Cysharp.Threading.Tasks;
using NativeWebSocket;
using UnityEngine;

namespace PitayaClient.Network.Transport
{
    /// <summary>
    /// WebSocket 传输层 - 内部管理 WebSocket 事件生命周期
    /// 职责：创建、管理 WebSocket 实例，处理所有底层事件
    /// </summary>
    public class WebSocketTransport : IDisposable
    {
        private WebSocket _webSocket;

        /// <summary>
        /// 连接状态（基于 WebSocket 原生状态）
        /// </summary>
        public bool IsConnected => _webSocket?.State == WebSocketState.Open;

        // 简洁的事件接口 - 使用 Action 委托
        public event Action OnOpen;                          // 连接打开
        public event Action<byte[]> OnMessage;              // 收到消息
        public event Action<WebSocketCloseCode> OnClose;    // 连接关闭
        public event Action<string> OnError;                // 发生错误

        /// <summary>
        /// 连接到 WebSocket 服务器
        /// </summary>
        public async UniTask<bool> ConnectAsync(string url)
        {
            try
            {
                if (IsConnected)
                {
                    Debug.LogWarning("WebSocket already connected");
                    return true;
                }

                // 创建 WebSocket 实例
                _webSocket = new WebSocket(url);

                // 内部设置事件处理
                SetupWebSocketEvents();

                Debug.Log($"Connecting to {url}...");
                await _webSocket.Connect();
                Debug.Log("WebSocket connected successfully");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"WebSocket connect failed: {ex.Message}");
                OnError?.Invoke($"Connect failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 发送二进制数据
        /// </summary>
        public async UniTask SendAsync(byte[] data)
        {
            if (!IsConnected)
                throw new InvalidOperationException("WebSocket is not connected");

            try
            {
                await _webSocket.Send(data);
            }
            catch (Exception ex)
            {
                Debug.LogError($"WebSocket send failed: {ex.Message}");
                OnError?.Invoke($"Send failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 关闭 WebSocket 连接
        /// </summary>
        public async UniTask CloseAsync()
        {
            if (_webSocket == null || !IsConnected)
                return;

            try
            {
                await _webSocket.Close();
                Debug.Log("WebSocket closed");
            }
            catch (Exception ex)
            {
                Debug.LogError($"WebSocket close failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理消息队列 - 必须在主线程的 Update 中调用
        /// </summary>
        public void ProcessMessageQueue()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            _webSocket?.DispatchMessageQueue();
#endif
        }

        /// <summary>
        /// 内部设置 WebSocket 事件处理
        /// </summary>
        private void SetupWebSocketEvents()
        {
            _webSocket.OnOpen += () => OnOpen?.Invoke();
            _webSocket.OnMessage += (bytes) => OnMessage?.Invoke(bytes);
            _webSocket.OnClose += (code) => OnClose?.Invoke(code);
            _webSocket.OnError += (error) => OnError?.Invoke(error);
        }

        /// <summary>
        /// 内部清理 WebSocket 事件处理
        /// </summary>
        private void CleanupWebSocketEvents()
        {
            if (_webSocket == null) return;

            // 由于使用的是匿名方法，无法直接取消订阅
            // 在 Dispose 时直接置空 WebSocket 即可
        }

        public void Dispose()
        {
            _webSocket?.Close();
            _webSocket = null;

            // 清理事件订阅
            OnOpen = null;
            OnMessage = null;
            OnClose = null;
            OnError = null;
        }
    }
}