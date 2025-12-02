
using System;
using System.Collections.Generic;
using System.Text;

namespace PitayaClient.Protocol
{
    /// <summary>
    /// Message 类型枚举
    /// </summary>
    public enum MessageType : byte
    {
        /// <summary>请求 (客户端 -> 服务器，需要响应)</summary>
        Request = 0x00,

        /// <summary>通知 (客户端 -> 服务器，不需要响应)</summary>
        Notify = 0x01,

        /// <summary>响应 (服务器 -> 客户端，对应 Request)</summary>
        Response = 0x02,

        /// <summary>推送 (服务器 -> 客户端，主动推送)</summary>
        Push = 0x03
    }
    /// <summary>
    /// Packet 类型枚举
    /// </summary>
    public enum PacketType : byte
    {
        /// <summary>握手请求</summary>
        Handshake = 0x01,

        /// <summary>握手响应</summary>
        HandshakeAck = 0x02,

        /// <summary>心跳</summary>
        Heartbeat = 0x03,

        /// <summary>数据</summary>
        Data = 0x04,

        /// <summary>踢出</summary>
        Kick = 0x05
    }
    /// <summary>
    /// Pitaya/Pomelo 协议 - Message 消息
    /// 格式根据 Type 和 Flag 有所不同，支持路由压缩和 ID
    /// </summary>
    public class Message
    {
        /// <summary>消息类型</summary>
        public MessageType Type { get; set; }

        /// <summary>消息 ID（Request/Response 使用）</summary>
        public uint ID { get; set; }

        /// <summary>路由字符串</summary>
        public string Route { get; set; }

        /// <summary>路由字典 ID（压缩路由使用）</summary>
        public ushort RouteCode { get; set; }

        /// <summary>消息数据（通常是 Protobuf 序列化后的字节）</summary>
        public byte[] Data { get; set; }

        /// <summary>是否使用路由压缩</summary>
        public bool IsCompressed { get; set; }

        /// <summary>是否为错误消息</summary>
        public bool Err { get; set; }

        public Message()
        {
            Data = Array.Empty<byte>();
            Route = string.Empty;
            Err = false;
        }

        public override string ToString()
        {
            string route = IsCompressed ? $"RouteCode={RouteCode}" : $"Route={Route}";
            string error = Err ? ", Error=true" : "";
            return $"Message[Type={Type}, ID={ID}, {route}, DataLen={Data.Length}{error}]";
        }
    }
    /// <summary>
    /// Message 解码器 - 从字节流中解析 Message 对象
    /// 
    /// Flag 字节结构 (与 Pitaya Go 版本一致):
    /// Bit 7-6: 未使用
    /// Bit 5: Error 标志 (0x20)
    /// Bit 4: Gzip 压缩标志 (0x10)
    /// Bit 3-1: Message Type (Request=0, Notify=1, Response=2, Push=3)
    /// Bit 0: Route 压缩标志 (0x01)
    /// </summary>
    public static class MessageDecoder
    {
        // Flag 位定义 (与 Go 版本一致)
        private const byte MSG_ROUTE_COMPRESS_MASK = 0x01; // Bit 0: 路由压缩
        private const byte GZIP_MASK = 0x10;               // Bit 4: Gzip 压缩
        private const byte ERROR_MASK = 0x20;              // Bit 5: 错误标志
        private const byte MSG_TYPE_MASK = 0x07;           // Bits 1-3: 消息类型
        private const int MSG_HEAD_LENGTH = 0x02;          // 最小消息头长度

