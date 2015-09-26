using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Xml.Linq;
using Zhy.IM.Controls;
using Zhy.IM.Framework.Tcp;
using Zhy.IM.Plugin.UserList;

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

        private void BaseWindow_Loaded_1(object sender, RoutedEventArgs e)
        {
            string fileName = Process.GetCurrentProcess().MainModule.FileName;
            fileName = System.IO.Path.GetFileName(fileName);
            fileName = string.Format("{0}.config", fileName);
            XDocument xml = XDocument.Load(fileName);
            IEnumerable<XElement> nodes = xml.Root.Element("appSettings").Elements();
            XElement node = nodes.SingleOrDefault(x => x.Attribute("key").Value == "serverip");
            IPAddress ip = IPAddress.Parse(node.Attribute("value").Value);
            node = nodes.SingleOrDefault(x => x.Attribute("key").Value == "port");
            int port = int.Parse(node.Attribute("value").Value);
            bool bln = TcpClient.CreateInstance().Connect(ip, port);
            if (!bln)
            {
                MessageBox.Show("error");
            }
            else
            {
                UserListPlugin plugin = new UserListPlugin();
                this.mainGrid.Children.Add(plugin);
            }
        }



    }
}
