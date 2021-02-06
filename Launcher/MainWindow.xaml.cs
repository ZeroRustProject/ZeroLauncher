using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Launcher
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WebClient net = new WebClient();
        List<string> toUpdate = new List<string>();
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                var th = new Thread(() =>
                {
                    var manifest = JsonConvert.DeserializeObject<Dictionary<string, string>>(net.DownloadString("http://azp.zeroproject.ru/game/manifest.json"));
                    foreach (var file in manifest)
                    {
                        var dir = Path.GetDirectoryName(file.Key);
                        if (!Directory.Exists(dir) && dir.Length > 0)
                        {
                            Directory.CreateDirectory(dir);
                        }
                        if (!File.Exists(file.Key))
                        {
                            toUpdate.Add(file.Key);
                        }
                        else
                        {
                            if (file.Value != "0")
                            {
                                if (MD5(file.Key) != file.Value)
                                {
                                    toUpdate.Add(file.Key);
                                }
                            }
                        }
                    }
                    if (toUpdate.Count > 0)
                    {
                        Task.Run(UpdateClient);
                    }
                    else
                    {
                        RunClient();
                    }
                });
                th.Start();
            } catch (Exception ex)
            {
                File.WriteAllText("Exception.log", ex.ToString());
            }
        }

        static readonly string[] SizeSuffixes =
                  { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        static string SizeSuffix(Int64 value, int decimalPlaces = 1)
        {
            if (value < 0) { return "-" + SizeSuffix(-value, decimalPlaces); }

            int i = 0;
            decimal dValue = (decimal)value;
            while (Math.Round(dValue, decimalPlaces) >= 1000)
            {
                dValue /= 1024;
                i++;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}", dValue, SizeSuffixes[i]);
        }

        async Task UpdateClient()
        {
            try
            {
                while (toUpdate.Count > 0)
                {
                    var file = toUpdate[0];
                    net.DownloadFileCompleted += (sender, f) =>
                    {
                        toUpdate.Remove(file);
                    };
                    net.DownloadProgressChanged += (sender, p) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            x1.Width = p.ProgressPercentage * 4;
                            x2.Content = $"{SizeSuffix(p.BytesReceived)} / {SizeSuffix(p.TotalBytesToReceive)} ({p.ProgressPercentage} %)";
                        });
                    };
                    Dispatcher.Invoke(() => { x0.Content = file; });
                    await net.DownloadFileTaskAsync(new Uri("http://azp.zeroproject.ru/game/" + file), file);
                }
                RunClient();
            } catch (Exception ex)
            {
                File.WriteAllText("Exception.log", ex.ToString());
            }
        }

        void RunClient()
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = "RustClient.exe",
                Arguments = "-show-screen-selector"
            });
            Environment.Exit(0);
        }

        string MD5(string s)
        {
            using (var provider = System.Security.Cryptography.MD5.Create())
            {
                using (var stream = File.OpenRead(s))
                {
                    var hash = provider.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Environment.Exit(0);
        }

        void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