        /// <summary>
        /// 解码字节数组为 Message
        /// </summary>
        public static Message Decode(byte[] buffer)
        {
            if (buffer == null || buffer.Length < MSG_HEAD_LENGTH)
            {
                throw new ArgumentException("Invalid message: buffer too short");
            }

            Message msg = new Message();
            int offset = 0;

            // 1. 读取 Flag 字节
            byte flag = buffer[offset++];

            // 提取 Type (bits 1-3): (flag >> 1) & 0x07
            msg.Type = (MessageType)((flag >> 1) & MSG_TYPE_MASK);

            // 提取 Error 标志 (bit 5)
            msg.Err = (flag & ERROR_MASK) == ERROR_MASK;

            // 提取 Route 压缩标志 (bit 0)
            msg.IsCompressed = (flag & MSG_ROUTE_COMPRESS_MASK) == MSG_ROUTE_COMPRESS_MASK;

            // 检查是否有 Gzip 压缩 (bit 4)
            bool isGzipped = (flag & GZIP_MASK) == GZIP_MASK;

            // 2. 读取 Message ID (varint 编码)
            // Request 和 Response 类型包含 Message ID
            if (msg.Type == MessageType.Request || msg.Type == MessageType.Response)
            {
                var result = ReadVarint(buffer, offset);
                msg.ID = result.value;
                offset += result.bytesRead;
            }

            // 3. 读取 Route
            // Request, Notify, Push 类型包含路由信息
            bool needRoute = msg.Type == MessageType.Request ||
                           msg.Type == MessageType.Notify ||
                           msg.Type == MessageType.Push;

            if (needRoute)
            {
                if (msg.IsCompressed)
                {
                    // 读取 RouteCode (2 bytes, Big Endian)
                    if (offset + 2 > buffer.Length)
                    {
                        throw new Exception("Invalid message: buffer too short for RouteCode");
                    }
                    msg.RouteCode = (ushort)((buffer[offset] << 8) | buffer[offset + 1]);
                    offset += 2;

                    // Note: 实际的 Route 字符串需要通过字典查找
                    // 这里暂时使用 RouteCode 的字符串表示
                    msg.Route = $"#{msg.RouteCode}";
                }
                else
                {
                    // 读取 Route Length + Route
                    if (offset >= buffer.Length)
                    {
                        throw new Exception("Invalid message: buffer too short for Route length");
                    }

                    byte routeLen = buffer[offset++];

                    if (offset + routeLen > buffer.Length)
                    {
                        throw new Exception("Invalid message: buffer too short for Route");
                    }

                    msg.Route = Encoding.UTF8.GetString(buffer, offset, routeLen);
                    offset += routeLen;
                }
            }

            // 4. 读取 Data
            if (offset > buffer.Length)
            {
                throw new Exception("Invalid message: offset exceeds buffer length");
            }

            int dataLen = buffer.Length - offset;
            if (dataLen > 0)
            {
                msg.Data = new byte[dataLen];
                Array.Copy(buffer, offset, msg.Data, 0, dataLen);

                // 如果数据被 Gzip 压缩，需要解压
                // Note: 需要实现 Gzip 解压功能
                if (isGzipped)
                {
                    // TODO: 实现 Gzip 解压
                    // msg.Data = InflateData(msg.Data);
                }
            }

            return msg;
        }

        /// <summary>
        /// 读取 Varint 编码的整数
        /// </summary>
        private static (uint value, int bytesRead) ReadVarint(byte[] buffer, int offset)
        {
            uint value = 0;
            int shift = 0;
            int bytesRead = 0;

            while (offset + bytesRead < buffer.Length)
            {
                byte b = buffer[offset + bytesRead++];
                value |= (uint)(b & 0x7F) << shift;

                if ((b & 0x80) == 0)
                {
                    return (value, bytesRead);
                }

                shift += 7;
                if (shift >= 32)
                {
                    throw new Exception("Varint too long");
                }
            }

            throw new Exception("Incomplete varint");
        }
    }
    /// <summary>
    /// Message 编码器 - 将 Message 对象编码为字节流
    /// 支持路由压缩、消息 ID 等特性
    /// 
    /// Flag 字节结构 (与 Pitaya Go 版本一致):
    /// Bit 7-6: 未使用
    /// Bit 5: Error 标志 (0x20)
    /// Bit 4: Gzip 压缩标志 (0x10)
    /// Bit 3-1: Message Type (Request=0, Notify=1, Response=2, Push=3)
    /// Bit 0: Route 压缩标志 (0x01)
    /// </summary>
    public static class MessageEncoder
    {
        // Flag 位定义 (与 Go 版本一致)
        private const byte MSG_ROUTE_COMPRESS_MASK = 0x01; // Bit 0: 路由压缩
        private const byte GZIP_MASK = 0x10;               // Bit 4: Gzip 压缩
        private const byte ERROR_MASK = 0x20;              // Bit 5: 错误标志
        private const byte MSG_TYPE_MASK = 0x07;           // Bits 1-3: 消息类型

