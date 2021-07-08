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
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Heart_volume_display
{
    public class Vitals : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private SerialPort port;//= new SerialPort("COM8",115200, Parity.None, 8, StopBits.One); // add port select

        int Buffersize = 2000;
        public double HeartRateNumber { get; set; }
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
      

        public Vitals()
        {
            Content = 0;
            HeartRateNumber = 0;
            var enumerator = new MMDeviceEnumerator();
            PointsBuffer = new List<string>();
            AudioDevices = enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active);
            AudioDeviceLabels = new List<string>();
            for (int i = 0 ; i < AudioDevices.Count; i++)
            {
                AudioDeviceLabels.Add(AudioDevices[i].ToString());
            }
            OnPropertyChanged("AudioDeviceLabels");

            var Ports = SerialPort.GetPortNames();
            PortLabels = new List<string>();
            foreach (string i in Ports)
            {
                PortLabels.Add(i);
            }


            SerialModel = new PlotModel();
            SerialModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, IsAxisVisible = false });
            SerialModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Minimum = 100, Maximum = 1000, IsAxisVisible = false });   
            SerialModel.Series.Add(new LineSeries { LineStyle = LineStyle.Solid, StrokeThickness = 4, Color = OxyColor.Parse("#9effa9") });
            SerialModel.PlotAreaBorderColor = OxyColors.Transparent;
            SetTimerA();
        }

        public void Invalidate_settings()
        {
            port = new SerialPort(PortLabels[PortIndex], 115200, Parity.None, 8, StopBits.One); // resets the port
            if (port.IsOpen)
            {
                //port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
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
            //if (data != null)
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
                PointsBuffer.RemoveAt(PointsBuffer.Count - 1);
                
                if (PointsBuffer.Count > Buffersize)
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
                                        HeartRateNumber = double.Parse(ii[0]);
                                    }
                                    catch
                                    {

                                    }
                                }

                                s.Points.Add(new DataPoint(delta_x, double.Parse(ii[2])));
                                delta_x += 10;
                            }
                        }
                        

                    }
                    data = null;
                    SerialModel.InvalidatePlot(true);
                    OnPropertyChanged("HeartRateNumber");
                    if (PointsBuffer.Count > Buffersize - 1)
                    {
                        for (int i = 0; i < (PointsBuffer.Count >> 4); i++)
                        {
                            PointsBuffer.RemoveAt(0);
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

        public void SetTimerA()
        {
            aTimer = new System.Timers.Timer(10);
            aTimer.Elapsed += Updata_points;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        public void Update_Content()
        {
            var temp = AudioDevices[AudioDeviceIndex].AudioMeterInformation.MasterPeakValue * 100;    
            Content = (int)temp;
            OnPropertyChanged("Content");
        }

      

    }
}
