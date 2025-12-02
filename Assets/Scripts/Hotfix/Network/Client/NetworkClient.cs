using System;
using System.Collections.Concurrent;
using System.Threading;
using Cysharp.Threading.Tasks;
using NativeWebSocket;
using PitayaClient.Network.Core;
using PitayaClient.Network.Transport;
using PitayaClient.Protocol;
using UnityEngine;

namespace PitayaClient.Network.Client
{
    /// <summary>
    /// 网络客户端 - 核心业务逻辑层
    /// 职责：协调传输层和协议层，管理连接生命周期，处理请求/响应
    /// </summary>
    public class NetworkClient : IDisposable
    {
        private readonly WebSocketTransport _transport;
        private readonly PitayaProtocol _protocol;

        // 请求管理
        private int _requestId = 0;
        private readonly ConcurrentDictionary<uint, UniTaskCompletionSource<Message>> _pendingRequests = new();
        private readonly ConcurrentDictionary<string, Action<Message>> _pushHandlers = new();

        // 连接管理
        private UniTaskCompletionSource<bool> _handshakeTcs;
        private const float HANDSHAKE_TIMEOUT = 10f;
        private const float HEARTBEAT_INTERVAL = 30f;
        private float _lastHeartbeatTime;
        private bool _disposed = false;

        /// <summary>
        /// 连接状态（直接使用传输层状态）
        /// </summary>
        public bool IsConnected => _transport?.IsConnected ?? false;

        // 业务事件 - 面向应用程序层
        public event Action OnConnected;              // 握手完成，真正可用的连接
        public event Action<string> OnDisconnected;   // 连接断开
        public event Action<Exception> OnError;       // 网络错误

        public NetworkClient()
        {
            _transport = new WebSocketTransport();
            _protocol = new PitayaProtocol();

            // 设置传输层事件处理
            SetupTransportEvents();
            // 设置协议层事件处理
            SetupProtocolEvents();
        }

        /// <summary>
        /// 设置传输层事件处理
        /// </summary>
        private void SetupTransportEvents()
        {
            _transport.OnOpen += OnTransportOpen;
            _transport.OnMessage += OnTransportMessage;
            _transport.OnClose += OnTransportClose;
            _transport.OnError += OnTransportError;
        }

        /// <summary>
        /// 设置协议层事件处理
        /// </summary>
        private void SetupProtocolEvents()
        {
            _protocol.OnMessageReceived += OnProtocolMessageReceived;
            _protocol.OnPacketReceived += OnProtocolPacketReceived;
            _protocol.OnError += OnProtocolError;
        }

