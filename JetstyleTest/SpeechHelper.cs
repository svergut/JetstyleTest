using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace JetstyleTest
{
    public class SpeechHelper
    {
        SpeechSynthesizer speechSynthesizer;

        public SpeechHelper()
        {
            speechSynthesizer = new SpeechSynthesizer();
            speechSynthesizer.Rate = 4;
        }

        void Speak(string text)
        {
            speechSynthesizer.SpeakAsync(new Prompt(text));
        }
    }
}
