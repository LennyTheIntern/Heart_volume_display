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
using NAudio.CoreAudioApi;
using System.IO;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
//using System.Threading

namespace Heart_volume_display
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Timers.Timer aTimer;
        settings _settings;

       
       
        public MainWindow() // make a app start funtion for vitals and put it in there
        {
            this.DataContext = new Vitals();
            _settings = new settings();
            _settings.DataContext = this.DataContext;
            InitializeComponent();
            SetTimerA();
        }

        public void SetTimerA()
        {
            aTimer = new System.Timers.Timer(10);
            aTimer.Elapsed += ATimer_Elapsed;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }
        private void ATimer_Elapsed(object sender, ElapsedEventArgs e)
        {
           
            this.Dispatcher.Invoke(() =>
            { 
                ((Vitals)DataContext).Update_Content();
            });
        }

        private void test_Click(object sender, RoutedEventArgs e)
        {
            _settings.Show();

           
        }
    }
}
