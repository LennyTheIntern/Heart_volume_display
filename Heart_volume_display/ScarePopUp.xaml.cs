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
using System.Timers;

namespace Heart_volume_display
{
    /// <summary>
    /// Interaction logic for ScarePopUp.xaml
    /// </summary>
    public partial class ScarePopUp : Window
    {
        Timer self_close_timer;
        public ScarePopUp() // this window should be self closing
        {
            

            InitializeComponent();
            self_close_timer = new System.Timers.Timer(1000);
            self_close_timer.Elapsed += auto_close;
            self_close_timer.AutoReset = true;
            self_close_timer.Enabled = false;
            //this.Show();
          

        }

        public void ShowThenTerminate()
        {
            this.Show();
            self_close_timer.Start();
        }
        private void auto_close(object sender, ElapsedEventArgs e)
        {
            self_close_timer.Stop();

            Dispatcher.Invoke(() =>
            {
                this.Hide();
            }
                );
        }
    }
}
