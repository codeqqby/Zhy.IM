using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Zhy.IM.Window
{
    public class BaseWindow : System.Windows.Window
    {
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
