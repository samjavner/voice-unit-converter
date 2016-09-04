using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;

namespace UnitConverterApp.ViewModels
{
    public class VoiceUnitConverterViewModel : ViewModel
    {
        private readonly Func<decimal, string> numberToTextConverter;
        
        private string fromText;
        private string toText;
        private string equalsText;
        
        private bool checkForRepeat = false;
        private string conversion;
        private string singularFrom;
        private string pluralFrom;
        private string singularTo;
        private string pluralTo;
        private decimal factorFrom;
        private decimal factorTo;
        private decimal offsetFrom;
        private decimal offsetTo;

        public VoiceUnitConverterViewModel(Func<decimal, string> numberToTextConverter)
        {
            this.numberToTextConverter = numberToTextConverter;
        }
        
        public event EventHandler<string> TextToSpeak;
        
        public string FromText
        {
            get { return fromText; }
            private set
            {
                fromText = value;
                NotifyPropertyChanged("FromText");
            }
        }

        public string ToText
        {
            get { return toText; }
            private set
            {
                toText = value;
                NotifyPropertyChanged("ToText");
            }
        }
        
        public string EqualsText
        {
            get { return equalsText; }
            private set
            {
                equalsText = value;
                NotifyPropertyChanged("EqualsText");
            }
        }

        public bool CheckForRepeat
        {
            get { return checkForRepeat; }
        }

        public void Convert(SpeechRecognitionResult result)
        {
            try
            {
                DoConversion(result);
                checkForRepeat = true;
            }
            catch (OverflowException)
            {
                FromText = "";
                EqualsText = "Error";
                ToText = "";
                OnTextToSpeak("Error");
                checkForRepeat = false;
            }
            catch (DivideByZeroException)
            {
                FromText = "";
                EqualsText = "Error";
                ToText = "";
                OnTextToSpeak("Error");
                checkForRepeat = false;
            }
        }

        private void DoConversion(SpeechRecognitionResult result)
        {
            bool isRepeat = result.SemanticInterpretation.Properties.ContainsKey("isRepeat");
            decimal number = decimal.Parse(result.SemanticInterpretation.Properties["number"][0]);

            if (!isRepeat)
            {
                conversion = result.SemanticInterpretation.Properties["conversion"][0];
                singularFrom = result.SemanticInterpretation.Properties["singularFrom"][0];
                pluralFrom = result.SemanticInterpretation.Properties["pluralFrom"][0];
                singularTo = result.SemanticInterpretation.Properties["singularTo"][0];
                pluralTo = result.SemanticInterpretation.Properties["pluralTo"][0];
                factorFrom = decimal.Parse(result.SemanticInterpretation.Properties["factorFrom"][0]);
                factorTo = decimal.Parse(result.SemanticInterpretation.Properties["factorTo"][0]);
            }

            decimal answer;

            switch (conversion)
            {
                case "temperature":
                    if (!isRepeat)
                    {
                        offsetFrom = decimal.Parse(result.SemanticInterpretation.Properties["offsetFrom"][0]);
                        offsetTo = decimal.Parse(result.SemanticInterpretation.Properties["offsetTo"][0]);
                    }
                    answer = (number + offsetFrom) * (factorTo / factorFrom) - offsetTo;
                    break;
                case "temperatureInterval":
                    answer = number * (factorTo / factorFrom);
                    break;
                default:
                    answer = number * (factorFrom / factorTo);
                    break;
            }

            string fromText = number.ToString("###,###,###,###,###,##0.####################");
            fromText += fromText == "1" ? " " + singularFrom : " " + pluralFrom;

            string toText = answer.ToString("###,###,###,###,###,##0.####");
            toText += toText == "1" ? " " + singularTo : " " + pluralTo;

            FromText = fromText;
            EqualsText = "=";
            ToText = toText;

            number = Math.Round(number, 20, MidpointRounding.AwayFromZero);
            answer = Math.Round(answer, 4, MidpointRounding.AwayFromZero);

            string fromTextSpoken = numberToTextConverter(number);
            fromTextSpoken += fromTextSpoken == "one" ? " " + singularFrom : " " + pluralFrom;

            string toTextSpoken = numberToTextConverter(answer);
            toTextSpoken += toTextSpoken == "one" ? " " + singularTo : " " + pluralTo;

            var textToSpeak = fromTextSpoken + " equals " + toTextSpoken;
            OnTextToSpeak(textToSpeak);
        }

        private void OnTextToSpeak(string textToSpeak)
        {
            TextToSpeak?.Invoke(this, textToSpeak);
        }
    }
}
