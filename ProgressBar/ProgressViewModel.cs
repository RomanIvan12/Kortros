using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Autodesk.Revit.UI.Events;

namespace ProgressBar
{
    public class ProgressViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _progressValue;

        public int ProgressValue
        {
            get => _progressValue;
            set
            {
                _progressValue = value;
                OnPropertyChanged();
                // OnPropertyChanged(nameof(ProgressText)); // Уведомляем, что ProgressText тоже изменился
            }
        }
        private int _processedWallsCount;
        public int ProcessedWallsCount
        {
            get => _processedWallsCount;
            private set
            {
                _processedWallsCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProgressText)); // Обновляем ProgressText при изменении ProcessedWallsCount
            }
        }
        public string ProgressText => $"Обработано стен: {ProcessedWallsCount}";

        private readonly ExternalEventHandler _externalEventHandler;
        private readonly ExternalEvent _externalEvent;
        public ProgressViewModel()
        {
            _externalEventHandler = new ExternalEventHandler();
            _externalEvent = ExternalEvent.Create(_externalEventHandler);
        }

        private RelayCommand _startCommand;
        public RelayCommand StartCommand
        {
            get
            {
                return _startCommand ??
                       (_startCommand = new RelayCommand(obj =>
                       {
                           try
                           {
                               _externalEventHandler.SetAction(RunRun);
                               _externalEvent.Raise();
                           }
                           catch (Exception ex)
                           {
                               MessageBox.Show($"ошибка загрузки параметров: {ex.Message}");
                           }
                       }));
            }
        }
        private void RunRunRun()
        {
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            timer.Tick += (s, e) =>
            {
                if (ProgressValue < 100)
                {
                    ProgressValue += 1;
                }
                else
                {
                    timer.Stop();
                    //CloseWindowAfterDelay(); // Закрываем окно
                }
            };
            timer.Start();
        }

        private void RunRun()
        {
            List<Element> listOfWalls1 = new FilteredElementCollector(ProgressBarCommand.Doc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();

            int totalWalls = listOfWalls1.Count;
            ProcessedWallsCount = 0;

            double previousPercentageComplete = -1; // Хранение предыдущего значения процента

            using (Transaction trans = new Transaction(ProgressBarCommand.Doc, "Update Wall Comment"))
            {
                trans.Start();
                foreach (Element el in listOfWalls1)
                {
                    // Устанавливаем значение параметра "Комментарий" для каждой стены
                        el.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)
                            ?.Set($"Стена номер {ProcessedWallsCount + 1}");

                    // Увеличиваем счетчик обработанных стен
                    ProcessedWallsCount++;

                    // Рассчитываем текущий процент выполнения
                    double currentPercentageComplete = (double)ProcessedWallsCount / totalWalls * 100;

                    // Проверяем, изменился ли процент на 1% по сравнению с предыдущим состоянием
                    if ((int)currentPercentageComplete > (int)previousPercentageComplete)
                    {
                        // Обновляем прогресс
                        ProgressValue = (int)currentPercentageComplete;
                        previousPercentageComplete = currentPercentageComplete;

                        // Информируем UI, чтобы обновить интерфейс
                        System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(() => { },
                            DispatcherPriority.Background);
                    }
                }
                trans.Commit();
            }
        }
            //try
            //{
            //    // Подсчитываем все стены
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Ошибка загрузки параметров: {ex.Message}");
            //}

        private void RunRunRunRun()
        {
            try
            {
                // Подсчитываем все стены
                List<Element> listOfWalls = new FilteredElementCollector(ProgressBarCommand.Doc)
                    .OfCategory(BuiltInCategory.OST_Walls)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .ToList();

                ProcessedWallsCount = 0;

                foreach (Element el in listOfWalls)
                {
                    // Устанавливаем значение параметра "Комментарий" для каждой стены
                    using (Transaction trans = new Transaction(ProgressBarCommand.Doc, "Update Wall Comment"))
                    {
                        trans.Start();
                        el.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)
                            ?.Set($"Стена номер {ProcessedWallsCount + 1}");
                        trans.Commit();
                    }
                    ProcessedWallsCount++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки параметров: {ex.Message}");
            }

            MessageBox.Show("ok");
        }

        private void CloseWindowAfterDelay()
        {
            Task.Delay(TimeSpan.FromSeconds(3)); // Ждем 3 секунд
            Application.Current.Dispatcher.Invoke(() =>
            {
                var window = Application.Current.Windows.OfType<Window>().FirstOrDefault();
                window?.Close();
            });
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ExternalEventHandler : IExternalEventHandler
    {
        private Action _action;
        public void SetAction(Action action)
        {
            _action = action;
        }
        public void Execute(UIApplication app)
        {
            try
            {
                _action?.Invoke();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Ошибка: {ex.Message} \n{ex.StackTrace}");
            }
        }
        public string GetName()
        {
            return "External Event Handler";
        }
    }

    public class RelayCommand : ICommand
    {
        private Action<object> execute;
        private Func<object, bool> canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }
        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute(parameter);
        }
        public void Execute(object parameter)
        {
            this.execute(parameter);
        }
    }
}
