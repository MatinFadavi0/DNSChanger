using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        }



    }
}
