using MathNet.Numerics.LinearAlgebra.Double;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
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

namespace Qfe
{
    public partial class FunctionMapPlot : UserControl
    {
        public Task Task { get; set; }

        private IEnumerable<IterationResults> results;
        public IEnumerable<IterationResults> Results
        {
            get { return results; }
            set
            {
                results = value;
                if (results != null)
                {
                    update(results);
                }
            }
        }

        private PlotModel model = new PlotModel();
        private LinearAxis axisX = new LinearAxis()
        {
            Position = AxisPosition.Bottom,
            Title = "x[0]"
        };
        private LinearAxis axisY = new LinearAxis()
        {
            Position = AxisPosition.Left,
            Title = "x[1]"
        };
        private LinearColorAxis axisHeatMap = new LinearColorAxis()
        {
            Palette = OxyPalettes.Rainbow(100),
            Key = "heatmap"
        };
        private LinearColorAxis axisBlack = new LinearColorAxis()
        {
            Palette = new OxyPalette(OxyColor.FromRgb(0,0,0)),
            Key = "black"
        };
        private LinearColorAxis axisRed = new LinearColorAxis()
        {
            Palette = new OxyPalette(OxyColor.FromRgb(255, 20, 20)),
            Key = "red"
        };

        private int dataResolution = 100;
        private double[,] functionValues;
        private double x_min, x_max, y_min, y_max;

        private HeatMapSeries heatMap = new HeatMapSeries()
        {
            Interpolate = false,
            RenderMethod = HeatMapRenderMethod.Bitmap,
            ColorAxisKey = "heatmap"
        };

        private ContourSeries levelSets = new ContourSeries()
        {
            Color = OxyColors.Black,
            LabelBackground = OxyColors.White
        };

        private ScatterSeries points = new ScatterSeries()
        {
            MarkerSize = 3,
            MarkerType = MarkerType.Circle,
            MarkerFill = OxyColor.FromRgb(0, 0, 0),
            MarkerStroke = OxyColor.FromRgb(240, 240, 240),
            MarkerStrokeThickness = 1,
            ColorAxisKey = "black"
        };

        private ScatterSeries constraints = new ScatterSeries()
        {
            MarkerSize = 1,
            MarkerType = MarkerType.Cross,
            MarkerFill = OxyColor.FromRgb(255, 50, 50),
            ColorAxisKey = "red"
        };

        private List<Series> ordering;

        public FunctionMapPlot()
        {
            InitializeComponent();

            ordering = new List<Series>() { heatMap, constraints, levelSets, points };

            model.Axes.Add(axisX);
            model.Axes.Add(axisY);
            model.Axes.Add(axisHeatMap);
            model.Axes.Add(axisBlack);
            model.Axes.Add(axisRed);

            plot.Model = model;
        }

        private double[] linspace(double from, double to, int count)
        {
            double[] span = new double[count];
            double step = (to - from) / (double)count;
            for(int i = 0; i < count; ++i)
            {
                span[i] = from + i * step;
            }
            return span;
        }

        private void update(IEnumerable<IterationResults> iterations)
        {
            try
            {
                x_min = -1.0; // Default span (e.g. -1 to 1)
                x_max = 1.0;
                y_min = -1.0;
                y_max = 1.0;

                if (iterations != null)
                {
                    x_min = iterations.Min((it) => it.CurrentPoint[0]) - 1.0;
                    x_max = iterations.Max((it) => it.CurrentPoint[0]) + 1.0;
                    y_min = iterations.Min((it) => it.CurrentPoint[1]) - 1.0;
                    y_max = iterations.Max((it) => it.CurrentPoint[1]) + 1.0;
                }

                functionValues = new double[dataResolution, dataResolution];
                double[] xs = linspace(x_min, x_max, dataResolution);
                double[] ys = linspace(y_min, y_max, dataResolution);
                for (int x = 0; x < dataResolution; ++x)
                {
                    for (int y = 0; y < dataResolution; ++y)
                    {
                        var point = new DenseVector(new double[2] { xs[x], ys[y] });
                        functionValues[x, y] = Task.Cost.Function(point);
                    }
                }

                axisX.Minimum = x_min;
                axisX.Maximum = x_max;
                axisY.Minimum = y_min;
                axisY.Maximum = y_max;

                if (buttonLevelSets.IsChecked.Value)
                {
                    updateLevelSets(iterations);
                }
                if(buttonColorMap.IsChecked.Value)
                {
                    updateHeatMap(iterations);
                }
                if (buttonTrackPoints.IsChecked.Value)
                {
                    updatePoints(iterations);
                }
                if (buttonDrawConstraints.IsChecked.Value)
                {
                    updateConstraints(iterations);
                }
            }
            catch (Exception)
            {
                functionValues = null;
                MessageBox.Show("Wystąpił wyjątek podczas wyliczania wartości funkcji - nie będzie działać wykres warstwic.");
            }
        }

