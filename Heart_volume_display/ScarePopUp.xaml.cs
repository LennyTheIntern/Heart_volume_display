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
using System.IO;
using System.Media;


namespace Heart_volume_display
{
    /// <summary>
    /// Interaction logic for ScarePopUp.xaml
    /// </summary>
    public partial class ScarePopUp : Window
    {
        
        Timer self_close_timer;
        SoundPlayer soundPlayer;
        List<string> image_names;
        List<string> sound_names;
        public ScarePopUp() // this window should be self closing
        {
            InitializeComponent();
            self_close_timer = new System.Timers.Timer(2000);
            self_close_timer.Elapsed += auto_close;
            self_close_timer.AutoReset = true;
            self_close_timer.Enabled = false;
            image_names = new List<string>();
            sound_names = new List<string>();
            
            string strUri2 = Directory.GetParent(Directory.GetCurrentDirectory()).ToString();
           
            foreach (string file in Directory.EnumerateFiles(Directory.GetCurrentDirectory() + @"\imgs\","*.jpg"))
            {
                image_names.Add(file);
            }

            foreach (string file in Directory.EnumerateFiles(Directory.GetCurrentDirectory() + @"\sounds\","*.wav"))
            {
                sound_names.Add(file);
            }
        }

        public void ShowThenTerminate()
        {
            rand_select_image();
            this.Show();
            self_close_timer.Start();
        }
        private void auto_close(object sender, ElapsedEventArgs e)
        {
            self_close_timer.Stop();
           
            Dispatcher.Invoke(() =>
                    {
                        soundPlayer.Stop();
                        this.Hide();
                    }
                );
        }

        // somewhow get all th recourses in a list 
        Random rnd = new Random(); // seed the funtion
        private void rand_select_image()
        {
            // load all of the file names into a list
            //read the images from the debug file becuse i gues this a a normal thing to do
            rnd = new Random();

            if (image_names.Count > 0)
            {
                int i = rnd.Next(0, (image_names.Count()));// it never goes to three wtf
                ScareImg.Source = new BitmapImage(new Uri(image_names[i]));
            }

            /// sound player
            // path to .wav
            if (sound_names.Count > 0)
            {
                int j = rnd.Next(0, (sound_names.Count()));
                soundPlayer = new SoundPlayer(sound_names[j]); // load sound files to finish up 
                soundPlayer.Play();
            }
           
        }

        private void rand_select_sound()
        {
            // load all of the file names into a list
            //read the images from the debug file becuse i gues this a a normal thing to do
            rnd = new Random();
            if (sound_names.Count > 0)
            {
                int j = rnd.Next(0, (sound_names.Count()));
                soundPlayer = new SoundPlayer(sound_names[j]); // load sound files to finish up 
                soundPlayer.Play();
            }

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
