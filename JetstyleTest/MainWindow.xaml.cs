using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Diagnostics;
using System.Speech.Synthesis;

namespace JetstyleTest
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string appName = "notepad";
        delegate void NotepadLaunchHandler(Process process);
        event NotepadLaunchHandler NotepadLaunched;

        public MainWindow()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
            {
                Debug.WriteLine(eventArgs.Exception.ToString());
            };

            StartListeningForNotepadLaunch(1000);

            Automation.AddAutomationFocusChangedEventHandler((o, e) => {
                Console.WriteLine((o as AutomationElement).Current.Name);
                Console.WriteLine(e.EventId);
            });

            NotepadLaunched += (process) =>
            {
                var notepadProcess = Process.GetProcessesByName(appName).First();
                var notepadWindow = AutomationElement.FromHandle(notepadProcess.Handle);

                //Automation.AddAutomationEventHandler(AutomationElementIdentifiers.ToolTipOpenedEvent, notepadWindow, TreeScope.Element, (o, e) => {
                //    Console.WriteLine(e.EventId);
                //});

                //fetch all children then compare

                var speechSynthesizer = new SpeechSynthesizer();

                Automation.AddAutomationFocusChangedEventHandler((o, e) => {
                    Console.WriteLine((o as AutomationElement).Current.BoundingRectangle + " " + (o as AutomationElement).Current.Name);
                    

                    var currentProcess = Process.GetCurrentProcess();

                   
                    if (currentProcess.ProcessName == notepadProcess.ProcessName)
                    {
                        var element = o as AutomationElement;

                        if (element != null)
                        {
                            speechSynthesizer.SpeakAsyncCancelAll();
                            speechSynthesizer.Speak(new Prompt(element.Current.Name));
                        }
                    }

                    
                        
                });
            };

            
        }  
        
        private void StartListeningForNotepadLaunch(double interval)
        {
            var timer = new System.Timers.Timer(interval);
            timer.Start();

            timer.Elapsed += (i, a) =>
            {
                var processes = Process.GetProcessesByName(appName);

                if (processes.Length > 0)
                {
                    var notepadProcess = processes.First();

                    NotepadLaunched?.Invoke(notepadProcess);

                    timer.Stop();
                }
            };
        }

    
    }
}
