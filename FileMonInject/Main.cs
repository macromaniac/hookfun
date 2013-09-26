using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using EasyHook;
namespace FileMonInject
{


    public class Main : EasyHook.IEntryPoint
    {
        FileMon.FileMonInterface Interface;
        LocalHook CreateFileHook;
        LocalHook CreateFileHook2;
        Stack<String> Queue = new Stack<String>();

        public Main(
            RemoteHooking.IContext InContext,
            String InChannelName)
        {
            // connect to host...

            Interface =
              RemoteHooking.IpcConnectClient<FileMon.FileMonInterface>(InChannelName);
        }

        public void Run(
            RemoteHooking.IContext InContext,
            String InChannelName)
        {
            // install hook...
            try
            {
                                CreateFileHook2 = LocalHook.Create(
                                    LocalHook.GetProcAddress("kernel32.dll", "LCMapStringW"),
                                    new DLCMapStringW(LCMapStringW_Hooked),
                                    this);
                CreateFileHook = LocalHook.Create(
                    LocalHook.GetProcAddress("gdi32.dll", "ExtTextOutW"),
                    new DExtTextOut(ExtTextOut_Hooked),
                    this);
                CreateFileHook.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
                 
                CreateFileHook2.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
            }
            catch (Exception ExtInfo)
            {
                Interface.ReportException(ExtInfo);

                return;
            }

            Interface.IsInstalled(RemoteHooking.GetCurrentProcessId());

            // wait for host process termination...
            try
            {
                while (true)
                {
                    Thread.Sleep(500);

                    // transmit newly monitored file accesses...
                    if (Queue.Count > 0)
                    {
                        String[] Package = null;

                        lock (Queue)
                        {
                            Package = Queue.ToArray();

                            Queue.Clear();
                        }

                        Interface.OnCreateFile(RemoteHooking.GetCurrentProcessId(), Package);
                    }
                    else
                        Interface.Ping();
                }
            }
            catch
            {
                // NET Remoting will raise an exception if host is unreachable
            }
        }

        
        [UnmanagedFunctionPointer(CallingConvention.StdCall,
            CharSet = CharSet.Unicode,
            SetLastError = true)]
        delegate int DLCMapStringW(
           UInt32 Locale,
            UInt32 dwMapFlags,
            String lpSrcStr,
            UInt32 cchSrc,
            UInt32 lpDestStr,
            int cchDest);

        // just use a P-Invoke implementation to get native API access
        // from C# (this step is not necessary for C++.NET)
        [DllImport("kernel32.dll",
            CharSet = CharSet.Unicode,
            SetLastError = true,
            CallingConvention = CallingConvention.StdCall)]
        static extern int LCMapStringW(
            UInt32 Locale,
            UInt32 dwMapFlags,
            String lpSrcStr,
            UInt32 cchSrc,
            UInt32 lpDestStr,
            int cchDest);

        // this is where we are intercepting all file accesses!
        static int LCMapStringW_Hooked(
           UInt32 Locale,
            UInt32 dwMapFlags,
            String lpSrcStr,
            UInt32 cchSrc,
            UInt32 lpDestStr,
            int cchDest)
        {
            try
            {
                Main This = (Main)HookRuntimeInfo.Callback;

                lock (This.Queue)
                {
                    if(lpSrcStr.Contains("coup") || lpSrcStr.Contains("crit"))
                        This.Queue.Push(lpSrcStr);
                }
            }
            catch
            {
            }

            // call original API...
            return LCMapStringW(
                Locale,
                dwMapFlags,
                lpSrcStr,
                cchSrc,
                lpDestStr,
                cchDest);
        }
    
         

        [UnmanagedFunctionPointer(CallingConvention.StdCall,
                   CharSet = CharSet.Unicode,
                   SetLastError = true)]
        delegate bool DExtTextOut(
            UInt32 hdc,
            int X,
            int Y,
            UInt32 fuOptions,
            UInt32 lprc,   //this is a const pointer actually
            String lpString,
            UInt32 cbCount,
            int lpDx    /*this is a const pointer actually */);

        // just use a P-Invoke implementation to get native API access
        // from C# (this step is not necessary for C++.NET)
        [DllImport("gdi32.dll",
            CharSet = CharSet.Unicode,
            SetLastError = true,
            CallingConvention = CallingConvention.StdCall)]
        static extern bool ExtTextOut(
            UInt32 hdc,
            int X,
            int Y,
            UInt32 fuOptions,
            UInt32 lprc,   //this is a const pointer actually
            String lpString,
            UInt32 cbCount,
            int lpDx    /*this is a const pointer actually */);

        // this is where we are intercepting all file accesses!
        static bool ExtTextOut_Hooked(
            UInt32 hdc,
            int X,
            int Y,
            UInt32 fuOptions,
            UInt32 lprc,   //this is a const pointer actually
            String lpString,
            UInt32 cbCount,
            int lpDx    /*this is a const pointer actually */)
        {

            try
            {
                Main This = (Main)HookRuntimeInfo.Callback;

                lock (This.Queue)
                {
                    if(lpString.Contains("crit"))
                        This.Queue.Push(lpString);
                }
            }
            catch
            {
            }

            // call original API...
            return ExtTextOut(
                hdc,
                X,
                Y,
                fuOptions,
                lprc,
                lpString,
                cbCount,
                lpDx);
        }
    }
}