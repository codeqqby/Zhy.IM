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
using System.Windows.Shapes;
using Zhy.IM.Controls;
using Zhy.IM.Framework.Tcp;


namespace Zhy.IM
{
    /// <summary>
    /// ChatWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ChatWindow : BaseWindow
    {
        public ChatWindow()
        {
            InitializeComponent();
        }

        //private void BaseWindow_Drop_1(object sender, DragEventArgs e)
        //{
        //    string msg = "Drop";
        //    if (e.Data.GetDataPresent(DataFormats.FileDrop))
        //    {
        //        msg = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
        //    }

        //    OpenFileDialog ofd = new OpenFileDialog();
        //    if (!ofd.ShowDialog().Value)
        //    {
        //        btn.IsEnabled = true;
        //        return;
        //    }
        //    TcpClient.CreateInstance().PrintSendingFileInfo += PrintSendingFileInfo;
        //    TcpClient.CreateInstance().PrintSendingProgress += PrintSendingProgress;
        //    TcpClient.CreateInstance().PrintSendedResult += PrintSendedResult;
        //    TcpClient.CreateInstance().SendFile(ofd.FileName);

        //    MessageBox.Show(msg);
        //}

        //private void PrintSendingFileInfo(string fileName, double length, double convertedLength, Capacity flag)
        //{
        //    this.txtInfo.Dispatcher.Invoke(() => this.txtInfo.Text = string.Format("{0}({1}{2})", fileName, convertedLength.ToString("F2"), flag.ToString()));
        //    this.pbProgress.Dispatcher.Invoke(() => this.pbProgress.Maximum = length);
        //}

        //private void PrintSendingProgress(string fileName, double convertedLength, double currentLength, double convertedCurrentLength, Capacity flag)
        //{
        //    this.txtInfo.Dispatcher.Invoke(() => this.txtInfo.Text = string.Format("{0}({1}/{2}{3})", fileName, convertedLength.ToString("F2"), convertedCurrentLength.ToString("F2"), flag.ToString()));
        //    this.pbProgress.Dispatcher.Invoke(() => this.pbProgress.Value = currentLength);
        //}

        //private void PrintSendedResult(string result)
        //{
        //    this.txtInfo.Dispatcher.Invoke(() => this.txtInfo.Text = result);
        //}
    }
}
