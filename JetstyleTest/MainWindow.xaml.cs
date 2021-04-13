using System;
using System.Windows;
using System.Windows.Automation;
using System.Diagnostics;
using System.Speech.Synthesis;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JetstyleTest
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SpeechHelper speechHelper = new SpeechHelper();
        private AutomationElement previouslyFocusedMenuItem;
        private HashSet<string> targetElementsNames;
        private delegate void NotepadLaunchHandler(Process process);
        private AutomationElement focusedWindow;
        public ObservableCollection<string> Items { get; set; }


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Items = new ObservableCollection<string>();
            MenuHoverHistoryList.ItemsSource = Items;

            AddCheckboxListeners();

            Closed += (o, e) =>
            {
                UnhookWindowsHookEx(hookId);
            };

            CreateMouseHook();
            Automation.AddAutomationFocusChangedEventHandler(OnFocusChanged);
        }

        private void AddCheckboxListeners()
        {
            Checkbox.Checked += (o, e) =>
            {
                targetElementsNames = Constants.NotepadMainMenuItemNames;
            };

            Checkbox.Unchecked += (o, e) =>
            {
                targetElementsNames = null;
            };
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
                        if (element != null && element != previouslyFocusedMenuItem && focusedWindow.Current.BoundingRectangle.Contains(point))
                        {
                            if (targetElementsNames != null)
                            {
                                if (targetElementsNames.Contains(element.Current.Name))
                                {
                                    App.Current.Dispatcher.Invoke(() => { Items.Add(element.Current.Name); });
                                    speechHelper.Speak(element.Current.Name);

                                    previouslyFocusedMenuItem = element;
                                }
                            }
                            else
                            {
                                App.Current.Dispatcher.Invoke(() => { Items.Add(element.Current.Name); });
                                speechHelper.Speak(element.Current.Name);

                                previouslyFocusedMenuItem = element;
                            }
                            
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

                    if (AutomationHelper.UIAelementIsRequiredProcessWindow(focusedWindow, Constants.appName))
                    {                        
                        MousePositionChanged += OnMousePositionChanged;
                    }                       
                    else
                    {
                        MousePositionChanged -= OnMousePositionChanged;
                    }
                        

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
