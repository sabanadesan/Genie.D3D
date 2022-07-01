using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
namespace D3D
{
    public class Log
    {
        private String path = "";

        private StreamWriter file;

        public Log(String fileName = "log.txt")
        {
            String name = String.Format("{0:yyyy_MM_dd}_{1}", DateTime.Now, "log.txt");

            var filePath = Path.Combine(path, fileName);

            //file = new StreamWriter("C:\\" + filePath);
        }

        public void WriteToConsole(String msg)
        {
            Debug.WriteLine(msg);
            System.Console.WriteLine(msg);
        }

        public void WriteToFile(String msg)
        {
            file.WriteLine(msg);
        }
    }
}
