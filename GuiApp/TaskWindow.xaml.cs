using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Linq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Qfe
{
    public partial class TaskWindow : Window
    {
        public IterativeMinimalization Algorithm { get; private set; }
        private IList<IterationResults> lastResults { get { return Algorithm.Results; } }

        private string Status
        {
            set
            {
                algorithmResults.Text = value;
            }
        }
        
        private DispatcherTimer updateTimer;
        private Stopwatch executionTime = new Stopwatch();

        public TaskWindow(IterativeMinimalization algorithm)
        {
            Algorithm = algorithm;

            InitializeComponent();

            updateTimer = new DispatcherTimer(DispatcherPriority.Background, Dispatcher);
            updateTimer.Interval = TimeSpan.FromMilliseconds(1000.0);
            updateTimer.Tick += UpdateTimer_Tick;

            Loaded += TaskWindow_Loaded;
            Closing += TaskWindow_Closing;

            if(algorithm.Task.Dim == 2)
            {
                mapPlotPanel.IsEnabled = true;
                functionMapPlot.Task = algorithm.Task;
            }
        }

        private void TaskWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Status = "W trakcie liczenia";
            startTask();
        }

        private void TaskWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            updateTimer.Stop();
            Algorithm.Terminated = true;
        }

        private void startTask()
        {
            updateTimer.Start();
            executionTime.Start();
            System.Threading.Tasks.Task.Run((Action)(() =>
            {
                try
                {
                    Algorithm.Solve();
                    
                    Dispatcher.Invoke((Action)(() =>
                    {
                        pickIteration.IsEnabled = true;
                        pickIteration.Maximum = lastResults.Count - 1;
                        pickIteration.Value = lastResults.Count - 1;
                        iterationsCountLabel.Content = (lastResults.Count - 1).ToString();

                        iterationsPlot.Results = lastResults;
                        if(mapPlotPanel.IsEnabled)
                        {
                            functionMapPlot.Results = lastResults;
                        }
                    }));
                }
                catch(Exception ex)
                {
                    Dispatcher.Invoke((Action)(() =>
                    {
                        Status = "Wystąpił błąd. Szczegóły: " + ex.Message;
                    }));
                }

                executionTime.Stop();
                updateTimer.Stop();
            }));
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            exeutionTimeLabel.Content = (executionTime.ElapsedMilliseconds * 0.001).ToString() + " s";
            Status = buildResultsMessage(Algorithm.Results.Last(), "W trakcie liczenia");

            iterationsPlot.Results = lastResults;
        }

        private string buildResultsMessage(IterationResults result, string status)
        {
            StringBuilder s = new StringBuilder();
            s.AppendFormat("Status: {0}", status);
            s.AppendLine();
            s.AppendLine();
            
            s.AppendFormat("x = {0}", printPoint(result.CurrentPoint));
            s.AppendLine();
            s.AppendFormat("f(x) = {0}", result.CurrentFunction);
            s.AppendLine();
            s.AppendFormat("f(x) + penalty(x) = {0}", result.CurrentCost);
            s.AppendLine();
            s.AppendFormat("max|c(x)| : {0}", result.MaxConstraintValue);
            s.AppendLine();
            s.AppendFormat("|fk+1 - fk| = {0}", result.LastFunuctionChange);
            s.AppendLine();
            s.AppendFormat("||xk+1 - xk|| = {0}", result.LastPointChange);
            s.AppendLine();
            //s.AppendFormat("IT1 = {0}", Algorithm.iterattions1);
            //s.AppendLine();
            //s.AppendFormat("IT2 = {0}", Algorithm.iterattions2);
            //s.AppendLine();

            return s.ToString();
        }

        private string printPoint(MathNet.Numerics.LinearAlgebra.Double.Vector x)
        {
            StringBuilder s = new StringBuilder();
            s.Append("{ ");
            s.Append(string.Join(", ", x.Select((v) => v.ToString("F3"))));
            s.Append(" }");
            return s.ToString();
        }

        private void PickIteration_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Status = buildResultsMessage(lastResults[pickIteration.Value.Value], "Ukończono");
        }
    }
}
