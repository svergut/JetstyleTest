using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace JetstyleTest
{
    public class AutomationHelper
    {
        public static bool UIAelementIsRequiredProcessWindow(AutomationElement uiaElement, string processName)
        {
            var uiaElementIsRequiredProcess = false;
            var processesWithGivenName = Process.GetProcessesByName(processName);

            if (processesWithGivenName.Length > 0 && uiaElement.Current.ClassName == "Notepad") //add to variables
                uiaElementIsRequiredProcess = true;

            return uiaElementIsRequiredProcess;
        }

        public static AutomationElement GetParentWindow(AutomationElement element)
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
            catch (Exception)
            {
                return null;
            }

            return node;
        }
    }
}
