namespace ColorCounter
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using global::ColorCounter.Interfaces;

    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Client;
    using Microsoft.ServiceFabric.Actors.Runtime;

    [StatePersistence(StatePersistence.Persisted)]
    internal class ColorCounter : Actor, IColorCounter, IRemindable
    {
        /// <summary>
        ///     Initializes a new instance of ColorCounter
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public ColorCounter(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        private static string Classify(Color color)
        {
            var hue = color.GetHue();
            var saturation = color.GetSaturation();
            var brightness = color.GetBrightness();
            if (brightness < 0.2) return "black";
            if (brightness > 0.8) return "white";
            if (saturation < 0.25) return "gray";
            if (hue < 30) return "red";
            if (hue < 90) return "yellow";
            if (hue < 150) return "green";
            if (hue < 210) return "cyan";
            if (hue < 270) return "blue";
            return hue < 330 ? "magenta" : "red";
        }

        public async Task CountPixels(string color, CancellationToken token)
        {
            var actorReminder = await this.RegisterReminderAsync(
                "countRequest",
                Encoding.ASCII.GetBytes(color),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromDays(1));
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] context, TimeSpan dueTime, TimeSpan period)
        {
            if (reminderName.Equals("countRequest"))
            {
                try
                {
                    var imageUri = await this.StateManager.TryGetStateAsync<string>("sourceImage");
                    var colorToInspect = Encoding.ASCII.GetString(context).ToLowerInvariant();
                    if (imageUri.HasValue)
                    {
                        var proxy = ActorProxy.Create<IResultAggregator>(this.Id);
                        var webClient = new WebClient();
                        var imageBytes = webClient.DownloadData(imageUri.Value);
                        var image = new Bitmap(new MemoryStream(imageBytes));
                        await proxy.TotalPixels(image.Width * image.Height);
                        for (var widthCounter = 0; widthCounter < image.Width; ++widthCounter)
                        {
                            for (var heightCounter = 0; heightCounter < image.Height; ++heightCounter)
                            {
                                var pixelColor = image.GetPixel(widthCounter, heightCounter);
                                if (Classify(pixelColor) == colorToInspect.ToLowerInvariant())
                                {
                                    await proxy.AggregateResult(colorToInspect.ToLowerInvariant(), 1);
                                }
                            }

                            Thread.Sleep(TimeSpan.FromMilliseconds(10));
                        }
                    }

                    var reminder = this.GetReminder("countRequest");
                    await this.UnregisterReminderAsync(reminder);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        public Task SetImage(Uri imageUri, CancellationToken token)
        {
            var imageLink = imageUri.ToString();
            return this.StateManager.AddOrUpdateStateAsync("sourceImage", imageLink, (key, value) => imageLink, token);
        }

        /// <summary>
        ///     This method is called whenever an actor is activated.
        ///     An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");
            this.StateManager.TryAddStateAsync("sourceImage", string.Empty);
            return this.StateManager.TryAddStateAsync("colorCounter", new Dictionary<string, long>());
        }

        /// <summary>
        /// Demonstrates a long running process.
        /// </summary>
        /// <returns>Task.</returns>
        public Task LongRunningProcess()
        {
            Thread.Sleep(TimeSpan.FromSeconds(10));
            return Task.FromResult(1);
        }
    }
}