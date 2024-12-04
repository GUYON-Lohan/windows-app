using System;
using System.Runtime.InteropServices;

namespace App.Library.Utility;

public static class ArchitectureHelper
{
    [StructLayout(LayoutKind.Sequential)]
    struct SYSTEM_INFO
    {
        public ushort wProcessorArchitecture;
        public ushort wReserved;
        public uint dwPageSize;
        public IntPtr lpMinimumApplicationAddress;
        public IntPtr lpMaximumApplicationAddress;
        public IntPtr dwActiveProcessorMask;
        public uint dwNumberOfProcessors;
        public uint dwProcessorType;
        public uint dwAllocationGranularity;
        public ushort wProcessorLevel;
        public ushort wProcessorRevision;
    }

    // P/Invoke for GetNativeSystemInfo
    [DllImport("kernel32.dll")]
    static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo);

    public static bool IsArm64()
    {
        return GetArchitecture() is "arm" or "arm64"; // ARM of ARM64
    }

    public static bool IsX64()
    {
        return GetArchitecture() is "amd64"; // AMD64
    }

    public static string GetArchitecture()
    {
        SYSTEM_INFO sysInfo = new SYSTEM_INFO();
        GetNativeSystemInfo(ref sysInfo);

        switch(sysInfo.wProcessorArchitecture)
        {
            case 0:
                return "x86";
            case 5:
                return "arm";
            case 9:
                return "amd64";
            case 12:
                return "arm64";

            default:
                return "Unknown";
        }
    }
}