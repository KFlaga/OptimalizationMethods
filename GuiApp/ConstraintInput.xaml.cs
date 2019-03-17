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
    public partial class ConstraintInput : UserControl
    {
        public string Lhs { get { return lhsBox.Text; } }
        public string Rhs { get { return rhsBox.Text; } }
        public string Operator { get { return (string)((ComboBoxItem)constraintTypeBox.SelectedItem).Content; } }
        public ConstraintType ConstraintType { get { return (ConstraintType)((ComboBoxItem)constraintTypeBox.SelectedItem).Tag; } }

        public event EventHandler<EventArgs> ToDelete;

        public ConstraintInput()
        {
            InitializeComponent();

            rhsBox.Text = "0";
        }

        private void Deleter_Click(object sender, RoutedEventArgs e)
        {
            ToDelete?.Invoke(this, new EventArgs());
        }
    }
}
