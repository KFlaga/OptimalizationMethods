using System.Windows;

namespace Qfe
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            taskParserPanel.TaskParsed += (s, task) =>
            {
                algorithmPanel.ParsedTask = task;
            };
        }
    }
}
