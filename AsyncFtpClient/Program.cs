using System.Text;

namespace AsyncFtpClient
{
    // ReSharper disable UnusedVariable

    internal class Program
    {
        private static void Main()
        {
            const string fileContents = "This file was uploaded via F#";
            var bytes = Encoding.UTF8.GetBytes(fileContents);

            var x = new AsyncFtp.CopyFiles();
            var result1 = x.UploadFile("192.168.0.133", "LoopMonTest", "FSharpFileSync.txt", bytes);
            var result2 = x.UploadFiles(15, "192.168.0.133", "LoopMonTest", "FSharpFileAsync", bytes);
        }
    }
}
