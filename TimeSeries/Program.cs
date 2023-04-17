using MathNet.Numerics.Statistics;
using System.Reactive.Linq;

namespace TimeSeries
{
    internal class Program
    {

        private static ConsoleColor GetRandomConsoleColor(Random random)
        {
            var consoleColors = Enum.GetValues(typeof(ConsoleColor)).OfType<ConsoleColor>()
                .Where(c => c != ConsoleColor.Black)
                .ToArray(); ;
             
            return consoleColors[random.Next(consoleColors.Length)];
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, Moving Average!");

            var timeSeriesData = new List<TimeSeriesData>();

            var random = new Random();
            for (var i = 0; i < 100; i++)
            {
                timeSeriesData.Add(new TimeSeriesData
                {
                    TimeStamp = DateTime.Now.AddMinutes(i),
                    Value = random.Next(0, 1000)
                });
            }

            const int windowSize = 3;


            IObservable<double> movingAverageStream = timeSeriesData
                .ToObservable()
                .Buffer(windowSize)
                .Select(windowData =>
                {
                    double[] values = windowData.Select(t => t.Value).ToArray();
                    return values.Mean();
                });

            // Use TimeInterval to introduce a 1-second delay between each Console.WriteLine
            IObservable<double> delayedMovingAverageStream = movingAverageStream
                .TimeInterval()
                .Select(_ => Observable.Timer(TimeSpan.FromMilliseconds(200)))
                .Concat()
                .Zip(movingAverageStream, (_, average) => average);

            // Create a countdown event to wait for the completion of the delayed stream
            CountdownEvent countdownEvent = new CountdownEvent(1);

            // Subscribe to the delayed moving average stream and print the results
            IDisposable subscription = delayedMovingAverageStream.Subscribe(
                average =>
                {
                    ConsoleColor originalColor = Console.ForegroundColor;
                    Console.ForegroundColor = GetRandomConsoleColor(random);
                    Console.WriteLine(average);
                    Console.ForegroundColor = originalColor;
                },
                () => countdownEvent.Signal());

            // Wait for the completion of the delayed stream
            countdownEvent.Wait();

            // Dispose of the subscription when you're done
            subscription.Dispose();


            //IObservable<double> movingAverageStream = timeSeriesData
            //    .ToObservable()
            //    .Buffer(windowSize)
            //    .Select(windowData =>
            //    {
            //        double[] values = windowData.Select(t => t.Value).ToArray();
            //        return values.Mean();
            //    })
            //    .Delay(TimeSpan.FromSeconds(1));

            //CountdownEvent countdownEvent = new CountdownEvent(1);

            //// Subscribe to the delayed moving average stream and print the results
            //IDisposable subscription = movingAverageStream.Subscribe(
            //    average => Console.WriteLine(average),
            //    () =>
            //    {
            //        countdownEvent.Signal();
            //        Console.WriteLine("Done");
            //    });

            //// Wait for the completion of the delayed stream
            //countdownEvent.Wait();

            //// Dispose of the subscription when you're done
            //subscription.Dispose();

            //// Create an observable from the time series data
            //IObservable<TimeSeriesData> timeSeriesDataStream = timeSeriesData.ToObservable();

            //// Buffer the data into windows of the specified size
            //IObservable<IList<TimeSeriesData>> bufferedDataStream = timeSeriesDataStream.Buffer(windowSize);

            //// Calculate the moving average for each buffered window
            //IObservable<double> movingAverageStream = bufferedDataStream.Select(windowData =>
            //{
            //    double[] values = windowData.Select(t => t.Value).ToArray();
            //    return values.Mean();
            //});

            //// Introduce a 1-second delay between each moving average output
            //IObservable<double> delayedMovingAverageStream = movingAverageStream.Delay(TimeSpan.FromSeconds(1));

            //// Create a countdown event to wait for the completion of the delayed stream
            //CountdownEvent countdownEvent = new CountdownEvent(1);

            //// Subscribe to the delayed moving average stream and print the results
            //IDisposable subscription = delayedMovingAverageStream.Subscribe(
            //    average => Console.WriteLine(average),
            //    () => countdownEvent.Signal());

            //// Wait for the completion of the delayed stream
            //countdownEvent.Wait();

            //// Dispose of the subscription when you're done
            //subscription.Dispose();


            //var movingAverages = MovingAverage(timeSeriesData, windowSize);
            //foreach (var average in movingAverages)
            //{
            //    Console.WriteLine(average);
            //}
        }

        //public static IEnumerable<double> MovingAverage(IReadOnlyList<TimeSeriesData> timeSeriesData, int windowSize)
        //{
        //    if (windowSize <= 0)
        //    {
        //        throw new ArgumentException("Window size must be greater than 0");
        //    }

        //    double[] values = timeSeriesData.Select(t => t.Value).ToArray();
        //    var movingAverage = new List<double>(values.Length - windowSize + 1);

        //    for (int i = 0; i < values.Length - windowSize + 1; i++)
        //    {
        //        double average = values[i..(i + windowSize)].Mean();
        //        movingAverage.Add(average);
        //    }
        //    return movingAverage;
        //}
    }

    public class TimeSeriesData
    {
        public DateTime TimeStamp { get; set; }
        public double Value { get; set; }
    }
}