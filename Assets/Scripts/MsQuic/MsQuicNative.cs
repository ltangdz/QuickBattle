using System;
using System.Runtime.InteropServices;
using UnityEngine;

// 基础类型映射
using QUIC_STATUS = System.UInt32;
using HQUIC = System.IntPtr;
using BOOLEAN = System.Byte;
using QUIC_UINT64 = System.UInt64;
using QUIC_PORT = System.UInt16;
using QUIC_FLAGS = System.UInt32;
using QUIC_PARAM_LEVEL = System.UInt32;
using QUIC_PARAM_ID = System.UInt32;

// 核心常量定义
internal static class QuicConstants
{
    // 版本定义
    public const uint QUIC_VERSION_1 = 1;
    public const uint QUIC_VERSION_2 = 2;

    // 状态码
    public const QUIC_STATUS QUIC_STATUS_SUCCESS = 0;
    public const QUIC_STATUS QUIC_STATUS_PENDING = 0x8000000A;
    public const QUIC_STATUS QUIC_STATUS_NOT_SUPPORTED = 0x80004002;
    public const QUIC_STATUS QUIC_STATUS_INVALID_PARAMETER = 0x80070057;
    public const QUIC_STATUS QUIC_STATUS_OUT_OF_MEMORY = 0x8007000E;
    public const QUIC_STATUS QUIC_STATUS_CONNECTION_REFUSED = 0x80000014;
    public const QUIC_STATUS QUIC_STATUS_ABORTED = 0x80000016;

    // 参数级别
    public const QUIC_PARAM_LEVEL QUIC_PARAM_LEVEL_GLOBAL = 0;
    public const QUIC_PARAM_LEVEL QUIC_PARAM_LEVEL_REGISTRATION = 1;
    public const QUIC_PARAM_LEVEL QUIC_PARAM_LEVEL_CONFIGURATION = 2;
    public const QUIC_PARAM_LEVEL QUIC_PARAM_LEVEL_LISTENER = 3;
    public const QUIC_PARAM_LEVEL QUIC_PARAM_LEVEL_CONNECTION = 4;
    public const QUIC_PARAM_LEVEL QUIC_PARAM_LEVEL_STREAM = 5;

    // 事件类型
    public const uint QUIC_LISTENER_EVENT_NEW_CONNECTION = 1;
    public const uint QUIC_CONNECTION_EVENT_CONNECTED = 2;
    public const uint QUIC_CONNECTION_EVENT_SHUTDOWN_COMPLETE = 3;
    public const uint QUIC_STREAM_EVENT_RECEIVE = 4;
    public const uint QUIC_STREAM_EVENT_SEND_COMPLETE = 5;

    // 标志位
    public const QUIC_FLAGS QUIC_STREAM_FLAG_NONE = 0;
    public const QUIC_FLAGS QUIC_STREAM_FLAG_CLIENT_INITIATED = 1 << 0;
    public const QUIC_FLAGS QUIC_STREAM_FLAG_UNIDIRECTIONAL = 1 << 1;
}

// 回调委托定义
internal unsafe delegate QUIC_STATUS QUIC_LISTENER_CALLBACK(
    HQUIC Listener,
    IntPtr Context,
    QUIC_LISTENER_EVENT* Event
);

internal unsafe delegate QUIC_STATUS QUIC_CONNECTION_CALLBACK(
    HQUIC Connection,
    IntPtr Context,
    QUIC_CONNECTION_EVENT* Event
);

internal unsafe delegate QUIC_STATUS QUIC_STREAM_CALLBACK(
    HQUIC Stream,
    IntPtr Context,
    QUIC_STREAM_EVENT* Event
);

