using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace Heart_volume_display
{
    /// <summary>
    /// Interaction logic for settings.xaml
    /// </summary>
    public partial class settings : Window , INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public settings()
        {
            InitializeComponent();
            ComPortListBox.SelectedIndex = 0;
            AudioListBox.SelectedIndex = 0;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }


        private void ComPortListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((Vitals)DataContext).PortIndex = ComPortListBox.SelectedIndex;
            ((Vitals)DataContext).Invalidate_settings();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void AudioListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((Vitals)DataContext).AudioDeviceIndex = AudioListBox.SelectedIndex;
        }
    }
}
