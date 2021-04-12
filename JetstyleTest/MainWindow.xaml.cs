using System;
using System.Windows;
using System.Windows.Automation;
using System.Diagnostics;
using System.Speech.Synthesis;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading;

namespace JetstyleTest
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string appName = "notepad";
        delegate void NotepadLaunchHandler(Process process);
        private AutomationElement focusedWindow;
        public ObservableCollection<string> Items { get; set; }

       

        //mousehook causes perf problems
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Items = new ObservableCollection<string>();
            MenuHoverHistoryList.ItemsSource = Items;

            Closed += (o, e) =>
            {
                UnhookWindowsHookEx(hookId);
            };

            CreateMouseHook();
            Automation.AddAutomationFocusChangedEventHandler(OnFocusChanged);
        }

        private void CreateMouseHook()
        {
            mouseProcess = HookCallback;
            hookId =
                SetHook(mouseProcess);
        }

        private void OnMousePositionChanged(Point point)
        { 
            try
            {
                var element = AutomationElement.FromPoint(point);

                if (element != null)
                {
                    try
                    {
                        if (element != null)
                        {
                            App.Current.Dispatcher.Invoke(() => { Items.Add(element.Current.Name); });
                            Console.WriteLine(element.Current.Name);
                        }
                    }
                    catch (ElementNotAvailableException)
                    {
                        Console.WriteLine("Automation framework error has occured");
                    }
                }

                element = null;
            }
            catch (Exception) { }
        }

        private void OnFocusChanged(object sender, AutomationFocusChangedEventArgs e)
        {
            try
            {
                var focusedElement = sender as AutomationElement;
              
                var parentWindow = AutomationHelper.GetParentWindow(focusedElement);

                if (parentWindow != focusedWindow)
                {
                    focusedWindow = parentWindow;

                    if (AutomationHelper.UIAelementIsRequiredProcessWindow(focusedWindow, appName))
                        new Thread(() => { MousePositionChanged += OnMousePositionChanged; }).Start();
                    else
                        new Thread(() => { MousePositionChanged -= OnMousePositionChanged; }).Start();

                }
            }
            catch (ElementNotAvailableException) { }
        }
        
        private void HandleAutomationFocusChange(object o, AutomationFocusChangedEventArgs e)
        {
            var speechSynthesizer = new SpeechSynthesizer();
            speechSynthesizer.Rate = 4;

            try
            {
                var element = o as AutomationElement;

                if (element != null)
                {
                    speechSynthesizer.SpeakAsyncCancelAll();
                    speechSynthesizer.Speak(new Prompt(element.Current.Name));
                }
            }
            catch (ElementNotAvailableException)
            {
                Console.WriteLine("Automation framework error has occured");
            }
        }
    }
}
