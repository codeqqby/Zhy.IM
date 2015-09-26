using Microsoft.Win32;
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


namespace Zhy.IM.Plugin.UserList
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

        private void BaseWindow_Drop_1(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string fileName = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
                //OpenFileDialog ofd = new OpenFileDialog();
                
                //if (!ofd.ShowDialog().Value)
                //{
                //    btn.IsEnabled = true;
                //    return;
                //}
                SendFile(((Array)e.Data.GetData(DataFormats.FileDrop)).Cast<string>().ToArray());
            }
        }

        private void SendFile(string[] files)
        {
            for (int i = 0; i < files.Length; i++)
            {
                FileControl fc = new FileControl(files[i]);
                this.spRight.Children.Add(fc);
            }

            this.spRight.Visibility = Visibility.Collapsed;
        }


    }
}
