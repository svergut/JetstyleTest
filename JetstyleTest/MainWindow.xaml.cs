using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Diagnostics;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Runtime.InteropServices;

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

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Items = new ObservableCollection<string>();
            MenuHoverHistoryList.ItemsSource = Items;

            hookId = SetHook(mouseProcess);

            MousePositionChanged += (point) =>
            {
                try
                {
                    var element = AutomationElement.FromPoint(point);
                    
                    if (element != null)
                    {
                        Console.WriteLine(element.Current.Name);
                    }

                    element = null;
                }
                catch (Exception) { }
            }; 


            //Automation.AddAutomationFocusChangedEventHandler(OnFocusChanged);
        }

        
        private AutomationElement GetParentWindow(AutomationElement element)
        {
            var walker = TreeWalker.ControlViewWalker;
            AutomationElement elementParent;
            var node = element;

            try 
            {
                if (node == AutomationElement.RootElement)
                {
                    return node;
                }
                while (true)
                {
                    elementParent = walker.GetParent(node);

                    if (elementParent == null)
                        return null;

                    if (elementParent == AutomationElement.RootElement)
                        break;

                    node = elementParent;
                }
            }
            catch (Exception e)
            {
                return null;
            }

            return node;
        }

        private void OnFocusChanged(object sender, AutomationFocusChangedEventArgs e)
        {
            try
            {
                var focusedElement = sender as AutomationElement;
                var parentWindow = GetParentWindow(focusedElement);

                if (parentWindow != focusedWindow)
                {
                    focusedWindow = parentWindow;
                }

                //TODO: ADD MOUSE MOVE SUPPORT (AUTOMATIONELEMENT.FROMPOINT)

                Automation.AddAutomationEventHandler(AutomationElement.MenuOpenedEvent, 
                    TreeWalker.ControlViewWalker.Normalize(focusedElement), TreeScope.Element, (o, ev) => {
                        Console.WriteLine(o);
                    });

                if (UIAelementIsRequiredProcessWindow(focusedWindow, appName))
                {                    
                    var patterns = focusedElement.GetSupportedPatterns();
                    App.Current.Dispatcher.Invoke(() => { Items.Add(focusedElement.Current.Name); });
                }                    
            }

            catch (ElementNotAvailableException)
            {
            }
        }

        private bool UIAelementIsRequiredProcessWindow(AutomationElement uiaElement, string processName) 
        {
            var uiaElementIsRequiredProcess = false;
            var processesWithGivenName = Process.GetProcessesByName(processName);

            if (processesWithGivenName.Length > 0)
            {
                uiaElementIsRequiredProcess = true;
            }

            return uiaElementIsRequiredProcess;
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
            catch (ElementNotAvailableException exception)
            {
                Console.WriteLine("Automation framework error has occured");
            }
        }
    }
}
