using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Text;
using EasyHook;
using System.Runtime.InteropServices;
using System.Diagnostics;
namespace FileMon
{
    public class FileMonInterface : MarshalByRefObject
    {
        public void IsInstalled(Int32 InClientPID)
        {
            Console.WriteLine("FileMon has been installed in target {0}.\r\n", InClientPID);
        }

        public void OnCreateFile(Int32 InClientPID, String[] InFileNames)
        {
            for (int i = 0; i < InFileNames.Length; i++)
            {
                Console.WriteLine(InFileNames[i]);
            }
        }

        public void ReportException(Exception InInfo)
        {
            Console.WriteLine("The target process has reported" +
                              " an error:\r\n" + InInfo.ToString());
        }

        public void Ping()
        {
        }
    }
    class Program
    {
        static String ChannelName = null;
        static int pid=0;
        static void Main(string[] args)
        {
            try
            {
                Config.Register(
                        "A FileMon like demo application.",
                        "FileMon.exe",
                        "FileMonInject.dll");

                RemoteHooking.IpcCreateServer<FileMonInterface>(
                     ref ChannelName, WellKnownObjectMode.SingleCall);

                //Process p = Process.Start("C:\\Program Files\\Microsoft Games\\Solitaire\\Solitaire.exe");
                Process p = Process.Start("C:\\Program Files (x86)\\Notepad++\\notepad++.exe");
                
                RemoteHooking.Inject(
                    p.Id,
                    "FileMonInject.dll",
                    "FileMonInject.dll",
                    ChannelName);
                //RemoteHooking.CreateAndInject("C:\\Program Files\\Microsoft Games\\Solitaire\\Solitaire.exe","",0,"FileMonInject.dll", "FileMonInject.dll", out pid,ChannelName);
                //RemoteHooking.CreateAndInject("C:\\Program Files (x86)\\Notepad++\\Notepad++.exe","",0,"FileMonInject.dll", "FileMonInject.dll", out pid,ChannelName);
                //RemoteHooking.CreateAndInject("C:\\Program Files (x86)\\Audacity\\audacity.exe", "", 0, "FileMonInject.dll", "FileMonInject.dll", out pid, ChannelName);

                Console.ReadLine();
            }
            catch (Exception ExtInfo)
            {
                Console.WriteLine("There was an error while connecting " +
                                  "to target:\r\n{0}", ExtInfo.ToString());
            }
            Console.ReadLine();
        }
    }
}