        /// <summary>
        /// 编码 Message 为字节数组
        /// </summary>
        public static byte[] Encode(Message msg)
        {
            if (msg == null)
            {
                throw new ArgumentNullException(nameof(msg));
            }

            // 计算所需缓冲区大小
            int bufferSize = 1; // Flag (1 byte)

            // Message ID (varint 编码)
            bool hasID = msg.Type == MessageType.Request || msg.Type == MessageType.Response;
            if (hasID)
            {
                bufferSize += GetVarintSize(msg.ID);
            }

            // Route (Request, Notify, Push 需要路由)
            int routeSize = 0;
            byte[] routeBytes = null;
            bool needRoute = msg.Type == MessageType.Request ||
                           msg.Type == MessageType.Notify ||
                           msg.Type == MessageType.Push;

            if (needRoute)
            {
                if (msg.IsCompressed)
                {
                    routeSize = 2; // RouteCode (2 bytes)
                }
                else
                {
                    routeBytes = Encoding.UTF8.GetBytes(msg.Route);
                    routeSize = 1 + routeBytes.Length; // Length(1 byte) + Route
                }
                bufferSize += routeSize;
            }

            // Data
            bufferSize += msg.Data?.Length ?? 0;

            // 创建缓冲区
            byte[] buffer = new byte[bufferSize];
            int offset = 0;

            // 1. 写入 Flag 字节
            // flag = (Type << 1) | routeCompressMask | errorMask | gzipMask
            byte flag = (byte)((byte)msg.Type << 1); // Type 在 bits 1-3

            if (msg.IsCompressed && needRoute)
            {
                flag |= MSG_ROUTE_COMPRESS_MASK; // Bit 0
            }

            if (msg.Err)
            {
                flag |= ERROR_MASK; // Bit 5
            }

            // Note: Gzip 压缩由服务端处理，客户端通常不设置

            buffer[offset++] = flag;

            // 2. 写入 Message ID (varint 编码)
            if (hasID)
            {
                offset += WriteVarint(buffer, offset, msg.ID);
            }

            // 3. 写入 Route
            if (needRoute)
            {
                if (msg.IsCompressed)
                {
                    // 写入 RouteCode (2 bytes, Big Endian)
                    buffer[offset++] = (byte)((msg.RouteCode >> 8) & 0xFF);
                    buffer[offset++] = (byte)(msg.RouteCode & 0xFF);
                }
                else
                {
                    // 写入 Route Length + Route
                    buffer[offset++] = (byte)routeBytes.Length;
                    Array.Copy(routeBytes, 0, buffer, offset, routeBytes.Length);
                    offset += routeBytes.Length;
                }
            }

            // 4. 写入 Data
            if (msg.Data != null && msg.Data.Length > 0)
            {
                Array.Copy(msg.Data, 0, buffer, offset, msg.Data.Length);
            }

            return buffer;
        }

        /// <summary>
        /// 计算 Varint 编码所需字节数
        /// </summary>
        private static int GetVarintSize(uint value)
        {
            if (value < 128) return 1;
            if (value < 16384) return 2;
            if (value < 2097152) return 3;
            return 4;
        }

        /// <summary>
        /// 写入 Varint 编码的整数
        /// </summary>
        private static int WriteVarint(byte[] buffer, int offset, uint value)
        {
            int start = offset;

            while (value >= 128)
            {
                buffer[offset++] = (byte)((value & 0x7F) | 0x80);
                value >>= 7;
            }
            buffer[offset++] = (byte)value;

            return offset - start;
        }
    }

    /// <summary>
    /// Pitaya/Pomelo 协议 - Packet 数据包
    /// 格式: [Type(1 byte)][Length(3 bytes)][Data(N bytes)]
    /// </summary>
    public class Packet
    {
        /// <summary>包头长度（Type + Length）</summary>
        public const int HEAD_LENGTH = 4;

        /// <summary>最大包体大小（16MB）</summary>
        public const int MAX_PACKET_SIZE = 1 << 24; // 16,777,216 bytes

        /// <summary>Packet 类型</summary>
        public PacketType Type { get; set; }

        /// <summary>数据长度</summary>
        public int Length { get; set; }

        /// <summary>数据内容</summary>
        public byte[] Data { get; set; }

        public Packet()
        {
            Data = Array.Empty<byte>();
        }

        public Packet(PacketType type, byte[] data)
        {
            Type = type;
            Data = data ?? Array.Empty<byte>();
            Length = Data.Length;
        }

