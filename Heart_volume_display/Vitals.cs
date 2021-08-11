using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Timers;
using NAudio;
using NAudio.CoreAudioApi;
using System.Windows.Threading;
using System.IO.Ports;
using System.IO;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;


namespace Heart_volume_display
{
    public class Vitals : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private SerialPort port;

        int Buffersize = 2000;
        List<double> heartRateList;
        double prev_avg = 0;
        double prev_sdv = 0;
        int calm_ticks = 0;

        public string HeartRateNumber { get; set; }
        public IList<string> PointsBuffer { get; set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static System.Timers.Timer aTimer;
        public int Content { get; set; }
        public List<string> AudioDeviceLabels { get; set; }
        public List<string> PortLabels { get; set; }
        public int AudioDeviceIndex { get; set; }
        public int PortIndex { get; set; }

        
        public MMDevice AudioDevice { get; set; }
        public MMDeviceCollection AudioDevices { get; set; }
        string data { get; set; }
        public PlotModel SerialModel { get; private set; }

        private ScarePopUp scarepopup = new ScarePopUp();
        public Vitals()
        {
            // no idea what content is
            Content = 0;

            //the should be renamed to HeartRate
            HeartRateNumber = "0";



            // adds all of the audio devices to the the settings menu
            var enumerator = new MMDeviceEnumerator();
            PointsBuffer = new List<string>();
            AudioDevices = enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active);
            AudioDeviceLabels = new List<string>();
            for (int i = 0; i < AudioDevices.Count; i++)
            {
                AudioDeviceLabels.Add(AudioDevices[i].ToString());
            }
            OnPropertyChanged("AudioDeviceLabels");


            // adds all the ports connected to the ports list
            var Ports = SerialPort.GetPortNames();
            PortLabels = new List<string>();
            foreach (string i in Ports)
            {
                PortLabels.Add(i);
            }


            //initiaizes the heart rate list
            heartRateList = new List<double>();

            // this should be done in a seperate class so you can easily cotom the pot
            SerialModel = new PlotModel();
            SerialModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, IsAxisVisible = false });
            SerialModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Minimum = 100, Maximum = 1000, IsAxisVisible = false });
            SerialModel.Series.Add(new LineSeries { LineStyle = LineStyle.Solid, StrokeThickness = 4, Color = OxyColor.Parse("#9effa9") });
            SerialModel.PlotAreaBorderColor = OxyColors.Transparent;

            //rename timer a to
            SetTimerA();
            SetTimerB();
            SetTimerShake();
        }

        public void Invalidate_settings()
        {
            port = new SerialPort(PortLabels[PortIndex], 115200, Parity.None, 8, StopBits.One); // resets the port
            if (port.IsOpen)
            {
                //port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
                //how do I use a timer for the 
                port.DataReceived -= new SerialDataReceivedEventHandler(port_DataReceived);
                port.Close();
                PointsBuffer.Clear();
            }

            data = null;

            //port = new SerialPort(PortLabels[PortIndex], 115200, Parity.None, 8, StopBits.One); // resets the port
            port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);

            try
            {
                port.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            data += port.ReadExisting();
        }

        private void Updata_points(object sender, ElapsedEventArgs e)
        {
            if (data != null)
            {
                data.Remove('\n');
                foreach (string i in data.Split('\r'))
                {
                    PointsBuffer.Add(i);
                }
                PointsBuffer.RemoveAt(PointsBuffer.Count - 1); // make sure the points buffer is not 0

                if (PointsBuffer.Count > Buffersize)
                {
                    lock (PointsBuffer)
                    {
                        var s = (LineSeries)SerialModel.Series[0];

                        s.Points.Clear();
                        int delta_x = 0;
                        foreach (string i in PointsBuffer.ToList())
                        {
                            if (i != null)
                            {
                                var ii = i.Split(',');

                                if (ii.Length == 3)
                                {
                                    // how to check for a format exeption before operation
                                    if (ii[0] != "")
                                    {
                                        try
                                        {
                                            
                                            lock (heartRateList)
                                            {
                                                
                                                heartRateList.Add(double.Parse(ii[0]));
                                            }
                                        }
                                        catch
                                        {

                                        }
                                    }

                                    double validationBufor;

                                    if (double.TryParse(ii[2], out validationBufor))
                                    {
                                        s.Points.Add(new DataPoint(delta_x, double.Parse(ii[2])));//TODO: check if they are in the right format
                                        delta_x += 10;
                                    }
                                }
                            }
                        }

                    }
                    // empty data
                    data = null;
                    SerialModel.InvalidatePlot(true);


                    // send the list of heart rates to the state machine 
                    lock (heartRateList)
                    {
                        intput_state(); // when this is a ref then it breaks even when you lock it
                    }
                    // pop the front of the points list to make i appear like the hear wave is moving

                    if (PointsBuffer.Count > Buffersize - 1)
                    {
                        lock (PointsBuffer)
                        {
                            for (int i = 0; i < (PointsBuffer.Count >> 4); i++)
                            {
                                PointsBuffer.RemoveAt(0);
                            }
                        }
                    }

                }
            }
        }



        public void force()
        {
            while(true)
            {
                Update_Content();
            }
        }


        // this is the for updataing the points
        public void SetTimerA()
        {
            aTimer = new System.Timers.Timer(10);
            aTimer.Elapsed += Updata_points;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        //this should be renamed to Update VolumeValue
        public void Update_Content()
        {
            var temp = AudioDevices[AudioDeviceIndex].AudioMeterInformation.MasterPeakValue * 100;    
            Content = (int)temp;
            OnPropertyChanged("Content");
        }



        // this is a timer for checking the states and outputng constantly
        System.Timers.Timer state_output_timer;
        public void SetTimerB()
        {
            state_output_timer = new System.Timers.Timer(1000);
            state_output_timer.Elapsed += state_timer_elapsed;
            state_output_timer.AutoReset = true;
            state_output_timer.Enabled = true;
            timer_start = Environment.TickCount & Int32.MaxValue;
        }


        //rename thi to "startMouseShake
        System.Timers.Timer shake_mouse_timer;
        public void SetTimerShake()
        {
            shake_mouse_timer = new System.Timers.Timer(100);
            shake_mouse_timer.Elapsed += rand_mouse_move;
            shake_mouse_timer.AutoReset = true;
            shake_mouse_timer.Enabled = false;
        }


        // move all of the state machine shit to another object
        enum State
        {
            start,
            normal,
            calm,
            stressed,
            scared
        }


        // this mess was the result of that debug mess
        //refactor this to take in a list of double

        //used for changing the state parameters
        State state = State.start;
        double stdv = 0;
        double avg = 0;
        void intput_state()
        {
          
            if (state == State.start)
                {
                    if (heartRateList.Count() > 0)
                    {
                        avg = heartRateList.Average();
                        double sum = heartRateList.Sum(d => Math.Pow(d - avg, 2));

                        stdv = Math.Sqrt((sum) / (heartRateList.Count() - 1));
                        state = State.normal;
                    }
                }
                else if (heartRateList.Count != 0)
            {
                    var temp = heartRateList.Average(); // check for no elements

                    if (temp > avg + 2)
                    {
                    avg = temp;
                    double sum = heartRateList.Sum(d => Math.Pow(d - avg, 2));

                        stdv = Math.Sqrt((sum) / (heartRateList.Count() - 1));
                        add_state();
                    }
                    else if (temp < avg - 2) // make sure it is 
                    {
                    avg = temp;
                    double sum = heartRateList.Sum(d => Math.Pow(d - avg, 2));
                    
                    stdv = Math.Sqrt((sum) / (heartRateList.Count() - 1));
                    drop_state();
                    }
                    else
                    {
                
                    }
                    
                }
        }


        // this is not a state machine so stop trying to make it one
        int timer_thresh = 4000;
        int timer_start = 0;
        private void state_timer_elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine(Environment.TickCount);


            if (int.Parse(HeartRateNumber) >= 50 && int.Parse(HeartRateNumber) <= 110)
            {
                lock (HeartRateNumber)
                {
                    //FileStream dumpfile = new FileStream("Metrics.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    //if (dumpfile.CanWrite)
                    //{
                    //    byte[] info = Encoding.ASCII.GetBytes(HeartRateNumber);
                    //    dumpfile.w(info, 0, info.Length);
                    //}
                    //dumpfile.Flush();
                    //dumpfile.Close();

                    StreamWriter sw;
                    sw = File.AppendText("Metrics.csv");
                    sw.WriteLine(HeartRateNumber);
                    sw.Close();
                    //File.AppendText("Metics.txt", HeartRateNumber);
                }
            }

            lock (heartRateList)
            {
                if (heartRateList.Count() > 1)
                {
                    HeartRateNumber =Math.Truncate( heartRateList.Average()).ToString();
                    OnPropertyChanged("HeartRateNumber");
                
                }
                intput_state(); // when this is a ref then it breaks even when you lock it
                heartRateList.Clear();
            }

            switch (state)
            {

                case State.normal:
                   
                    Console.WriteLine("normal");
                    if (((Environment.TickCount & Int32.MaxValue) - timer_start) > timer_thresh)
                    {
                        calm_state_unlocked = true;
                        drop_state();
                    }
                    break;
                case State.scared:
                    // output curso
                    Console.WriteLine("scared");
                    
                    if (((Environment.TickCount & Int32.MaxValue) - timer_start) > timer_thresh)
                    {
                        drop_state();
                    }

                    break;
                case State.calm:
                    Console.WriteLine("calm");
                    if (((Environment.TickCount & Int32.MaxValue) - timer_start) > timer_thresh)
                    {
                        //scarepopup.Dispatcher.Invoke(() =>
                        //{
                        //    scarepopup.ShowThenTerminate();
                        //}
                        //);
                        add_state();
                    }
                    
                    break;
                default:
                    break;
            }
        }


        Random rnd = new Random();
        private void rand_mouse_move(object sender, ElapsedEventArgs e)
        {
            InputSender.SendMouseInput(new InputSender.MouseInput[]
            {
                new InputSender.MouseInput
                {
                    dx = rnd.Next(-10, 10) ,
                    dy = rnd.Next(-10, 10) ,
                    dwFlags = (uint)(InputSender.MouseEventF.Move),
                }
            });
        }

        bool calm_state_unlocked = false;
        private void drop_state() // negitive transition
        {
            switch (state)
            {
                case State.normal:
                    if (calm_state_unlocked)
                    {
                        timer_start = Environment.TickCount & Int32.MaxValue;
                        timer_thresh = 4000;
                        state = State.calm;
                    }
                    break;
                case State.scared:
                    shake_mouse_timer.Stop();
                    timer_start = Environment.TickCount & Int32.MaxValue;
                    timer_thresh = 4000;
                    state = State.normal;
                    break;
                case State.calm:
                    //do nothign
                    break;
                default:
                    break;
            }
        }



        private void add_state() // this is the positive transition
        {
            switch (state)
            {
                case State.normal:
                    state = State.scared;
                    shake_mouse_timer.Start();
                    timer_start = Environment.TickCount & Int32.MaxValue;
                    timer_thresh = 4000;
                    break;
                case State.scared:
                    // do nothign
                    break;
                case State.calm:
                    state = State.normal;
                    calm_state_unlocked = false;
                    timer_start = Environment.TickCount & Int32.MaxValue;
                    timer_thresh = 4000;
                    break;
                default:
                    break;
            }
        }
    }
}
