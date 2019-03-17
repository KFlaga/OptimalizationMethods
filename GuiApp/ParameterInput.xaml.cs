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

using Xceed.Wpf.Toolkit;

namespace Qfe
{
    public partial class ParameterInput : UserControl
    {
        public string ParameterName { get { return nameBox.Text; } }
        public List<double> ParameterValues { get { return valueBoxes.Select((x) => x.Value.Value).ToList(); } }

        public event EventHandler<EventArgs> ToDelete;

        List<DoubleUpDown> valueBoxes = new List<DoubleUpDown>();

        public ParameterInput()
        {
            InitializeComponent();

            lengthBox.Value = 1;
            addValueBox();
            lengthBox.ValueChanged += LengthBox_ValueChanged;
        }

        private void addValueBox()
        {
            DoubleUpDown box = new DoubleUpDown()
            {
                Width = 40,
                Height = 25,
                Value = 0.0,
                ShowButtonSpinner = false
            };
            valueBoxes.Add(box);
            valuesPanel.Children.Add(box);
        }

        private void LengthBox_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            int toAdd = (int)e.NewValue - valueBoxes.Count;

            if (toAdd < 0)
            {
                int toRemove = -toAdd;
                int first = valueBoxes.Count - toRemove;
                valueBoxes.RemoveRange(first, toRemove);
                valuesPanel.Children.RemoveRange(first, toRemove);
            }
            while (toAdd > 0)
            {
                addValueBox();
                toAdd--;
            }
        }

        private void Deleter_Click(object sender, RoutedEventArgs e)
        {
            ToDelete?.Invoke(this, new EventArgs());
        }
    }
}
