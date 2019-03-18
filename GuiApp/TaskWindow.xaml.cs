using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Qfe
{
    public partial class TaskWindow : Window
    {
        public MinimalizationAlgorithm Algorithm { get; private set; }

        private string Status
        {
            set
            {
                algorithmResults.Text = value;
            }
        }
        
        private DispatcherTimer updateTimer;
        private Stopwatch executionTime = new Stopwatch();

        public TaskWindow(MinimalizationAlgorithm algorithm)
        {
            Algorithm = algorithm;

            InitializeComponent();

            updateTimer = new DispatcherTimer(DispatcherPriority.Background, Dispatcher);
            updateTimer.Interval = TimeSpan.FromMilliseconds(1000.0);
            updateTimer.Tick += UpdateTimer_Tick;

            Loaded += TaskWindow_Loaded;
            Closing += TaskWindow_Closing;
        }

        private void TaskWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Status = "Trwa wykonywanie algorytmu";
            startTask();
        }

        private void TaskWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            updateTimer.Stop();
            Algorithm.Terminate();
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
                }
                catch(Exception ex)
                {
                    // TODO
                }

                Dispatcher.Invoke((Action)(() =>
                {
                    Status = "Ukończono";
                }));

                executionTime.Stop();
                updateTimer.Stop();
            }));
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            // TODO
            exeutionTimeLabel.Content = (executionTime.ElapsedMilliseconds * 0.001).ToString() + " s";
        }
    }
}
