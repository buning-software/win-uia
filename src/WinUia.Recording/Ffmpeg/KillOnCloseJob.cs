using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinUia.Recording;

/// <summary>
/// A Windows job object that kills its processes when the last handle to it closes, which the OS does when this process
/// ends, however it ends. ffmpeg is assigned to it, so a test host that is killed or crashes does not leave ffmpeg
/// recording the desktop.
/// </summary>
internal static class KillOnCloseJob
{
    private const int ExtendedLimitInformationClass = 9;
    private const uint KillOnJobClose = 0x2000;

    // Held for the life of the process on purpose: closing the handle would kill the processes in the job.
    private static readonly Lazy<nint> Job = new(Create);

    /// <summary>Puts the process in the job. Best effort: without it ffmpeg still stops normally, it just is not killed with the host.</summary>
    public static void TryAssign(Process process)
    {
        try
        {
            var job = Job.Value;
            if (job != nint.Zero)
                AssignProcessToJobObject(job, process.Handle);
        }
        catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception or DllNotFoundException or EntryPointNotFoundException)
        {
            // Already exited, or not allowed to join the job.
        }
    }

    private static nint Create()
    {
        var job = CreateJobObject(nint.Zero, null);
        if (job == nint.Zero)
            return nint.Zero;

        var info = new ExtendedLimitInformation();
        info.Basic.LimitFlags = KillOnJobClose;
        if (!SetInformationJobObject(job, ExtendedLimitInformationClass, ref info, (uint)Marshal.SizeOf<ExtendedLimitInformation>()))
        {
            CloseHandle(job);
            return nint.Zero;
        }

        return job;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BasicLimitInformation
    {
        public long PerProcessUserTimeLimit;
        public long PerJobUserTimeLimit;
        public uint LimitFlags;
        public nuint MinimumWorkingSetSize;
        public nuint MaximumWorkingSetSize;
        public uint ActiveProcessLimit;
        public nuint Affinity;
        public uint PriorityClass;
        public uint SchedulingClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct IoCounters
    {
        public ulong ReadOperationCount;
        public ulong WriteOperationCount;
        public ulong OtherOperationCount;
        public ulong ReadTransferCount;
        public ulong WriteTransferCount;
        public ulong OtherTransferCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ExtendedLimitInformation
    {
        public BasicLimitInformation Basic;
        public IoCounters Io;
        public nuint ProcessMemoryLimit;
        public nuint JobMemoryLimit;
        public nuint PeakProcessMemoryUsed;
        public nuint PeakJobMemoryUsed;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint CreateJobObject(nint attributes, string? name);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetInformationJobObject(nint job, int informationClass, ref ExtendedLimitInformation information, uint length);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AssignProcessToJobObject(nint job, nint process);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(nint handle);
}