        private int indexOfSeries(Series series)
        {
            int index = 0;
            for(int i = 0; i < ordering.Count; ++i)
            {
                if(series == ordering[index])
                {
                    break;
                }
                if(model.Series.Contains(ordering[index]))
                {
                    index++;
                }
            }
            return index;
        }

        private void updateLevelSets(IEnumerable<IterationResults> iterations)
        {
            if (functionValues != null)
            {
                levelSets.ColumnCoordinates = linspace(x_min, x_max, dataResolution);
                levelSets.RowCoordinates = linspace(y_min, y_max, dataResolution);

                levelSets.Data = functionValues;
                model.Series.Insert(indexOfSeries(levelSets), levelSets);
                plot.InvalidatePlot(true);
            }
        }

        private void updateHeatMap(IEnumerable<IterationResults> iterations)
        {
            if(functionValues != null)
            {
                heatMap.X0 = x_min;
                heatMap.X1 = x_max;
                heatMap.Y0 = y_min;
                heatMap.Y1 = y_max;

                heatMap.Data = functionValues;
                model.Series.Insert(indexOfSeries(heatMap), heatMap);
                plot.InvalidatePlot(true);
            }
        }

        private void updatePoints(IEnumerable<IterationResults> iterations)
        {
            points.Points.Clear();
            if (iterations != null && iterations.Count() > 0)
            {
                var curr = iterations.GetEnumerator();
                curr.MoveNext();
                var prev = iterations.GetEnumerator();
                prev.MoveNext();

                points.Points.Add(new ScatterPoint(curr.Current.CurrentPoint[0], curr.Current.CurrentPoint[1], 3.0, curr.Current.CurrentFunction));

                while(curr.MoveNext())
                {
                    points.Points.Add(new ScatterPoint(curr.Current.CurrentPoint[0], curr.Current.CurrentPoint[1], 3.0, curr.Current.CurrentFunction));

                    model.Annotations.Add(new ArrowAnnotation()
                    {
                        StartPoint = new DataPoint(prev.Current.CurrentPoint[0], prev.Current.CurrentPoint[1]),
                        EndPoint = new DataPoint(curr.Current.CurrentPoint[0], curr.Current.CurrentPoint[1]),
                        Color = OxyColor.FromRgb(0, 0, 0),
                        LineStyle = LineStyle.Dash,
                        StrokeThickness = 1.0
                    });
                    prev.MoveNext();
                }
                
                model.Series.Insert(indexOfSeries(points), points);
                plot.InvalidatePlot(true);
            }
        }

        private void updateConstraints(IEnumerable<IterationResults> iterations)
        {
            constraints.Points.Clear();
            if (iterations != null)
            {
                // How to find exact border points ?
                // Root finding algorithm -> for each border point with False get closest one with True and find root of constraint (or min of abs(c(x)) * H)
                // Sample over x-y space to find False and True
                // How to find borders?
                // - e.g. got through each row and mark pairs of changing points
                // But maybe todo later

                double[] xs = linspace(x_min, x_max, dataResolution);
                double[] ys = linspace(y_min, y_max, dataResolution);
                for (int x = 0; x < dataResolution; ++x)
                {
                    for (int y = 0; y < dataResolution; ++y)
                    {
                        var point = new DenseVector(new double[2] { xs[x], ys[y] });
                        foreach(var constraint in Task.Constraints)
                        {
                            if(!constraint.IsMet(point))
                            {
                                constraints.Points.Add(new ScatterPoint(point[0], point[1], 2.0, 1.0));
                                break;
                            }
                        }
                    }
                }
                model.Series.Insert(indexOfSeries(constraints), constraints);
                plot.InvalidatePlot(true);
            }
        }

        private void ButtonLevelSets_Checked(object sender, RoutedEventArgs e)
        {
            updateLevelSets(Results);
        }

        private void ButtonLevelSets_Unchecked(object sender, RoutedEventArgs e)
        {
            model.Series.Remove(levelSets);
            plot.InvalidatePlot(true);
        }

        private void ButtonColorMap_Checked(object sender, RoutedEventArgs e)
        {
            updateHeatMap(Results);
        }

        private void ButtonColorMap_Unchecked(object sender, RoutedEventArgs e)
        {
            model.Series.Remove(heatMap);
            plot.InvalidatePlot(true);
        }

        private void ButtonTrackPoints_Checked(object sender, RoutedEventArgs e)
        {
            updatePoints(Results);
        }

        private void ButtonTrackPoints_Unchecked(object sender, RoutedEventArgs e)
        {
            model.Series.Remove(points);
            model.Annotations.Clear();
            plot.InvalidatePlot(true);
        }

        private void ButtonDrawConstraints_Checked(object sender, RoutedEventArgs e)
        {
            updateConstraints(Results);
        }

        private void ButtonDrawConstraints_Unchecked(object sender, RoutedEventArgs e)
        {
            model.Series.Remove(constraints);
            plot.InvalidatePlot(true);
        }
    }
}
