using System;
using System.Windows;
using System.Windows.Controls;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Qfe
{
    public partial class AlgorithmPanel : UserControl
    {
        Qfe.Task task;
        public Qfe.Task ParsedTask
        {
            get { return task; }
            set
            {
                task = value;
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    updateInitialPoints(task != null ? task.Rank : 0);
                    startButton.IsEnabled = task != null;
                }));
            }
        }
        
        public InitialPointMethod InitializationMethod { get; set; } = InitialPointMethod.Manual;
        public int MaxIterations { get { return maxIterationsBox.Value.Value; } }
        public double MinPositionChange { get { return minPositionChangeBox.Value.Value; } }
        public double MinFunctionChange { get { return minFunctionChangeBox.Value.Value; } }
        public double MysteriousCriteria { get { return mysteriusCriteriaBox.Value.Value; } }

        public double[] InitialValues
        {
            get
            {
                if(InitializationMethod == InitialPointMethod.Manual)
                {
                    double[] result = new double[initialPointsPanel.Children.Count];
                    foreach(var child in initialPointsPanel.Children)
                    {
                        var ip = (InitialPointSingleInput)child;
                        result[ip.PointNumber] = ip.PointValue;
                    }
                    return result;
                }
                else
                {
                    // TODO
                }
                return null;
            }
        }

        public AlgorithmPanel()
        {
            InitializeComponent();
            this.DataContext = this;

            initialPointMethodBox.SelectedIndex = 0;
        }

        private void InitialPointMethodBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count > 0)
            {
                ComboBoxItem item = (ComboBoxItem)e.AddedItems[0];
                InitialPointMethod newMethod = (InitialPointMethod)item.Tag;
            }
        }

        private void StartAlgorithm_Click(object sender, RoutedEventArgs e)
        {
            TaskWindow taskWindow = new TaskWindow(
                new GaussSiedler()
                {
                    Task = task,
                    InitializationMethod = InitializationMethod,
                    InitialPoint = new DenseVector(InitialValues),
                    MinPositionChange = MinPositionChange,
                    MinFunctionChange = MinFunctionChange,
                    MysteriusCriteria = MysteriousCriteria,
                    MaxIterations = MaxIterations
                })
            {
                ShowActivated = true
            };
            taskWindow.Show();
        }

        private void updateInitialPoints(int rank, InitialPointMethod method)
        {
            if(method != InitializationMethod)
            {
                initialPointsPanel.Children.Clear();
            }
            InitializationMethod = method;
            updateInitialPoints(rank);
        }

        private void updateInitialPoints(int rank)
        {
            var boxes = initialPointsPanel.Children;
            if (rank > boxes.Count)
            {
                for(int i = boxes.Count; i < rank; ++i)
                {
                    UIElement box;
                    if(InitializationMethod == InitialPointMethod.Manual)
                    {
                        box = new InitialPointSingleInput()
                        {
                            PointNumber = i
                        };
                    }
                    else
                    {
                        box = new InitialPointSingleInput()
                        {
                            PointNumber = i
                        };
                    }
                    boxes.Add(box);
                }
            }
            else if (rank < boxes.Count)
            {
                int toRemove = boxes.Count - rank;
                int first = boxes.Count - toRemove;
                boxes.RemoveRange(first, toRemove);
            }
        }
    }
}
