using System;
using System.Runtime.InteropServices;

namespace PInvoke
{
    class PInvokeMessageBox {

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);
        static void MessageBoxHelloWorld(string[] args)
        {
            MessageBox(IntPtr.Zero, "Hello unmanaged code.", "Test!", 0);
        }

        //static void Main(string[] args)
        //{
        //    MessageBoxHelloWorld(args);
        //}
    }
}
