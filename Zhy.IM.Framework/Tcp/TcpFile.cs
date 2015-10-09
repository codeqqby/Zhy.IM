using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zhy.IM.Framework.Tcp
{
    public class TcpFile : INotifyPropertyChanged
    {
        private string fileName;
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName
        {
            get { return fileName; }
            set
            {
                fileName = value;
                OnPropertyChanged("FileName");
            }
        }

        private string progressText;
        /// <summary>
        /// 文件显示的当前进度
        /// </summary>
        public string ProgressText
        {
            get { return progressText; }
            set
            {
                progressText = value;
                OnPropertyChanged("ProgressText");
            }
        }

        private double progressValue;
        /// <summary>
        /// 进度条显示的当前进度
        /// </summary>
        public double ProgressValue
        {
            get { return progressValue; }
            set
            {
                progressValue = value;
                OnPropertyChanged("ProgressValue");
            }
        }

        private double progressMaximum;
        /// <summary>
        /// 进度条的最大进度值
        /// </summary>
        public double ProgressMaximum
        {
            get { return progressMaximum; }
            set 
            { 
                progressMaximum = value;
                OnPropertyChanged("ProgressMaximum");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
