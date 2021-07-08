using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Axes;

namespace Heart_volume_display
{
    class CustomPlotModal
    {
        public PlotModel CostomView()
        {
            var plotmodel = new PlotModel();
            plotmodel.PlotAreaBorderThickness = new OxyThickness(0);
            plotmodel.PlotMargins = new OxyThickness(10);
            LinearAxis linearAxis = new LinearAxis();
            linearAxis.Maximum = 40;
            linearAxis.Minimum = -40;
            linearAxis.PositionAtZeroCrossing = true;
            linearAxis.TickStyle = OxyPlot.Axes.TickStyle.None;

            linearAxis.AxislineColor = OxyColor.Parse("Tansperant");
            linearAxis.TicklineColor = OxyColor.Parse("Tansperant");
            
            plotmodel.Axes.Add(linearAxis);

            var secondLinearAxis = new LinearAxis();
            secondLinearAxis.Maximum = 40;
            secondLinearAxis.Minimum = -40;
            secondLinearAxis.PositionAtZeroCrossing = true;
            secondLinearAxis.TickStyle = OxyPlot.Axes.TickStyle.None;
            secondLinearAxis.Position = AxisPosition.Bottom;
            secondLinearAxis.AxislineColor = OxyColor.Parse("#FE6584"); // 
            secondLinearAxis.TicklineColor = OxyColor.Parse("#FE6584");
            plotmodel.Axes.Add(secondLinearAxis);

            return plotmodel;
        }

    }
}
