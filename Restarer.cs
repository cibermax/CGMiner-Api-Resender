using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Management;
using System.Threading;

namespace CGMiner_Api_Resender
{
    class Restarer
    {
        private static void _cw(string text)
        {
            Program.CW("Restarter: " + text);
        }

        public static void DelayedRestart(int delay)
        {
           
            var delayFunc = new Action(() =>
            {
                for (int i = 0; i < 5; i++ )
                {
                    _cw("Reboot in " + (delay - i) + " sec!!!");
                    Thread.Sleep(1000);
                }
                Restart();
            });
            
            delayFunc.Invoke();
        }

        public static void Restart()
        {
            _cw("Asked for reboot");
            _cw("Rebooting...");
            var mcWin32 = new ManagementClass("Win32_OperatingSystem");
            mcWin32.Get();

            Console.WriteLine(!TokenAdjuster.EnablePrivilege("SeShutdownPrivilege", true)
                                  ? "Could not enable SeShutdownPrivilege"
                                  : "Enabled SeShutdownPrivilege");

            // You can't shutdown without security privileges
            mcWin32.Scope.Options.EnablePrivileges = true;
            var mboShutdownParams = mcWin32.GetMethodParameters("Win32Shutdown");

            // Flag 1 means we want to shut down the system
            mboShutdownParams["Flags"] = "6";
            mboShutdownParams["Reserved"] = "0";

            foreach (ManagementObject manObj in mcWin32.GetInstances())
            {
                try
                {
                    manObj.InvokeMethod("Win32Shutdown",
                                        mboShutdownParams, null);
                }
                catch (ManagementException mex)
                {
                    Console.WriteLine(mex.ToString());
                    Console.ReadKey();
                }
            }
        }


    }


    public sealed class TokenAdjuster
    {
        // PInvoke stuff required to set/enable security privileges
        [DllImport("advapi32", SetLastError = true),
        SuppressUnmanagedCodeSecurity]
        static extern int OpenProcessToken(
        IntPtr processHandle, // handle to process
        int desiredAccess, // desired access to process
        ref IntPtr tokenHandle // handle to open access token
        );

        [DllImport("kernel32", SetLastError = true),
        SuppressUnmanagedCodeSecurity]
        static extern bool CloseHandle(IntPtr handle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int AdjustTokenPrivileges(
        IntPtr tokenHandle,
        int disableAllPrivileges,
        IntPtr newState,
        int bufferLength,
        IntPtr previousState,
        ref int returnLength);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool LookupPrivilegeValue(
        string lpSystemName,
        string lpName,
        ref Luid lpLuid);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Luid
        {
            internal int LowPart;
            internal int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TokenPrivileges
        {
            internal int PrivilegeCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            internal int[] Privileges;
        }
        const int SePrivilegeEnabled = 0x00000002;
        const int TokenAdjustPrivileges = 0X00000020;
        const int TokenQuery = 0X00000008;

        public static bool EnablePrivilege(string lpszPrivilege, bool
        bEnablePrivilege)
        {
            var retval = false;
            var ltkpOld = 0;
            var hToken = IntPtr.Zero;
            var tkp = new TokenPrivileges {Privileges = new int[3]};
            new TokenPrivileges {Privileges = new int[3]};
            var tLuid = new Luid();
            tkp.PrivilegeCount = 1;
            if (bEnablePrivilege)
                tkp.Privileges[2] = SePrivilegeEnabled;
            else
                tkp.Privileges[2] = 0;
            if (LookupPrivilegeValue(null, lpszPrivilege, ref tLuid))
            {
                var proc = Process.GetCurrentProcess();
                if (proc.Handle != IntPtr.Zero)
                {
                    if (OpenProcessToken(proc.Handle, TokenAdjustPrivileges | TokenQuery,
                    ref hToken) != 0)
                    {
                        tkp.PrivilegeCount = 1;
                        tkp.Privileges[2] = SePrivilegeEnabled;
                        tkp.Privileges[1] = tLuid.HighPart;
                        tkp.Privileges[0] = tLuid.LowPart;
                        const int bufLength = 256;
                        IntPtr tu = Marshal.AllocHGlobal(bufLength);
                        Marshal.StructureToPtr(tkp, tu, true);
                        if (AdjustTokenPrivileges(hToken, 0, tu, bufLength, IntPtr.Zero, ref ltkpOld) != 0)
                        {
                            // successful AdjustTokenPrivileges doesn't mean privilege could be changed
                            if (Marshal.GetLastWin32Error() == 0)
                            {
                                retval = true; // Token changed
                            }
                        }
                        var tp = (TokenPrivileges)Marshal.PtrToStructure(tu,
                                                                typeof(TokenPrivileges));
                        Marshal.FreeHGlobal(tu);
                    }
                }
            }
            if (hToken != IntPtr.Zero)
            {
                CloseHandle(hToken);
            }
            return retval;
        }

    }
}