        public override string ToString()
        {
            return $"Packet[Type={Type}, Length={Length}]";
        }
    }
    /// <summary>
    /// Packet 解码器 - 从字节流中解析 Packet 对象
    /// 支持粘包和半包处理
    /// </summary>
    public class PacketDecoder
    {
        /// <summary>
        /// 解码字节流为 Packet 列表（处理粘包）
        /// </summary>
        /// <param name="buffer">接收到的字节流</param>
        /// <returns>解析出的 Packet 列表</returns>
        public static List<Packet> Decode(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
            {
                return new List<Packet>();
            }

            List<Packet> packets = new List<Packet>();
            int offset = 0;

            while (offset < buffer.Length)
            {
                // 1. 检查是否有足够的数据读取包头
                if (buffer.Length - offset < Packet.HEAD_LENGTH)
                {
                    break; // 数据不足，等待更多数据
                }

                // 2. 解析包头
                byte typeByte = buffer[offset];
                int length = ParseLength(buffer, offset + 1);

                // 验证 Type
                if (!IsValidPacketType(typeByte))
                {
                    throw new Exception($"Invalid packet type: {typeByte}");
                }

                // 验证 Length
                if (length > Packet.MAX_PACKET_SIZE)
                {
                    throw new Exception($"Packet size exceeds maximum: {length} > {Packet.MAX_PACKET_SIZE}");
                }

                // 3. 检查是否有足够的数据读取包体
                if (buffer.Length - offset - Packet.HEAD_LENGTH < length)
                {
                    break; // 包体不完整，等待更多数据
                }

                // 4. 读取包体
                byte[] data = new byte[length];
                if (length > 0)
                {
                    Array.Copy(buffer, offset + Packet.HEAD_LENGTH, data, 0, length);
                }

                // 5. 创建 Packet
                Packet packet = new Packet
                {
                    Type = (PacketType)typeByte,
                    Length = length,
                    Data = data
                };
                packets.Add(packet);

                // 6. 移动偏移量
                offset += Packet.HEAD_LENGTH + length;
            }

            return packets;
        }

        /// <summary>
        /// 解析 3 字节长度字段（大端序）
        /// </summary>
        private static int ParseLength(byte[] buffer, int offset)
        {
            int length = 0;
            length |= (buffer[offset] & 0xFF) << 16;
            length |= (buffer[offset + 1] & 0xFF) << 8;
            length |= (buffer[offset + 2] & 0xFF);
            return length;
        }

        /// <summary>
        /// 验证 Packet 类型是否有效
        /// </summary>
        private static bool IsValidPacketType(byte type)
        {
            return type >= (byte)PacketType.Handshake && type <= (byte)PacketType.Kick;
        }
    }
    /// <summary>
    /// Packet 编码器 - 将 Packet 对象编码为字节流
    /// </summary>
    public static class PacketEncoder
    {
        /// <summary>
        /// 编码 Packet 为字节数组
        /// </summary>
        /// <param name="type">Packet 类型</param>
        /// <param name="data">数据内容</param>
        /// <returns>编码后的字节数组</returns>
        public static byte[] Encode(PacketType type, byte[] data)
        {
            data = data ?? Array.Empty<byte>();

            // 检查包体大小
            if (data.Length > Packet.MAX_PACKET_SIZE)
            {
                throw new Exception($"Packet size exceeds maximum: {data.Length} > {Packet.MAX_PACKET_SIZE}");
            }

            int length = data.Length;
            byte[] buffer = new byte[Packet.HEAD_LENGTH + length];

            // 写入 Type (1 byte)
            buffer[0] = (byte)type;

            // 写入 Length (3 bytes, Big Endian)
            buffer[1] = (byte)((length >> 16) & 0xFF);
            buffer[2] = (byte)((length >> 8) & 0xFF);
            buffer[3] = (byte)(length & 0xFF);

            // 写入 Data
            if (length > 0)
            {
                Array.Copy(data, 0, buffer, Packet.HEAD_LENGTH, length);
            }

            return buffer;
        }

        /// <summary>
        /// 编码 Packet 对象为字节数组
        /// </summary>
        public static byte[] Encode(Packet packet)
        {
            return Encode(packet.Type, packet.Data);
        }
    }
}

