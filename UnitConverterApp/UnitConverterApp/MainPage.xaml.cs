using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using UnitConverterApp.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UnitConverterApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private VoiceUnitConverterViewModel viewModel;
        private MediaElement answerElement;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (viewModel != null)
            {
                return;
            }
            
            var numberToTextConverter = new DecimalToEnglishConverter();
            viewModel = new VoiceUnitConverterViewModel(numberToTextConverter.Convert);
            DataContext = viewModel;

            viewModel.TextToSpeak += async (sender, text) =>
            {
                SuggestionsPanel.Visibility = Visibility.Collapsed;
                ResultsPanel.Visibility = Visibility.Visible;

                var settings = ApplicationData.Current.LocalSettings;
                if (!settings.Values.ContainsKey("SpeakTheAnswer") || (bool) settings.Values["SpeakTheAnswer"] == true)
                {
                    var synthesizer = new SpeechSynthesizer();
                    var stream = await synthesizer.SynthesizeTextToStreamAsync(text);

                    answerElement = new MediaElement();
                    answerElement.SetSource(stream, stream.ContentType);
                    answerElement.Play();
                }

                ConvertButton.IsEnabled = true;
            };
        }

        private async void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            // Disable button so it cannot be tapped twice.
            ConvertButton.IsEnabled = false;

            // Stop speech synthesis while recognizing.
            if (answerElement != null)
            {
                answerElement.Stop();
            }

            var speechRecognizer = new SpeechRecognizer();

            StorageFile file;
            if (viewModel.CheckForRepeat)
            {
                file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Grammars/Repeat.grxml"));
            }
            else
            {
                file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Grammars/Convert.grxml"));
            }

            var constraint = new SpeechRecognitionGrammarFileConstraint(file);
            speechRecognizer.Constraints.Add(constraint);

            await speechRecognizer.CompileConstraintsAsync();
            speechRecognizer.UIOptions.ShowConfirmation = false;

            try
            {
                var result = await speechRecognizer.RecognizeWithUIAsync();

                if (result.Status == SpeechRecognitionResultStatus.Success)
                {
                    viewModel.Convert(result);
                }
                else
                {
                    ConvertButton.IsEnabled = true;
                }
            }
            catch (Exception)
            {
                // Catch this so we don't crash when receiving a phone call.
                ConvertButton.IsEnabled = true;
            }
        }
    }
}