        /// <summary>
        /// 清理事件订阅
        /// </summary>
        private void CleanupEventSubscriptions()
        {
            // 传输层事件
            _transport.OnOpen -= OnTransportOpen;
            _transport.OnMessage -= OnTransportMessage;
            _transport.OnClose -= OnTransportClose;
            _transport.OnError -= OnTransportError;

            // 协议层事件
            _protocol.OnMessageReceived -= OnProtocolMessageReceived;
            _protocol.OnPacketReceived -= OnProtocolPacketReceived;
            _protocol.OnError -= OnProtocolError;
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        public async UniTask<bool> ConnectAsync(string host, string path = "/ws")
        {
            try
            {
                string url = $"ws://{host}{path}";
                Debug.Log($"Connecting to server: {url}");

                _handshakeTcs = new UniTaskCompletionSource<bool>();

                // 1. 连接 WebSocket 传输层
                bool transportConnected = await _transport.ConnectAsync(url);
                if (!transportConnected)
                {
                    Debug.LogError("WebSocket transport connection failed");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Connect failed: {ex.Message}");
                OnError?.Invoke(ex);
                await DisconnectAsync();
                return false;
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public async UniTask DisconnectAsync()
        {
            if (_disposed) return;

            try
            {
                Debug.Log("Disconnecting...");
                await _transport.CloseAsync();
                CleanupResources();
                Debug.Log("Disconnected successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Disconnect error: {ex.Message}");
            }
        }

        /// <summary>
        /// 发送请求并等待响应
        /// </summary>
        public async UniTask<Message> RequestAsync(string route, byte[] data, float timeout = 10f)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Not connected to server");

            uint requestId = (uint)Interlocked.Increment(ref _requestId);
            var tcs = new UniTaskCompletionSource<Message>();

            _pendingRequests[requestId] = tcs;

            try
            {
                byte[] packetData = _protocol.CreateRequestPacket(requestId, route, data);
                await _transport.SendAsync(packetData);

                Debug.Log($"Request sent: {route}, ID: {requestId}");

                try
                {
                    return await tcs.Task.Timeout(TimeSpan.FromSeconds(timeout));
                }
                catch (TimeoutException)
                {
                    throw new TimeoutException($"Request timeout: {route}");
                }
            }
            finally
            {
                _pendingRequests.TryRemove(requestId, out _);
            }
        }

        /// <summary>
        /// 注册服务器推送消息处理器
        /// </summary>
        public void RegisterPushHandler(string route, Action<Message> handler)
        {
            _pushHandlers[route] = handler;
        }

        /// <summary>
        /// 取消推送消息处理器
        /// </summary>
        public void UnregisterPushHandler(string route)
        {
            _pushHandlers.TryRemove(route, out _);
        }

        /// <summary>
        /// 更新方法 - 处理消息队列和心跳
        /// </summary>
        public void Update()
        {
            _transport.ProcessMessageQueue();

            if (IsConnected && Time.time - _lastHeartbeatTime >= HEARTBEAT_INTERVAL)
            {
                _ = SendHeartbeatAsync();
                _lastHeartbeatTime = Time.time;
            }
        }

        /// <summary>
        /// 发送心跳包
        /// </summary>
        private async UniTask SendHeartbeatAsync()
        {
            try
            {
                byte[] heartbeatPacket = _protocol.CreateHeartbeatPacket();
                await _transport.SendAsync(heartbeatPacket);
                Debug.Log("Heartbeat sent");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Send heartbeat failed: {ex.Message}");
            }
        }

        #region 传输层事件处理

        private async void OnTransportOpen()
        {
            Debug.Log("WebSocket connection opened");
            // 2. 发送握手请求
            byte[] handshakePacket = _protocol.CreateHandshakePacket();
            await _transport.SendAsync(handshakePacket);

            // 3. 等待握手完成
            try
            {
                await _handshakeTcs.Task.Timeout(TimeSpan.FromSeconds(HANDSHAKE_TIMEOUT));
            }
            catch (TimeoutException)
            {
                throw new TimeoutException($"Handshake timeout after {HANDSHAKE_TIMEOUT} seconds");
            }

            bool handshakeSuccess = await _handshakeTcs.Task;
            if (!handshakeSuccess)
                throw new InvalidOperationException("Handshake failed");

            _lastHeartbeatTime = Time.time;
            OnConnected?.Invoke();
            Debug.Log("Network client connected successfully!");
        }

        private void OnTransportMessage(byte[] data)
        {
            _protocol.ProcessReceivedData(data);
        }

        private void OnTransportClose(WebSocketCloseCode closeCode)
        {
            string reason = GetCloseReason(closeCode);
            Debug.Log($"WebSocket closed: {reason}");
            OnDisconnected?.Invoke(reason);
        }

        private void OnTransportError(string errorMsg)
        {
            Debug.LogError($"WebSocket error: {errorMsg}");
            OnError?.Invoke(new Exception(errorMsg));
        }

        private string GetCloseReason(WebSocketCloseCode closeCode)
        {
            return closeCode switch
            {
                WebSocketCloseCode.Normal => "正常关闭",
                WebSocketCloseCode.Abnormal => "异常关闭",
                WebSocketCloseCode.InvalidData => "无效载荷",
                WebSocketCloseCode.PolicyViolation => "策略违规",
                WebSocketCloseCode.TooBig => "消息过大",
                WebSocketCloseCode.NoStatus => "无状态",
                WebSocketCloseCode.ProtocolError => "协议错误",
                WebSocketCloseCode.UnsupportedData => "不支持的数据",
                WebSocketCloseCode.Undefined => "未定义",
                WebSocketCloseCode.TlsHandshakeFailure => "TLS握手失败",
                _ => $"关闭代码: {(int)closeCode}"
            };
        }

        #endregion

        #region 协议层事件处理

        private void OnProtocolMessageReceived(Message message)
        {
            Debug.Log($"Message received: {message.Type}, Route: {message.Route}");

            switch (message.Type)
            {
                case MessageType.Response:
                    HandleResponseMessage(message);
                    break;
                case MessageType.Push:
                    HandlePushMessage(message);
                    break;
            }
        }

        private void OnProtocolPacketReceived(Packet packet)
        {
            Debug.Log($"Packet received: {packet.Type}");

            switch (packet.Type)
            {
                case PacketType.Handshake:
                    HandleHandshakePacket(packet);
                    break;
                case PacketType.HandshakeAck:
                    Debug.Log("Handshake ACK received");
                    _handshakeTcs?.TrySetResult(true);
                    break;
                case PacketType.Kick:
                    HandleKickPacket(packet);
                    break;
            }
        }

        private void OnProtocolError(Exception ex)
        {
            Debug.LogError($"Protocol error: {ex.Message}");
            OnError?.Invoke(ex);
        }

        private void HandleResponseMessage(Message message)
        {
            if (_pendingRequests.TryGetValue(message.ID, out var tcs))
            {
                tcs.TrySetResult(message);
            }
        }

        private void HandlePushMessage(Message message)
        {
            if (_pushHandlers.TryGetValue(message.Route, out var handler))
            {
                handler.Invoke(message);
            }
        }

        private async void HandleHandshakePacket(Packet packet)
        {
            try
            {
                bool success = _protocol.ProcessHandshakeResponse(packet, out var handshakeJson);
                if (success)
                {
                    byte[] ackPacket = _protocol.CreateHandshakeAckPacket();
                    await _transport.SendAsync(ackPacket);
                    Debug.Log("Handshake ACK sent");
                    _handshakeTcs?.TrySetResult(true);
                }
                else
                {
                    _handshakeTcs?.TrySetResult(false);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Handle handshake error: {ex.Message}");
                _handshakeTcs?.TrySetResult(false);
            }
        }

        private void HandleKickPacket(Packet packet)
        {
            string reason = System.Text.Encoding.UTF8.GetString(packet.Data);
            Debug.LogWarning($"Kicked by server: {reason}");
            OnDisconnected?.Invoke($"Kicked: {reason}");
        }

        #endregion

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Debug.Log("NetworkClient disposing...");
            CleanupEventSubscriptions();
            _transport.Dispose();
            _protocol.Dispose();
            CleanupResources();
            Debug.Log("NetworkClient disposed");
        }

        private void CleanupResources()
        {
            _pendingRequests.Clear();
            _pushHandlers.Clear();

            _handshakeTcs?.TrySetCanceled();
            _handshakeTcs = null;

            OnConnected = null;
            OnDisconnected = null;
            OnError = null;
        }
    }
}