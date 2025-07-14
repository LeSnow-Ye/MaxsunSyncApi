// Modified from decompiled MaxsunSync.exe code

using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;

namespace MaxsunSyncApi;

#region MaxsunService

/// <summary>
/// Provides methods to interact with the MaxsunSync service.
/// Note that MaxsunSync must be installed and its service "CppWindowsService" is running.
/// Also, currently, only Windows is supported.
/// Administrator privileges are required to use this API.
/// </summary>
public static class MaxsunService
{
    public static void CheckService()
    {
        var serviceController = new ServiceController("CppWindowsService");
        if (serviceController.Status != ServiceControllerStatus.Running)
        {
            serviceController.Start();
            serviceController.WaitForStatus(
                ServiceControllerStatus.Running,
                TimeSpan.FromSeconds(30.0)
            );
        }

        serviceController.Close();
    }

    public static List<DeviceInfo> ScanDevice()
    {
        var list = new List<DeviceInfo>();
        using var namedPipeClientStream = new NamedPipeClientStream(
            ".",
            "ScanDevice",
            PipeDirection.InOut
        );

        namedPipeClientStream.Connect();
        var value = new string[2] { "", "" };
        var bytes = Encoding.UTF8.GetBytes(string.Join(",", value));
        namedPipeClientStream.Write(bytes, 0, bytes.Length);
        var array = new byte[4096];

        int num;

        do
        {
            num = namedPipeClientStream.Read(array, 0, array.Length);
        } while (num == 0);

        var array2 = Encoding.UTF8.GetString(array, 0, num).Split(';');
        for (var i = 0; i < array2.Length; i++)
        {
            var array4 = array2[i].Split(',');
            var deviceName = array4[0];
            var deviceType = int.Parse(array4[1]);
            var num3 = int.Parse(array4[2]);
            var item = new DeviceInfo
            {
                index = i,
                deviceName = deviceName,
                deviceType = (DeviceType)deviceType,
                syncStatus = num3 == 1,
            };

            list.Add(item);
        }

        namedPipeClientStream.Close();
        namedPipeClientStream.Dispose();
        return list;
    }

    public static void ApplyEffectForSleep() =>
        ApplyEffect(0, EffectMode.Single, 30, 70, 0.0, 0.0, 0.0, 0);

    public static void ApplyEffect(
        int speed = 1,
        EffectMode mode = EffectMode.Single,
        int cpuLow = 30,
        int cpuHigh = 70,
        double hue = 220,
        double sat = 0.73,
        double val = 1,
        int musicMode = 0
    )
    {
        using var namedPipeClientStream = new NamedPipeClientStream(
            ".",
            "ApplyEffect",
            PipeDirection.Out
        );

        namedPipeClientStream.Connect();

        var structure = new EffectData
        {
            speed = speed,
            mode = (int)mode,
            cpuLow = cpuLow,
            cpuHigh = cpuHigh,
            musicMode = musicMode,
            h = hue,
            s = sat,
            v = val,
            milliSecs = 0.0,
        };

        var deviceList = ScanDevice();
        var syncStr = string.Join(
            ";",
            deviceList.Select(d => $"{(int)d.deviceType},{(d.syncStatus ? 1 : 0)}")
        );

        structure.syncStr = syncStr;
        var array = new byte[Marshal.SizeOf<EffectData>()];
        var intPtr = Marshal.AllocHGlobal(Marshal.SizeOf<EffectData>());
        Marshal.StructureToPtr(structure, intPtr, fDeleteOld: false);
        Marshal.Copy(intPtr, array, 0, array.Length);
        namedPipeClientStream.Write(array, 0, array.Length);
        Marshal.FreeHGlobal(intPtr);
        namedPipeClientStream.Close();
        namedPipeClientStream.Dispose();
    }

    public static List<bool> GetSyncStatusList()
    {
        using var namedPipeClientStream = new NamedPipeClientStream(
            ".",
            "SyncStatus",
            PipeDirection.InOut
        );

        namedPipeClientStream.Connect();
        var bytes = Encoding.UTF8.GetBytes("-1");
        namedPipeClientStream.Write(bytes, 0, bytes.Length);
        var array = new byte[4096];
        int num;

        do
        {
            num = namedPipeClientStream.Read(array, 0, array.Length);
        } while (num == 0);

        var list = new List<bool>();
        var text = Encoding.UTF8.GetString(array, 0, num);
        for (var i = 0; i < text.Length; i++)
        {
            switch (text[i])
            {
                case '1':
                    list.Add(item: true);
                    break;
                case '0':
                    list.Add(item: false);
                    break;
            }
        }

        namedPipeClientStream.Close();
        namedPipeClientStream.Dispose();
        return list;
    }

    public static bool SetSyncStatus(List<bool> syncStatusList)
    {
        var stringBuilder = new StringBuilder();
        foreach (var syncStatus in syncStatusList)
            stringBuilder.Append(syncStatus ? "1" : "0");

        var s = stringBuilder.ToString();

        using var namedPipeClientStream = new NamedPipeClientStream(
            ".",
            "SyncStatus",
            PipeDirection.InOut
        );

        namedPipeClientStream.Connect();
        var bytes = Encoding.UTF8.GetBytes(s);
        namedPipeClientStream.Write(bytes, 0, bytes.Length);
        namedPipeClientStream.Close();
        namedPipeClientStream.Dispose();
        return true;
    }
}

#endregion

#region Data Types

public enum EffectMode
{
    Single = 1,
    Breathing,
    ColorCycle,
    Rainbow,

    /// <summary>
    /// By CPU usage or CPU temperature? Idk, I think this is broken.
    /// </summary>
    CPU,
    Music,
    Close,
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct EffectData
{
    public int speed;

    public int mode;

    public int cpuLow;

    public int cpuHigh;

    public int musicMode;

    public double h;

    public double s;

    public double v;

    public double milliSecs;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string syncStr;
}

public enum DeviceType
{
    MotherBoard,
    Keyboard,
    Mouse,
    Dram,
    Argb,
    WaterCooler,
    Vga,
}

public struct DeviceInfo
{
    public int index;

    public string deviceName;

    public DeviceType deviceType;

    public bool syncStatus;
}

#endregion
