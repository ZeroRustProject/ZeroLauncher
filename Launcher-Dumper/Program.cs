using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher_Dumper
{
    class Program
    {
        static void Main(string[] args)
        {
            int x = Environment.CurrentDirectory.Length + 1;
            string output = "[\n";
            foreach (string file in Directory.GetFiles(Environment.CurrentDirectory, "*", SearchOption.AllDirectories))
            {
                string filename = file.Remove(0, x);
                output += "\n\t\"" + filename + "\":\"" + MD5(File.ReadAllBytes(filename)) + "\",";
            }
            output += "\n]";
            File.WriteAllText("manifest.json", output);
        }

        public static string MD5(byte[] s)
        {
            using (var provider = System.Security.Cryptography.MD5.Create())
            {
                StringBuilder builder = new StringBuilder();

                foreach (byte b in provider.ComputeHash(s))
                    builder.Append(b.ToString("x2").ToLower());

                return builder.ToString();
            }
        }
    }
}
