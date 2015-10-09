using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Zhy.IM.Framework.Tcp;

namespace Zhy.IM.Plugin.UserList
{
    /// <summary>
    /// FileControl.xaml 的交互逻辑
    /// </summary>
    public partial class FileControl : UserControl
    {
        private TcpFile _tcpFile;

        public FileControl(string fileName)
        {
            InitializeComponent();

            this.txtFileName.Text = System.IO.Path.GetFileName(fileName);
            this._tcpFile = new TcpFile() { FileName = fileName };
            this.DataContext = this._tcpFile;
        }

        private void UserControl_Loaded_1(object sender, RoutedEventArgs e)
        {
            TcpClient.CreateInstance().SendFile(this._tcpFile);
        }
    }
}
