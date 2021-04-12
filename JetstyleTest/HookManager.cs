using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace JetstyleTest
{
    public partial class MainWindow
    {
        private static readonly LowLevelMouseProc mouseProcess = HookCallback;
        private static IntPtr hookId = IntPtr.Zero;
        private static int mouseMoveMessage = 0x0200;
        private const int WH_MOUSE_LL = 14;

        private delegate void MouseMoveHandler(Point point);
        private static event MouseMoveHandler MousePositionChanged;


        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process currentProcess = Process.GetCurrentProcess())
            {
                using (ProcessModule currentModule = currentProcess.MainModule)
                {
                    return SetWindowsHookEx(WH_MOUSE_LL, proc,
                        GetModuleHandle(currentModule.ModuleName), 0);
                }
            }
            
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 &&
                mouseMoveMessage == (int) wParam)
            {
                var hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                UnhookWindowsHookEx(hookId);
                MousePositionChanged.Invoke(new Point(hookStruct.pt.X, hookStruct.pt.Y));
                hookId = SetHook(mouseProcess);
            }

            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LocalPoint
        {
            public int X;
            public int Y;

            public static implicit operator Point(LocalPoint point)
            {
                return new Point(point.X, point.Y);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public LocalPoint pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }


        #region DllImport
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        #endregion
    }
}
