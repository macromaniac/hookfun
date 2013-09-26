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
                if (i > 0)
                {
                    //get rid of repeats
                    //if( !InFileNames[i].Equals(InFileNames[i-1]) )
                        Console.WriteLine(InFileNames[i]);
                }
                else
                {
                    Console.WriteLine(InFileNames[i]);
                }
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
        static int pid=2420;
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

                //Process p = Process.Start("C:\\Program Files (x86)\\Audacity\\audacity.exe");

                //Process p = Process.Start("C:\\Program Files (x86)\\Steam\\steamapps\\common\\dota 2 beta\\dota.exe","-console");
                Process p = Process.Start("C:\\Program Files (x86)\\Steam\\Steam.exe", "-applaunch 570");

                
                System.Threading.Thread.Sleep(5000);

                //This is really sloppy, I use window titles to find the pid because launching via steam doesn't return the pid
                var processes = Process.GetProcesses();
                foreach (var process in processes)
                {
                    var Wintitle = process.MainWindowTitle;
                    if(Wintitle.ToString().ToUpper() == "DOTA 2")
                        pid = process.Id;
                }

                RemoteHooking.Inject(
                    pid,
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