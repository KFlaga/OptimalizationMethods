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
using System.Windows.Shapes;

namespace Qfe
{
    public partial class TaskWindow : Window
    {
        public Qfe.Task ARiddleToSolve { get; private set; }

        public TaskWindow(Qfe.Task task)
        {
            ARiddleToSolve = task;

            InitializeComponent();
        }
    }
}
