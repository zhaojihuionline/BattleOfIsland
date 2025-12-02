using System;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEngine;
using PitayaClient.Protocol;

namespace PitayaClient.Network.Core
{
    /// <summary>
    /// Pitaya 协议处理器
    /// 职责：Pitaya 协议的编码、解码、握手、心跳处理
    /// 纯协议逻辑，不依赖具体的传输层
    /// </summary>
    public class PitayaProtocol : IDisposable
    {
        // 路由字典：路由字符串 ↔ 路由代码
        private readonly Dictionary<string, ushort> _routeDict = new();
        private readonly Dictionary<ushort, string> _routeReverseDict = new();

        // 协议层事件
        public event Action<Message> OnMessageReceived;
        public event Action<Packet> OnPacketReceived;
        public event Action<Exception> OnError;

        public PitayaProtocol()
        {
            InitializeRoutes();
        }

        /// <summary>
        /// 初始化预定义的路由字典
        /// </summary>
        private void InitializeRoutes()
        {
            // 网关相关路由
            AddRoute("gateway.handshake", 1);
            AddRoute("gateway.heartbeat", 2);

            // 登录服务路由
            AddRoute("loginsvr.auth.login", 3);
            AddRoute("loginsvr.auth.register", 4);
            AddRoute("loginsvr.auth.verifyToken", 5);

            // 游戏服务路由
            AddRoute("gamesvr.game.getUserInfo", 6);
            AddRoute("gamesvr.game.updateProfile", 7);
        }

        private void AddRoute(string route, ushort code)
        {
            _routeDict[route] = code;
            _routeReverseDict[code] = route;
        }

        #region 数据包创建方法

        /// <summary>
        /// 创建握手数据包
        /// </summary>
        public byte[] CreateHandshakePacket()
        {
            string handshakeData = "{\"sys\":{\"type\":\"unity\",\"version\":\"1.0.0\"}}";
            byte[] data = System.Text.Encoding.UTF8.GetBytes(handshakeData);
            return PacketEncoder.Encode(PacketType.Handshake, data);
        }

        /// <summary>
        /// 创建心跳数据包
        /// </summary>
        public byte[] CreateHeartbeatPacket()
        {
            return PacketEncoder.Encode(PacketType.Heartbeat, Array.Empty<byte>());
        }

        /// <summary>
        /// 创建握手确认数据包
        /// </summary>
        public byte[] CreateHandshakeAckPacket()
        {
            return PacketEncoder.Encode(PacketType.HandshakeAck, Array.Empty<byte>());
        }

        /// <summary>
        /// 创建请求数据包
        /// </summary>
        public byte[] CreateRequestPacket(uint requestId, string route, byte[] data)
        {
            var message = new Message
            {
                Type = MessageType.Request,
                ID = requestId,
                Route = route,
                Data = data ?? Array.Empty<byte>()
            };

            // 使用路由压缩（如果路由在字典中）
            if (_routeDict.ContainsKey(route))
            {
                message.IsCompressed = true;
                message.RouteCode = _routeDict[route];
            }

            byte[] messageData = MessageEncoder.Encode(message);
            return PacketEncoder.Encode(PacketType.Data, messageData);
        }

        #endregion

        #region 数据包处理方法

        /// <summary>
        /// 处理接收到的原始数据
        /// </summary>
        public void ProcessReceivedData(byte[] data)
        {
            try
            {
                var packets = PacketDecoder.Decode(data);
                foreach (var packet in packets)
                {
                    HandlePacket(packet);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Process received data error: {ex.Message}");
                OnError?.Invoke(ex);
            }
        }

        /// <summary>
        /// 处理单个数据包
        /// </summary>
        private void HandlePacket(Packet packet)
        {
            OnPacketReceived?.Invoke(packet);

            switch (packet.Type)
            {
                case PacketType.Data:
                    HandleDataPacket(packet);
                    break;
                case PacketType.Handshake:
                case PacketType.HandshakeAck:
                case PacketType.Heartbeat:
                case PacketType.Kick:
                    // 这些包由上层处理业务逻辑
                    break;
                default:
                    Debug.LogWarning($"⚠️ Unknown packet type: {packet.Type}");
                    break;
            }
        }

        /// <summary>
        /// 处理数据包中的消息
        /// </summary>
        private void HandleDataPacket(Packet packet)
        {
            try
            {
                var message = MessageDecoder.Decode(packet.Data);

                // 解压缩路由
                if (message.IsCompressed && _routeReverseDict.ContainsKey(message.RouteCode))
                {
                    message.Route = _routeReverseDict[message.RouteCode];
                }

                OnMessageReceived?.Invoke(message);
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Decode message error: {ex.Message}");
                OnError?.Invoke(ex);
            }
        }

        /// <summary>
        /// 处理握手响应
        /// </summary>
        public bool ProcessHandshakeResponse(Packet handshakePacket, out string handshakeJson)
        {
            handshakeJson = null;

            try
            {
                byte[] data = handshakePacket.Data;
                if (IsCompressed(data))
                {
                    byte[] decompressed = InflateData(data);
                    handshakeJson = System.Text.Encoding.UTF8.GetString(decompressed);
                }
                else
                {
                    handshakeJson = System.Text.Encoding.UTF8.GetString(data ?? Array.Empty<byte>());
                }

                Debug.Log($"🤝 Handshake response: {handshakeJson}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Process handshake error: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region 压缩处理

        /// <summary>
        /// 检查数据是否被压缩
        /// </summary>
        private bool IsCompressed(byte[] data)
        {
            if (data == null || data.Length <= 2) return false;

            // zlib 格式检测
            if (data[0] == 0x78 && (data[1] == 0x9C || data[1] == 0x01 || data[1] == 0xDA || data[1] == 0x5E))
                return true;

            // gzip 格式检测
            if (data[0] == 0x1F && data[1] == 0x8B)
                return true;

            return false;
        }

        /// <summary>
        /// 解压缩数据
        /// </summary>
        private byte[] InflateData(byte[] data)
        {
            if (data == null || data.Length == 0) return Array.Empty<byte>();

            // gzip 格式
            if (data.Length >= 2 && data[0] == 0x1F && data[1] == 0x8B)
            {
                using (var ms = new System.IO.MemoryStream(data))
                using (var gz = new GZipStream(ms, CompressionMode.Decompress))
                using (var outMs = new System.IO.MemoryStream())
                {
                    gz.CopyTo(outMs);
                    return outMs.ToArray();
                }
            }

            // zlib 格式
            if (data.Length >= 2 && data[0] == 0x78)
            {
                int start = 2;
                int len = Math.Max(0, data.Length - 6);

                using (var ms = new System.IO.MemoryStream(data, start, len))
                using (var deflate = new DeflateStream(ms, CompressionMode.Decompress))
                using (var outMs = new System.IO.MemoryStream())
                {
                    deflate.CopyTo(outMs);
                    return outMs.ToArray();
                }
            }

            throw new System.IO.InvalidDataException("Data is not in a recognized compressed format");
        }

        #endregion

        public void Dispose()
        {
            _routeDict.Clear();
            _routeReverseDict.Clear();
            OnMessageReceived = null;
            OnPacketReceived = null;
            OnError = null;
        }
    }
}