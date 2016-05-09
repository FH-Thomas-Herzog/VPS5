using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Quandl.API;

namespace Quandl.UI
{
    public partial class QuandlViewer : Form
    {
        public event EventHandler<IList<Series>> DataLoaded;

        private QuandlService service;
        private readonly string[] names = { "NASDAQ_MSFT", "NASDAQ_AAPL", "NASDAQ_GOOG" };
        private const int INTERVAL = 2000;

        public QuandlViewer()
        {
            InitializeComponent();
            service = new QuandlService();
            DataLoaded += OnDataLoaded;
        }

        private async void displayButton_Click(object sender, EventArgs e)
        {
            // clear former displayed serieses [for testing]
            chart.Series.Clear();

            //SequentialImplementation();

            // Parallel implementation
            //displayButton_Click_parallel(sender, e);

            // Async await implementation
            displayButton_Click_async(sender, e);
        }

        #region Sequential Implementation
        private void SequentialImplementation()
        {
            List<Series> seriesList = new List<Series>();

            foreach (var name in names)
            {
                StockData sd = RetrieveStockData(name);
                List<StockValue> values = sd.GetValues();
                seriesList.Add(GetSeries(values, name));
                seriesList.Add(GetTrend(values, name));
            }

            DisplayData(seriesList);
            SaveImage("chart");
        }
        private StockData RetrieveStockData(string name)
        {
            return service.GetData(name);
        }

        private Series GetSeries(List<StockValue> stockValues, string name)
        {
            Series series = new Series(name);
            series.ChartType = SeriesChartType.FastLine;

            int j = 0;
            for (int i = stockValues.Count - INTERVAL; i < stockValues.Count; i++)
            {
                series.Points.Add(new DataPoint(j++, stockValues[i].Close));
            }
            return series;
        }


        private Series GetTrend(List<StockValue> stockValues, string name)
        {
            double k, d;
            Series series = new Series(name + " Trend");
            series.ChartType = SeriesChartType.FastLine;

            var vals = stockValues.Select(x => x.Close).ToArray();
            LinearLeastSquaresFitting.Calculate(vals, out k, out d);

            int j = 0;
            for (int i = stockValues.Count - INTERVAL; i < stockValues.Count; i++)
            {
                series.Points.Add(new DataPoint(j++, k * i + d));
            }
            return series;
        }
        #endregion

        #region Parallel Implementation
        /// <summary>
        /// Button click handler for the parallel implementation.
        /// Here we update the Ui via an event handler, because this method will finish right after the task hqas started.
        /// </summary>
        /// <param name="sender">the sender of the event</param>
        /// <param name="e">the event arguments</param>
        private void displayButton_Click_parallel(object sender, EventArgs e)
        {
            Task.Run(() => ParallelImplementation());
        }

        public void OnDataLoaded(object sender, IList<Series> allSerieses)
        {
            // If invoke is required, because here we could be on another thread
            if (InvokeRequired)
            {
                Invoke(new EventHandler<List<Series>>(OnDataLoaded), sender, allSerieses);
            }
            // if on the same thread
            else
            {
                DisplayData(allSerieses);
                SaveImage("chart");
            }
        }

        /// <summary>
        /// Implementation for parallel load of data.
        /// </summary>
        private void ParallelImplementation()
        {
            // list which holds all runnint main tasks
            IList<Task<IList<Series>>> mainTasks = new List<Task<IList<Series>>>(names.Length);

            foreach (var name in names)
            {
                // preserve name
                var copiedName = name;
                // create main task
                var task = Task.Run(() =>
                {
                    IList<Series> seriesList = new List<Series>();
                    var data = RetrieveStockData(copiedName);

                    // run inner tasks
                    var seriesTask = Task.Run(() => GetSeries(data.GetValues(), copiedName));
                    var trendTask = Task.Run(() => GetTrend(data.GetValues(), copiedName));

                    // wait for all inner tasks
                    Task.WaitAll(seriesTask, trendTask);

                    // collect data of inner tasks
                    seriesList.Add(seriesTask.Result);
                    seriesList.Add(trendTask.Result);

                    // return result of main taks
                    return seriesList;
                });

                // remember main task
                mainTasks.Add(task);
            }

            // wait for all main tasks
            var result = Task.WhenAll(mainTasks.ToArray());

            // collect and merge results

            // fire event for UI
            DataLoaded?.Invoke(this, MergeResults(result.Result));
        }
        #endregion

        #region Async Await implementation
        /// <summary>
        /// Button click handler for the async await implementation.
        /// Here we have no need for a event handler because await will processed on the UI thread if the result is ready to be processed.
        /// </summary>
        /// <param name="sender">the sender of the click event</param>
        /// <param name="e">the event arguments</param>
        private async void displayButton_Click_async(object sender, EventArgs e)
        {
            DisplayData(MergeResults(await ParallelImplementationAsync()));
            SaveImage("chart");
        }

        private async Task<IList<Series>[]> ParallelImplementationAsync()
        {
            // list which holds all runnint main tasks
            IList<Task<List<Series>>> mainTasks = new List<Task<List<Series>>>(names.Length);

            foreach (var name in names)
            {
                var task = LoadAsync(name);

                // remember main task
                mainTasks.Add(task);
            }

            // wait for all main tasks
            return await Task.WhenAll(mainTasks.ToArray());
        }

        private async Task<List<Series>> LoadAsync(String name)
        {
            return await Task.Run(async () =>
            {
                var seriesList = new List<Series>();
                var data = await RetrieveStockDataAsync(name);

                // run inner tasks
                var seriesTask = GetSeriesAsync(data.GetValues(), name);
                var trendTask = GetTrendAsync(data.GetValues(), name);

                // wait for all inner tasks
                var result = await Task.WhenAll(seriesTask, trendTask);

                // collect data of inner tasks
                seriesList.Add(result[0]);
                seriesList.Add(result[1]);

                // return result of main taks
                return seriesList;
            });
        }

        private async Task<StockData> RetrieveStockDataAsync(string name)
        {
            return await Task.Run(() => service.GetData(name));
        }

        private async Task<Series> GetSeriesAsync(List<StockValue> stockValues, string name)
        {
            return await Task.Run(() => GetSeries(stockValues, name));
        }

        private async Task<Series> GetTrendAsync(List<StockValue> stockValues, string name)
        {
            return await Task.Run(() => GetTrend(stockValues, name));
        }
        #endregion

        #region Helper Methods
        private void DisplayData(IList<Series> seriesList)
        {
            chart.Series.Clear();
            foreach (Series series in seriesList)
            {
                chart.Series.Add(series);
            }
        }

        private void SaveImage(string fileName)
        {
            chart.SaveImage(fileName + ".jpg", ChartImageFormat.Jpeg);
        }

        /// <summary>
        /// Helper for merging the series to a single list
        /// </summary>
        /// <param name="seriesArray">the array of series lists</param>
        /// <returns>the merged list</returns>
        private IList<Series> MergeResults(IList<Series>[] seriesArray)
        {
            List<Series> allSerieses = new List<Series>();
            foreach (var list in seriesArray)
            {
                allSerieses.AddRange(list);
            }
            return allSerieses;
        }
        #endregion
    }
}
