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
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Diagnostics;
using System.Windows.Automation.Peers;
using System.Threading;
using System.Windows.Interop;

namespace JetstyleTest
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AutomationEventHandler UIAeventHandler;

        public MainWindow()
        {
            InitializeComponent();

            var notepadWindow = AutomationElement.FromLocalProvider(AutomationInteropProvider.HostProviderFromHandle(Process.GetProcessesByName("notepad").First().MainWindowHandle));
            var menuItemCondition = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.MenuItem);

            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
            {
                Debug.WriteLine(eventArgs.Exception.ToString());
            };

            var elements = notepadWindow.FindAll(TreeScope.Descendants, menuItemCondition);
            var helpElement = elements[elements.Count - 1];

            var expandPattern = helpElement.GetCurrentPattern(ExpandCollapsePattern.Pattern) as ExpandCollapsePattern;
            expandPattern.Expand();

            Automation.AddAutomationFocusChangedEventHandler((o, e) => {
                Console.WriteLine((o as AutomationElement).Current.Name);
            });

            Automation.AddAutomationEventHandler(AutomationElement.MenuOpenedEvent, helpElement, TreeScope.Element, (o, e) => {
                Console.WriteLine("debug");
            });

            Console.WriteLine();

            //Automation.AddAutomationEventHandler(SelectionItemPattern.ElementSelectedEvent,
            //       elementItem, TreeScope.Element,
            //       new AutomationEventHandler(OnUIAutomationEvent));

            //Automation.AddAutomationEventHandler(InvokePattern.InvokedEvent,
            //    element, 
            //    TreeScope.Element, OnNotepadControlAction);
        }

        private static AutomationPattern GetSpecifiedPattern(AutomationElement element, string patternName)
        {
            AutomationPattern[] supportedPattern = element.GetSupportedPatterns();

            foreach (AutomationPattern pattern in supportedPattern)
            {
                if (pattern.ProgrammaticName == patternName)
                    return pattern;
            }

            return null;
        }

        private void OnNotepadControlAction(object sender, AutomationEventArgs automationEventArgs)
        {
            var spacing = "   ";
            var rootElement = sender as AutomationElement;
            
            Console.WriteLine(rootElement.Current.ClassName);
            
            Automation.AddAutomationEventHandler(SelectionItemPattern.ElementSelectedEvent,
                   rootElement.GetUpdatedCache(CacheRequest.Current), TreeScope.Children,
                   new AutomationEventHandler(OnUIAutomationEvent));
            
            var children = rootElement.FindAll(TreeScope.Children, System.Windows.Automation.Condition.TrueCondition);
            
            foreach (AutomationElement child in children)
            {
                var childrenSpacing = spacing + "   ";
            
                Console.WriteLine(childrenSpacing + child.Current.ClassName);
            
                var ch = child.FindAll(TreeScope.Children, System.Windows.Automation.Condition.TrueCondition);
            
                foreach (AutomationElement c in ch)
                {
                    Console.WriteLine(childrenSpacing + c.Current.Name);
                    Automation.AddAutomationEventHandler(TextPattern.TextChangedEvent, c.GetUpdatedCache(CacheRequest.Current), TreeScope.Descendants,
                    new AutomationEventHandler(OnUIAutomationEvent));
                }
                    
            
            }
        }

       

        private void OnUIAutomationEvent(object src, AutomationEventArgs e)
            {
            Console.WriteLine(e.EventId);
                // Make sure the element still exists. Elements such as tooltips
                // can disappear before the event is processed.
                AutomationElement sourceElement;
                try
                {
                    sourceElement = src as AutomationElement;
                }
                catch (ElementNotAvailableException)
                {
                    return;
                }
                if (e.EventId == InvokePattern.InvokedEvent)
                {
                    // TODO Add handling code.
                }
                else
                {
                    // TODO Handle any other events that have been subscribed to.
                }
            }

        }
}
