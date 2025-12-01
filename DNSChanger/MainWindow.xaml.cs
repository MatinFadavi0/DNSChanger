using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            DnsList.Items.Add("8.8.8.8, 8.8.4.4  (Google)");
            DnsList.Items.Add("1.1.1.1, 1.0.0.1  (Cloudflare)");
            DnsList.Items.Add("9.9.9.9, 149.112.112.112  (Quad9)");
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

            PrimaryBox.Text = "";
            SecondaryBox.Text = "";
        }

        private void ApplyDns_Click(object sender, RoutedEventArgs e)
        {
            if (DnsList.SelectedItem == null)
            {
                MessageBox.Show("Pick a DNS first.");
                return;
            }

            string item = DnsList.SelectedItem.ToString();
            var parts = item.Split(',');

            string primary = parts[0].Trim();
            string secondary = parts[1].Trim().Split(' ')[0];

            RunCmd($"netsh interface ip set dns name=\"Ethernet\" static {primary}");
            RunCmd($"netsh interface ip add dns name=\"Ethernet\" {secondary} index=2");

            MessageBox.Show("DNS applied.");
        }

        private void RunCmd(string cmd)
        {
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c " + cmd);
            psi.Verb = "runas";
            psi.CreateNoWindow = true;
            psi.UseShellExecute = true;
            Process.Start(psi);
        }
    }
}
