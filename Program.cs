using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Teste_GlobalHook{
    internal static class Program{
        private static string KeysPressed = "";
        [STAThread]

        static void Main(){
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());

            GlobalKeyboardHook keyboardHook = new GlobalKeyboardHook();
            keyboardHook.KeyDown += (sender, e) => {
                if (e.KeyCode == System.Windows.Forms.Keys.F1){
                    Console.WriteLine("Current KeyValues: " + KeysPressed);
                }else{
                    KeysPressed += e.KeyCode + ' ';
                    Console.WriteLine("Key Down: " + e.KeyCode);
                }
            };
            keyboardHook.KeyUp += (sender, e) => Console.WriteLine("Key Up: " + e.KeyCode);

            Application.Run();
        }
    }
}
public class GlobalKeyboardHook : IDisposable{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private static LowLevelKeyboardProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;
    private static GlobalKeyboardHook _instance;

    public event EventHandler<KeyEventArgs> KeyDown;
    public event EventHandler<KeyEventArgs> KeyUp;

    public GlobalKeyboardHook(){
        _hookID = SetHook(_proc);
        _instance = this;
    }

    public void Dispose()
    {
        UnhookWindowsHookEx(_hookID);
    }

    ~GlobalKeyboardHook(){
        UnhookWindowsHookEx(_hookID);
    }

    private static IntPtr SetHook(LowLevelKeyboardProc proc){
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule){
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam){
        if (nCode >= 0){
            int vkCode = Marshal.ReadInt32(lParam);
            if (wParam == (IntPtr)WM_KEYDOWN){
                _instance.KeyDown?.Invoke(_instance, new KeyEventArgs((Keys)vkCode));
            }
            else if (wParam == (IntPtr)WM_KEYUP){
                _instance.KeyUp?.Invoke(_instance, new KeyEventArgs((Keys)vkCode));
            }
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
}