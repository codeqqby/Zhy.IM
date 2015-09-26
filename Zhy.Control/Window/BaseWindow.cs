using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Zhy.Control.Window
{
    public class BaseWindow : System.Windows.Window
    {
        public BaseWindow()
        {
            InitializeTheme();
        }

        private void InitializeTheme()
        {
            ResourceDictionary dic = new ResourceDictionary() { Source = new Uri("pack://application:,,,/Zhy.SkinShow/lvshangzhitou.xaml") };
            Application.Current.Resources.MergedDictionaries.Add(dic);
            this.Style = (Style)Application.Current.Resources["WindowStyle"];
        }
    }
}
