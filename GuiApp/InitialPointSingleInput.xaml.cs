using System.Windows;
using System.Windows.Controls;

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
