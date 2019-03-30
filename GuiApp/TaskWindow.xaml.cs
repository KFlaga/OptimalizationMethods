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

            if(algorithm.Task.Rank == 2)
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

                    string msg = buildResultsMessage(lastResults, "Ukończono");
                    Dispatcher.Invoke((Action)(() =>
                    {
                        Status = msg;
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
            Status = buildResultsMessage(Algorithm.Results, "W trakcie liczenia");

            iterationsPlot.Results = lastResults;
        }

        private string buildResultsMessage(IList<IterationResults> results, string status)
        {
            var lastResult = results[results.Count - 1];

            StringBuilder s = new StringBuilder();
            s.AppendFormat("Status: {0}", status);
            s.AppendLine();
            s.AppendLine();

            s.AppendFormat("Iteracja: {0} / {1}", lastResult.Iteration, Algorithm.MaxIterations);
            s.AppendLine();
            s.AppendFormat("x = {0}", printPoint(lastResult.CurrentPoint));
            s.AppendLine();
            s.AppendFormat("f(x) = {0}", lastResult.CurrentFunction);
            s.AppendLine();
            s.AppendFormat("f(x) + penalty(x) = {0}", lastResult.CurrentCost);
            s.AppendLine();
            s.AppendFormat("Ograniczenia spełnione: {0}", lastResult.CostraintsMet);
            s.AppendLine();
            s.AppendFormat("|fk+1 - fk| = {0}", lastResult.LastFunuctionChange);
            s.AppendLine();
            s.AppendFormat("||xk+1 - xk|| = {0}", lastResult.LastPointChange);
            s.AppendLine();
            
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
    }
}
