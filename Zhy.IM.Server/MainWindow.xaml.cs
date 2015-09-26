using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Zhy.IM.Framework.Tcp;

namespace Zhy.IM.Server
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (IPAddress ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        this.cbxIP.Items.Add(ip);
                    }
                }
            }
            catch (Exception ex)
            {
                this.txtIPMsg.Text = ex.Message;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (!CheckInput())
            {
                return;
            }
            new TcpServer().Listen();
            (sender as Button).IsEnabled = false;
        }

        /// <summary>
        /// 检查输入的内容是否正确
        /// </summary>
        /// <returns></returns>
        private bool CheckInput()
        {
            if (string.IsNullOrEmpty(this.cbxIP.Text.Trim()))
            {
                this.txtIPMsg.Text = "IP地址不能为空";
                return false;
            }
            if (string.IsNullOrEmpty(this.txtPort.Text.Trim()))
            {
                this.txtPortMsg.Text = "端口不能为空";
                return false;
            }
            if (!Regex.IsMatch(this.txtPort.Text.Trim(), "^\\d+$"))
            {
                this.txtPortMsg.Text = "端口只能是整数";
                return false;
            }

            return true;
        }

        private void cbxIP_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtIPMsg.Text.Trim()))
            {
                return;
            }
            this.txtIPMsg.Text = string.Empty;
        }

        private void txtPort_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtPortMsg.Text.Trim()))
            {
                return;
            }
            this.txtPortMsg.Text = string.Empty;
        }

    }

}