// 事件结构体
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct QUIC_LISTENER_EVENT
{
    public uint Type;
    public fixed byte Data[128]; // 事件数据联合体
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct QUIC_CONNECTION_EVENT
{
    public uint Type;
    public fixed byte Data[256]; // 事件数据联合体
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct QUIC_STREAM_EVENT
{
    public uint Type;
    public fixed byte Data[192]; // 事件数据联合体
}

// 辅助结构体
[StructLayout(LayoutKind.Sequential)]
internal struct QUIC_BUFFER
{
    public IntPtr Buffer; // 数据指针
    public uint Length;   // 数据长度
}

[StructLayout(LayoutKind.Sequential)]
internal struct QUIC_ADDR
{
    public byte[/*16*/] Address; // IPv6地址（16字节）
    public QUIC_PORT Port;
}

// API表结构（完整版本）
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct QUIC_API_TABLE
{
    // 上下文操作函数
    public delegate* unmanaged[Cdecl]<HQUIC, IntPtr, QUIC_STATUS> SetContext;
    public delegate* unmanaged[Cdecl]<HQUIC, out IntPtr, QUIC_STATUS> GetContext;
    public delegate* unmanaged[Cdecl]<HQUIC, IntPtr, QUIC_STATUS> SetCallbackHandler;

    // 参数操作函数
    public delegate* unmanaged[Cdecl]<HQUIC, uint, nuint, nuint, QUIC_STATUS> SetParam;
    public delegate* unmanaged[Cdecl]<HQUIC, uint, nuint, nuint, QUIC_STATUS> GetParam;

    // 注册操作函数
    public delegate* unmanaged[Cdecl]<IntPtr, out HQUIC, QUIC_STATUS> RegistrationOpen;
    public delegate* unmanaged[Cdecl]<HQUIC, void> RegistrationClose;
    public delegate* unmanaged[Cdecl]<HQUIC, QUIC_UINT64, QUIC_STATUS> RegistrationShutdown;

    // 配置操作函数
    public delegate* unmanaged[Cdecl]<HQUIC, out HQUIC, QUIC_STATUS> ConfigurationOpen;
    public delegate* unmanaged[Cdecl]<HQUIC, void> ConfigurationClose;
    public delegate* unmanaged[Cdecl]<HQUIC, IntPtr, QUIC_STATUS> ConfigurationLoadCredential;

    // 监听器操作函数
    public delegate* unmanaged[Cdecl]<HQUIC, IntPtr, out HQUIC, QUIC_STATUS> ListenerOpen;
    public delegate* unmanaged[Cdecl]<HQUIC, void> ListenerClose;
    public delegate* unmanaged[Cdecl]<HQUIC, IntPtr, ushort, QUIC_STATUS> ListenerStart;
    public delegate* unmanaged[Cdecl]<HQUIC, QUIC_STATUS> ListenerStop;

    // 连接操作函数
    public delegate* unmanaged[Cdecl]<HQUIC, IntPtr, out HQUIC, QUIC_STATUS> ConnectionOpen;
    public delegate* unmanaged[Cdecl]<HQUIC, void> ConnectionClose;
    public delegate* unmanaged[Cdecl]<HQUIC, uint, QUIC_UINT64, QUIC_STATUS> ConnectionShutdown;
    public delegate* unmanaged[Cdecl]<HQUIC, HQUIC, byte, string, ushort, QUIC_STATUS> ConnectionStart;
    public delegate* unmanaged[Cdecl]<HQUIC, HQUIC, QUIC_STATUS> ConnectionSetConfiguration;
    public delegate* unmanaged[Cdecl]<HQUIC, uint, ushort, byte*, QUIC_STATUS> ConnectionSendResumptionTicket;

    // 流操作函数
    public delegate* unmanaged[Cdecl]<HQUIC, uint, IntPtr, IntPtr, out HQUIC, QUIC_STATUS> StreamOpen;
    public delegate* unmanaged[Cdecl]<HQUIC, void> StreamClose;
    public delegate* unmanaged[Cdecl]<HQUIC, uint, QUIC_STATUS> StreamStart;
    public delegate* unmanaged[Cdecl]<HQUIC, uint, QUIC_UINT64, QUIC_STATUS> StreamShutdown;
    public delegate* unmanaged[Cdecl]<HQUIC, QUIC_BUFFER*, uint, uint, IntPtr, QUIC_STATUS> StreamSend;
    public delegate* unmanaged[Cdecl]<HQUIC, ulong, void> StreamReceiveComplete;
    public delegate* unmanaged[Cdecl]<HQUIC, BOOLEAN, QUIC_STATUS> StreamReceiveSetEnabled;

    // 数据报操作函数
    public delegate* unmanaged[Cdecl]<HQUIC, QUIC_BUFFER*, uint, uint, IntPtr, QUIC_STATUS> DatagramSend;

    // v2.2 新增函数
    public delegate* unmanaged[Cdecl]<HQUIC, BOOLEAN, QUIC_STATUS> ConnectionResumptionTicketValidationComplete;
    public delegate* unmanaged[Cdecl]<HQUIC, BOOLEAN, uint, QUIC_STATUS> ConnectionCertificateCertificateValidationComplete;

    // v2.5 新增函数
    public delegate* unmanaged[Cdecl]<HQUIC, IntPtr, IntPtr, out HQUIC, QUIC_STATUS> ConnectionOpenInPartition;

    // 预览特性（可选）
#if QUIC_API_ENABLE_PREVIEW_FEATURES
    public delegate* unmanaged[Cdecl]<HQUIC, uint, QUIC_BUFFER*, QUIC_STATUS> StreamProvideReceiveBuffers;
    public delegate* unmanaged[Cdecl]<IntPtr, HQUIC*, QUIC_STATUS> ConnectionPoolCreate;

#if !_KERNEL_MODE
    public delegate* unmanaged[Cdecl]<IntPtr, out IntPtr, QUIC_STATUS> ExecutionCreate;
    public delegate* unmanaged[Cdecl]<IntPtr, void> ExecutionDelete;
    public delegate* unmanaged[Cdecl]<IntPtr, uint, QUIC_STATUS> ExecutionPoll;
#endif

    public delegate* unmanaged[Cdecl]<HQUIC, uint, void> RegistrationClose2;
#endif

    // 补充v2版本常用函数
    public delegate* unmanaged[Cdecl]<HQUIC, QUIC_UINT64, QUIC_STATUS> ConnectionGetStats;
    public delegate* unmanaged[Cdecl]<HQUIC, QUIC_UINT64, QUIC_STATUS> StreamGetStats;
    public delegate* unmanaged[Cdecl]<HQUIC, IntPtr, QUIC_STATUS> ConfigurationSetAlpn;
    public delegate* unmanaged[Cdecl]<HQUIC, IntPtr, QUIC_STATUS> ConfigurationGetAlpn;
}

public static class MsQuicNative
{
    private const string MSQUIC_DLL = "msquic";
    
    // 版本相关函数
    [DllImport(MSQUIC_DLL, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern QUIC_STATUS MsQuicOpenVersion([In] uint version, [Out] out IntPtr quicApi);

    [DllImport(MSQUIC_DLL, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void MsQuicClose([In] IntPtr quicApi);

    [DllImport(MSQUIC_DLL, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern QUIC_STATUS MsQuicGetVersion([Out] out uint Version);

    [DllImport(MSQUIC_DLL, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern IntPtr MsQuicGetStringFromStatus(QUIC_STATUS Status);

    // 辅助函数：获取状态码对应的字符串描述
    public static string GetStatusString(QUIC_STATUS status)
    {
        var ptr = MsQuicGetStringFromStatus(status);
        return Marshal.PtrToStringAnsi(ptr);
    }

    // MsQuicOpen2 封装（Version=2）
    public static QUIC_STATUS MsQuicOpen2(out IntPtr quicApi)
    {
        return MsQuicOpenVersion(QuicConstants.QUIC_VERSION_2, out quicApi);
    }
}