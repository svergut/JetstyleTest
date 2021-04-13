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
            speechSynthesizer.Rate = 3;
        }

        public void Speak(string text)
        {
            if (speechSynthesizer.State == SynthesizerState.Speaking)
                try { speechSynthesizer.SpeakAsyncCancelAll(); }
                catch (Exception) { }

            speechSynthesizer.SpeakAsync(new Prompt(text));



            //speechSynthesizer.Speak(new Prompt(text));
        }
    }
}
