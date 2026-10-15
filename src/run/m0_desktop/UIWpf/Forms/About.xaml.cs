using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace m0.UIWpf.Forms
{
    /// <summary>
    /// Logika interakcji dla klasy About.xaml
    /// </summary>
    public partial class About : Window
    {
        AboutLogoScene _logo;

        public About(Window owner)
        {
            this.Owner = owner;
            InitializeComponent();
            _logo = new AboutLogoScene();
            _logo.Attach(LogoView);
            MouseMove += About_MouseMove;
            MouseLeave += About_MouseLeave;
            Closed += About_Closed;

            ShowDialog();
        }

        void About_MouseMove(object sender, MouseEventArgs e)
        {
            if (ActualWidth <= 0 || ActualHeight <= 0)
                return;

            Point p = e.GetPosition(this);
            _logo.SetPointer(p.X / ActualWidth, p.Y / ActualHeight);
        }

        void About_MouseLeave(object sender, MouseEventArgs e)
        {
            _logo.SetPointer(0.5, 0.34);
        }

        void About_Closed(object sender, EventArgs e)
        {
            _logo.Dispose();
        }

        private void WebsiteHyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true
            });
            e.Handled = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
