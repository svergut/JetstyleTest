using System;
using System.Windows;
using System.Windows.Automation;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace JetstyleTest
{

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

        private void HandleHoveredElement(AutomationElement element)
        {
            App.Current.Dispatcher.Invoke(() => { Items.Add(element.Current.Name); });
            speechHelper.Speak(element.Current.Name);

            previouslyFocusedMenuItem = element;
        }  
        
        private void OnMousePositionChanged(Point point)
        { 
            try
            {
                var element = AutomationElement.FromPoint(point);

                if (element == null)
                    return;

                if (element != previouslyFocusedMenuItem && focusedWindow.Current.BoundingRectangle.Contains(point))
                {
                    var elementMustBeHandled = true;

                    if (targetElementsNames != null && !targetElementsNames.Contains(element.Current.Name))
                        elementMustBeHandled = false;

                    if (elementMustBeHandled)
                        HandleHoveredElement(element);
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

                if (focusedElement == null)
                    return;

                var parentWindow = AutomationHelper.GetParentWindow(focusedElement);

                if (parentWindow != focusedWindow)
                {
                    focusedWindow = parentWindow;

                    if (AutomationHelper.UIAelementIsRequiredProcessWindow(focusedWindow, Constants.appName))                   
                        MousePositionChanged += OnMousePositionChanged;         
                    else
                        MousePositionChanged -= OnMousePositionChanged;
                }
            }
            catch (ElementNotAvailableException) { }
        }                
    }
}
