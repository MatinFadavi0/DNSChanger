using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Windows;

namespace DnsChanger
{
    public partial class MainWindow : Window
    {
        private string savePath = "saved_dns.json";

        public MainWindow()
        {
            InitializeComponent();
            LoadDefaultDns();
            LoadSavedDns();

        }

        private void LoadDefaultDns()
        {
            DnsList.Items.Add("8.8.8.8, 8.8.4.4 (Google)");
            DnsList.Items.Add("1.1.1.1, 1.0.0.1 (Cloudflare)");
            DnsList.Items.Add("9.9.9.9, 149.112.112.112 (Quad9)");
        }

        private void LoadSavedDns()
        {
            if (!File.Exists(savePath))
                return;

            var items = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(savePath));

            foreach (var dns in items)
                DnsList.Items.Add(dns);
        }

        private void SaveDns()
        {
            var list = new List<string>();

            foreach (var item in DnsList.Items)
            {
                string s = item.ToString();

                if (!s.Contains("(Google)") &&
                    !s.Contains("(Cloudflare)") &&
                    !s.Contains("(Quad9)"))
                {
                    list.Add(s);
                }
            }

            File.WriteAllText(savePath, JsonSerializer.Serialize(list));
        }

        private void AddDns_Click(object sender, RoutedEventArgs e)
        {
            string p = PrimaryBox.Text.Trim();
            string s = SecondaryBox.Text.Trim();

            if (p.Length == 0 || s.Length == 0)
            {
                MessageBox.Show("Fill both fields.");
                return;
            }

            string entry = $"{p}, {s}";
            DnsList.Items.Add(entry);

            SaveDns();

            PrimaryBox.Clear();
            SecondaryBox.Clear();
        }

        public static string GetActiveInterface()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var ni in interfaces)
            {
                var props = ni.GetIPProperties();

                bool hasGateway =
                    props.GatewayAddresses.Any(g => g.Address.ToString().Length > 0);

                if (ni.OperationalStatus == OperationalStatus.Up && hasGateway)
                {
                    return ni.Name; 
                }
            }

            return null;
        }


        private void ApplyDns_Click(object sender, RoutedEventArgs e)
        {
            if (DnsList.SelectedItem == null)
            {
                MessageBox.Show("Pick a DNS first.");
                return;
            }

            string iface = GetActiveInterface();
            if (iface == null)
            {
                MessageBox.Show("No active network interface found.");
                return;
            }

            string item = DnsList.SelectedItem.ToString();
            var parts = item.Split(',');
            string primary = parts[0].Trim().Split(' ')[0];   
            string secondary = parts[1].Trim().Split(' ')[0]; 

            if (!RunCmd($"netsh interface ip set dns name=\"{iface}\" static {primary}")) return;
            RunCmd($"netsh interface ip add dns name=\"{iface}\" {secondary} index=2");

            MessageBox.Show($"DNS applied to: {iface}");
        }

        private bool RunCmd(string cmd)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c " + cmd)
                {
                    Verb = "runas",
                    CreateNoWindow = true,
                    UseShellExecute = true
                };
                var proc = Process.Start(psi);
                proc.WaitForExit(); 
                return proc.ExitCode == 0;
            }
            catch
            {
                MessageBox.Show("Failed to run command. Make sure you run the app as Administrator.");
                return false;
            }
        }

    }
}
