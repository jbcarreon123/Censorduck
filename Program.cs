using Windows.Foundation;
using Windows.Media.SpeechRecognition;
using Discord.Webhook;
using Discord;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Media.Capture;
using System.Drawing;
using System.Collections.ObjectModel;
using Windows.Devices.Enumeration;
using System.Windows.Forms;
using System.Reflection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Censorduck
{
    internal class Program
    {
        private SpeechRecognizer reco;
        private DiscordWebhookClient loggingWebhook;
        private DiscordWebhookClient cursingWebhook;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private CancellationToken cancellationToken { get; set; }

        static void Main(string[] args)
        {
            new Program().MainAsync(args).GetAwaiter().GetResult();
        }

        async Task MainAsync(string[] args)
        {
            cancellationToken = cancellationTokenSource.Token;

            await AudioCapturePermissions.RequestMicrophonePermission();

            var conf = GetConfig();
            loggingWebhook = new DiscordWebhookClient(conf.LoggingWebhookUrl);
            cursingWebhook = new DiscordWebhookClient(conf.SwearWebhookUrl);

            reco = new SpeechRecognizer();
            reco.Constraints.Add(new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, "dictation"));

            var op = await reco.CompileConstraintsAsync();
            if (op.Status == SpeechRecognitionResultStatus.Success)
            {
                reco.StateChanged += async (a, b) => await Reco_StateChanged(a, b);
                reco.HypothesisGenerated += Reco_HypothesisGenerated;
                Console.Clear();
                Console.WriteLine("Status: Starting");
                Console.SetCursorPosition(0, 2);
                Console.WriteLine("Result:");
                Console.SetCursorPosition(0, 14);
                Console.WriteLine("Hypothesis:");
                reco.ContinuousRecognitionSession.ResultGenerated += (a, b) => ContinuousRecognitionSession_ResultGenerated(a, b, args.Length == 1 ? args[0] : "");
                reco.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;
                //reco.ContinuousRecognitionSession.AutoStopSilenceTimeout = TimeSpan.FromSeconds(3);
                try
                {
                    await Task.Delay(1000);
                    await reco.ContinuousRecognitionSession.StartAsync();
                    await Task.Delay(-1, cancellationToken);
                }
                catch (TaskCanceledException) { }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            else
            {
                Console.WriteLine("Cannot compile constraints.");
            }

            System.Diagnostics.Process.Start(Application.ExecutablePath);
        }

        private void Reco_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            Console.SetCursorPosition(0, 15);
            Console.Write("{0}       ", args.Hypothesis.Text);
        }

        private string status = "";
        private async void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            await loggingWebhook.SendMessageAsync($"Session completed: {args.Status}");
            Console.SetCursorPosition(0, 0);
            status = String.Format("Status: {0}       ", args.Status);
            Console.WriteLine("Status: {0}       ", args.Status);

            cancellationTokenSource.Cancel();
        }

        private void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args, string? arg)
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.WriteLine(status);
            Console.SetCursorPosition(0, 2);
            Console.WriteLine("Result:");
            Console.SetCursorPosition(0, 14);
            Console.WriteLine("Hypothesis:");

            if (args.Result.Text != "")
            {
                loggingWebhook.SendMessageAsync(String.Format("Detected {0} (confidence {1}%)", args.Result.Text.EscapeAsterisks(), Math.Round(args.Result.RawConfidence * 100.0d, 2)));
                Console.SetCursorPosition(0, 3);
                Console.WriteLine("[{1}%] {0}", args.Result.Text, Math.Round(args.Result.RawConfidence * 100.0d, 2));

                string pattern = @".*\*.*";
                if (Regex.IsMatch(args.Result.Text, pattern))
                {
                    var embed = new EmbedBuilder()
                    {
                        Title = "Swear word detected!"
                    }
                    .AddField("Text", args.Result.Text.EscapeAsterisks(), true)
                    .AddField("Confidence", String.Format("{0}%", Math.Round(args.Result.RawConfidence * 100.0d, 2)), true);
                    cursingWebhook.SendMessageAsync(arg is null? "@everyone" : "", embeds: new[] { embed.Build() });
                }
            }
        }

        private async Task Reco_StateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            try
            {
                //await loggingWebhook.SendMessageAsync($"State changed: {args.State}");
            } catch { }
            Console.SetCursorPosition(0, 0);
            status = String.Format("Status: {0}       ", args.State.ToString());
            Console.WriteLine("Status: {0}       ", args.State.ToString());
        }

        private Config GetConfig()
        {
            if (File.Exists(System.AppContext.BaseDirectory + "/cdConfig.yml"))
            {
                var confStr = File.ReadAllText(System.AppContext.BaseDirectory + "/cdConfig.yml");
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(PascalCaseNamingConvention.Instance)
                    .Build();
                return deserializer.Deserialize<Config>(confStr);
            }
            else
            {
                var serializer = new SerializerBuilder()
                    .WithNamingConvention(PascalCaseNamingConvention.Instance)
                    .Build();
                File.WriteAllText(System.AppContext.BaseDirectory + "/cdConfig.yml", serializer.Serialize(new Config()));
                Console.WriteLine("You must fill up the config file.");
                Console.WriteLine("A file called cdConfig.yml is generated on the directory of this program.");
                Console.WriteLine();
                Console.WriteLine("Exiting in 5 seconds...");
                Console.WriteLine();
                Thread.Sleep(5000);
                return null;
            }
        }
    }
}