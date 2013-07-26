using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
//using System.Text;

namespace AsyncFtpClient
{
    // ReSharper disable UnusedVariable

    internal class Program
    {
        private static void Main()
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 10;

            //const string fileContents = "This file was uploaded via F#";
            //var bytes = Encoding.UTF8.GetBytes(fileContents);

            var x = new AsyncFtp.CopyFiles();
            x.Notification += (s, e) => Console.WriteLine(e.Message);

            //var result1 = x.UploadFile("192.168.0.133", "LoopMonTest", "FSharpFileSync.txt", bytes);
            //var result2 = x.UploadFiles(15, "192.168.0.133", "LoopMonTest", "FSharpFileAsync", bytes);

            var dict = new ConcurrentDictionary<string, byte[]>();
            foreach (var fileName in Enumerable.Range(1, 12).Select(fileNumber => string.Format("file{0}.txt", fileNumber)))
            {
                dict[fileName] = null;
            }

            var listing = new List<string>
                {
                    "file3.txt",
                    "file4.txt"
                };

            x.CopyFiles(
                dict,
                listing,
                "192.168.0.133", "LoopMonTest/Src",
                "192.168.0.133", "LoopMonTest/Dst");
        }
    }
}
