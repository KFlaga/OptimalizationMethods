using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Qfe
{
    public partial class TaskParserPanel : UserControl
    {
        public event EventHandler<Task> TaskParsed;

        public TaskParserPanel()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    Parser.TaskParser.Initialize();
                });
            };
        }

        private void onFullInputAcceptClicked(object sender, RoutedEventArgs e)
        {
            compileTask(fullInput.Text);
        }

        private void onFullInputResetClicked(object sender, RoutedEventArgs e)
        {
            fullInput.Text = (string)Resources["defaultTaskInput"];
        }
        
        private void onWizardInputAcceptClicked(object sender, RoutedEventArgs e)
        {
            string input = convertWizardInput();
            compileTask(input);
        }

        private void compileTask(string input)
        {
            statusTextBlock.Text = "Kompilacja zadania w toku.";

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, (Action)(() =>
            {
                Qfe.Task task = null;
                try
                {
                    Parser.TaskParser parser = new Parser.TaskParser();
                    task = parser.Parse(input);
                    statusTextBlock.Text = buildTaskInfoMessage(input);
                }
                catch (Parser.FunctionCompilationFailure ex)
                {
                    statusTextBlock.Text = buildCompilationError(ex);
                }
                catch (Exception ex)
                {
                    StringBuilder error = new StringBuilder();
                    error.AppendLine("Rzucono wyjątek podczas przetwarzania wejścia. Powód:");
                    error.AppendLine(ex.Message);
                    statusTextBlock.Text = error.ToString();
                }
                TaskParsed?.Invoke(this, task);
            }));
        }

        private string buildTaskInfoMessage(string input)
        {
            StringBuilder msg = new StringBuilder();

            msg.AppendLine("Kompilacja zadania zakończona.");
            msg.AppendLine("Wejście:");
            msg.AppendLine(input);

            return msg.ToString();
        }

        private string buildCompilationError(Parser.FunctionCompilationFailure ex)
        {
            StringBuilder error = new StringBuilder();
            error.AppendLine("Niepowodzenie kompilacji funkcji wejściowej.");

            error.AppendLine("Błędy:");
            foreach (var diag in ex.Diagnostics)
            {
                error.AppendLine(diag.ToString());
            }

            error.AppendLine();
            error.AppendLine("Kod:");
            int line = 1;
            foreach (string codeLine in ex.Code.Split('\n'))
            {
                error.AppendLine(line.ToString() + ":  " + codeLine.TrimEnd());
                line++;
            }

            return error.ToString();
        }

        private void AddParameter_Click(object sender, RoutedEventArgs e)
        {
            var parameterInput = new ParameterInput();
            parameterInput.ToDelete += (s, _) => parametersPanel.Children.Remove(s as ParameterInput);
            parametersPanel.Children.Add(parameterInput);
        }

        private void AddConstraint_Click(object sender, RoutedEventArgs e)
        {
            var constraintInput = new ConstraintInput();
            constraintInput.ToDelete += (s, _) => constraintsPanel.Children.Remove(s as ConstraintInput);
            constraintsPanel.Children.Add(constraintInput);
        }

        private string convertWizardInput()
        {
            StringBuilder result = new StringBuilder();

            //result.AppendLine("$variables: " + variableCountBox.Value.Value.ToString() + ";");

            if(parametersPanel.Children.Count > 0)
            {
                result.AppendLine("$parameters:");
                foreach(var pp in parametersPanel.Children)
                {
                    ParameterInput parameterInput = (ParameterInput)pp;
                    if(parameterInput.ParameterValues.Count == 1)
                    {
                        result.AppendFormat("double {0} = {1};{2}", parameterInput.ParameterName, parameterInput.ParameterValues[0], Environment.NewLine);
                    }
                    else
                    {
                        result.AppendFormat("double[] {0} = new double[] {{ {1} }};{2}",
                            parameterInput.ParameterName,
                            string.Join(", ", parameterInput.ParameterValues),
                            Environment.NewLine);
                    }
                }
            }

            result.AppendLine("$function:");
            result.AppendLine(costFunctionTextBox.Text + ";");

            if (constraintsPanel.Children.Count > 0)
            {
                result.AppendLine("$constraints:");
                foreach (var cp in constraintsPanel.Children)
                {
                    ConstraintInput constraintInput = (ConstraintInput)cp;
                    result.AppendFormat("{0} {1} {2};{3}", constraintInput.Lhs, constraintInput.Operator, constraintInput.Rhs, Environment.NewLine);
                }
            }

            return result.ToString();
        }
    }
}
