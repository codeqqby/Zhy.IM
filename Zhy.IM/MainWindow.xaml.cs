using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Zhy.IM.Framework.Tcp;
using Zhy.IM.Window;

namespace Zhy.IM
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : BaseWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txt.Text))
            {
                MessageBox.Show("IP地址不能为空");
                return;
            }
            bool bln = TcpClient.CreateInstance().Connect(IPAddress.Parse(txt.Text), 8010);
            if (!bln)
            {
                MessageBox.Show("error");
                return;
            }
            (sender as Button).IsEnabled = false;
        }

        private void BaseWindow_Loaded_1(object sender, RoutedEventArgs e)
        {

        }



    }
}
