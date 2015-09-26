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
        private string _fileName;

        public FileControl(string fileName)
        {
            InitializeComponent();

            this._fileName = fileName;
            this.txtFileName.Text = _fileName;
        }

        private void UserControl_Loaded_1(object sender, RoutedEventArgs e)
        {
            TcpClient.CreateInstance().PrintSendingFileInfo += PrintSendingFileInfo;
            TcpClient.CreateInstance().PrintSendingProgress += PrintSendingProgress;
            TcpClient.CreateInstance().PrintSendedResult += PrintSendedResult;
            TcpClient.CreateInstance().SendFile(this._fileName);
        }

        private void PrintSendingFileInfo(string fileName, double length, double convertedLength, Capacity flag)
        {
            this.txtProgress.Dispatcher.Invoke(() => this.txtProgress.Text = string.Format("({0}{1})", convertedLength.ToString("F2"), flag.ToString()));
            this.pbProgress.Dispatcher.Invoke(() => this.pbProgress.Maximum = length);
        }

        private void PrintSendingProgress(string fileName, double convertedLength, double currentLength, double convertedCurrentLength, Capacity flag)
        {
            this.txtProgress.Dispatcher.Invoke(() => this.txtProgress.Text = string.Format("({0}/{1}{2})", convertedLength.ToString("F2"), convertedCurrentLength.ToString("F2"), flag.ToString()));
            this.pbProgress.Dispatcher.Invoke(() => this.pbProgress.Value = currentLength);
        }

        private void PrintSendedResult(string result)
        {
            this.txtProgress.Dispatcher.Invoke(() => this.txtProgress.Text = result);
        }
    }
}
