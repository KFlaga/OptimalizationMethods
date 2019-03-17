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
    public partial class InitialPointSingleInput : UserControl
    {
        public static readonly DependencyProperty PointNameProperty = DependencyProperty.Register("PointName", typeof(string), typeof(InitialPointSingleInput), new PropertyMetadata(""));
        public static readonly DependencyProperty PointValueProperty = DependencyProperty.Register("PointValue", typeof(double), typeof(InitialPointSingleInput), new PropertyMetadata(0.0));

        int pointNumber;
        public int PointNumber
        {
            get { return pointNumber; }
            set
            {
                pointNumber = value;
                SetValue(PointNameProperty, string.Format("x[{0}] =", pointNumber));
            }
        }

        public double PointValue
        {
            get { return (double)GetValue(PointValueProperty); }
            set { SetValue(PointValueProperty, value); }
        }

        public InitialPointSingleInput()
        {
            InitializeComponent();
            this.DataContext = this;
        }
    }
}
