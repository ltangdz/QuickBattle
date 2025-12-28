using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Quic;
using System.Numerics;
using System.Threading;
using MessagePack;
using Microsoft.Quic;
using QuicServer; // 需安装 MsQuic NuGet 包

class GameServer
{
    private static QuicListener _listener;
    private static Dictionary<int, PlayerState> _playerStates = new(); // 权威状态
    private static List<QuicConnection> _connections = new(); // 客户端连接列表

    static void Main(string[] args)
    {
        // 初始化服务器（监听 4567 端口，使用自签名证书）
        var cert = new QuicCertificate("test_cert.pfx", "password"); // 测试证书
        _listener = new QuicListener(new QuicListenerOptions
        {
            ApplicationProtocols = new[] { "game-quic" },
            ListenEndPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 4567),
            Certificate = cert
        });

        Console.WriteLine("服务器启动，监听端口 4567...");
        _ = AcceptClientsAsync(); // 异步接受客户端连接

        // 启动状态广播协程（20Hz）
        var broadcastThread = new Thread(BroadcastStatesLoop);
        broadcastThread.IsBackground = true;
        broadcastThread.Start();

        Console.ReadLine(); // 阻塞主线程
    }

    // 接受客户端连接
    private static async System.Threading.Tasks.Task AcceptClientsAsync()
    {
        while (true)
        {
            var connection = await _listener.AcceptConnectionAsync();
            _connections.Add(connection);
            Console.WriteLine("新客户端连接");

            // 为每个客户端启动消息处理线程
            _ = HandleClientAsync(connection);
        }
    }

    // 处理客户端指令
    private static async System.Threading.Tasks.Task HandleClientAsync(QuicConnection connection)
    {
        var stream = await connection.AcceptStreamAsync(); // 接收流
        var buffer = new byte[4096];

        while (true)
        {
            try
            {
                int bytesRead = await stream.ReadAsync(buffer);
                if (bytesRead == 0) break;

                // 反序列化指令并更新权威状态
                var cmd = MessagePackSerializer.Deserialize<InputCommand>(buffer, 0, bytesRead);
                UpdatePlayerState(cmd);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"客户端断开：{ex.Message}");
                _connections.Remove(connection);
                break;
            }
        }
    }

    // 更新服务器权威状态
    private static void UpdatePlayerState(InputCommand cmd)
    {
        if (!_playerStates.ContainsKey(cmd.PlayerId))
        {
            // 新玩家初始化状态
            _playerStates[cmd.PlayerId] = new PlayerState
            {
                PlayerId = cmd.PlayerId,
                Position = Vector3.zero,
                Rotation = Quaternion.identity,
                Health = 100
            };
        }

        // 根据指令更新位置（简单逻辑）
        var state = _playerStates[cmd.PlayerId];
        state.Position += new Vector3(cmd.MoveDir.x, 0, cmd.MoveDir.z) * 0.5f;
        state.Timestamp = DateTime.UtcNow.Ticks;
    }

    // 广播权威状态到所有客户端（20Hz）
    private static void BroadcastStatesLoop()
    {
        while (true)
        {
            if (_connections.Count == 0)
            {
                Thread.Sleep(50);
                continue;
            }

            // 序列化所有玩家状态
            var states = _playerStates.Values.ToArray();
            byte[] data = MessagePackSerializer.Serialize(states);

            // 发送到所有连接的客户端
            foreach (var conn in _connections)
            {
                try
                {
                    var stream = conn.OpenUnidirectionalStream();
                    await stream.WriteAsync(data);
                    await stream.ShutdownAsync();
                }
                catch { /* 忽略断开的客户端 */ }
            }

            Thread.Sleep(50); // 50ms 间隔（20Hz）
        }
    }
}