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

namespace Zhy.IM.Controls
{
  
    public class BaseWindow : Window
    {
        //static BaseWindow()
        //{
        //    DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseWindow), new FrameworkPropertyMetadata(typeof(BaseWindow)));
        //}

        public BaseWindow()
        {
            InitializeTheme();
            InitEvent();
        }

        private void InitializeTheme()
        {
            this.Style = (Style)Application.Current.Resources["WindowStyle"];
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void InitEvent()
        {
            this.MouseMove += delegate(object sender, MouseEventArgs e)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    this.DragMove();
                }
            };
        }
    }
}
