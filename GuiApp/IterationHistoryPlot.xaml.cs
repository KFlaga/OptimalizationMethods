using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Qfe
{
    public partial class IterationHistoryPlot : UserControl
    {
        private IEnumerable<IterationResults> results;
        public IEnumerable<IterationResults> Results
        {
            get { return results; }
            set
            {
                results = value;
                if(results != null)
                {
                    update(results);
                }
            }
        }

        public bool ShowFunction { get { return buttonFunctionPlot.IsChecked.Value; } }
        public bool ShowCost { get { return buttonCostPlot.IsChecked.Value; } }
        public bool ShowFunctionChange { get { return buttonFunctionChangePlot.IsChecked.Value; } }
        public bool ShowPointChange { get { return buttonPointChangePlot.IsChecked.Value; } }

        private PlotModel model = new PlotModel();
        private LinearAxis axisIterations = new LinearAxis() { Position = AxisPosition.Bottom };
        private LinearAxis axisValues = new LinearAxis() { Position = AxisPosition.Left };

        private LineSeries seriesFunction = new LineSeries()
        {
            StrokeThickness = 1,
            Color = OxyColor.FromRgb(0, 0, 0),
            Title = "f(x)"
        };
        private LineSeries seriesCost = new LineSeries()
        {
            StrokeThickness = 1,
            Color = OxyColor.FromRgb(200, 0, 0),
            Title = "f(x) + penalty(x)"
        };
        private LineSeries seriesFuncionChange = new LineSeries()
        {
            StrokeThickness = 1,
            Color = OxyColor.FromRgb(0, 200, 0),
            Title = "|fk+1 - fk|"
        };
        private LineSeries seriesPointChange = new LineSeries()
        {
            StrokeThickness = 1,
            Color = OxyColor.FromRgb(0, 0, 200),
            Title = "|||xk+1 - xk||"
        };


        public IterationHistoryPlot()
        {
            this.DataContext = this;
            InitializeComponent();

            model.Axes.Add(axisIterations);
            model.Axes.Add(axisValues);

            model.Series.Add(seriesFunction);
            model.Series.Add(seriesCost);
            model.Series.Add(seriesFuncionChange);
            model.Series.Add(seriesPointChange);

            iterationsPlot.Model = model;
        }

        private void update(IEnumerable<IterationResults> iterations)
        {
            if (ShowFunction)
                updateIterationsPlot(seriesFunction, iterations, (i) => i.CurrentFunction);
            if (ShowCost)
                updateIterationsPlot(seriesCost, iterations, (i) => i.CurrentCost);
            if (ShowFunctionChange)
                updateIterationsPlot(seriesFuncionChange, iterations.Skip(1), (i) => i.LastFunuctionChange);
            if (ShowPointChange)
                updateIterationsPlot(seriesPointChange, iterations.Skip(1), (i) => i.LastPointChange);
        }

        private void updateIterationsPlot(LineSeries series, IEnumerable<IterationResults> iterations, Func<IterationResults, double> selector)
        {
            if(iterations != null)
            {
                series.Points.Clear();
                int index = iterations.First().Iteration;
                foreach (var iteration in iterations)
                {
                    series.Points.Add(new DataPoint(index, selector(iteration)));
                    ++index;
                }

                model.InvalidatePlot(true);
            }
        }

        private void addFunctionToGraph(object sender, RoutedEventArgs e)
        {
            updateIterationsPlot(seriesFunction, results, (i) => i.CurrentFunction);
        }

        private void removeFunctionFromGraph(object sender, RoutedEventArgs e)
        {
            seriesFunction.Points.Clear();
            model.InvalidatePlot(true);
        }

        private void addCostToGraph(object sender, RoutedEventArgs e)
        {
            updateIterationsPlot(seriesCost, results, (i) => i.CurrentCost);
        }

        private void removeCostFromGraph(object sender, RoutedEventArgs e)
        {
            seriesCost.Points.Clear();
            model.InvalidatePlot(true);
        }

        private void addFunctionChangeToGraph(object sender, RoutedEventArgs e)
        {
            updateIterationsPlot(seriesFuncionChange, results.Skip(1), (i) => i.LastFunuctionChange);
        }

        private void removeFunctionChangeFromGraph(object sender, RoutedEventArgs e)
        {
            seriesFuncionChange.Points.Clear();
            model.InvalidatePlot(true);
        }

        private void addPointChangeToGraph(object sender, RoutedEventArgs e)
        {
            updateIterationsPlot(seriesPointChange, results.Skip(1), (i) => i.LastPointChange);
        }

        private void removePointChangeFromGraph(object sender, RoutedEventArgs e)
        {
            seriesPointChange.Points.Clear();
            model.InvalidatePlot(true);
        }

    }
}
