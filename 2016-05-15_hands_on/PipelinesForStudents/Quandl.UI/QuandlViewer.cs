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
        public event EventHandler<List<Series>> DataLoaded;

        private QuandlService service;
        private readonly string[] names = { "NASDAQ_MSFT", "NASDAQ_AAPL", "NASDAQ_GOOG" };
        private const int INTERVAL = 2000;

        public QuandlViewer()
        {
            InitializeComponent();
            service = new QuandlService();
            DataLoaded += OnDataLoaded;
        }

        private void displayButton_Click(object sender, EventArgs e)
        {
            // clear former displayed serieses [for testing]
            chart.Series.Clear();
            //SequentialImplementation();
            Task.Run(() => ParallelImplementation());
        }

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

        public void OnDataLoaded(object sender, List<Series> allSerieses)
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
            IList<Task<List<Series>>> mainTasks = new List<Task<List<Series>>>(names.Length);

            foreach (var name in names)
            {
                // preserve name
                var copiedName = name;
                // create main task
                var task = Task.Run(() =>
                {
                    var seriesList = new List<Series>();
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
            Task.WaitAll(mainTasks.ToArray());

            // collect and merge results
            List<Series> allSerieses = new List<Series>();
            foreach (var task in mainTasks)
            {
                allSerieses.AddRange(task.Result);
            }

            // fire event for UI
            DataLoaded?.Invoke(this, allSerieses);
        }

        private Task<StockData> RetrieveStockDataAsync(String name)
        {
            return null;
        }

        private StockData RetrieveStockData(string name)
        {
            return service.GetData(name);
        }

        private Task<Series> GetSeriesAsync(List<StockValue> stockValues, string name)
        {
            return null;
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

        private Task<Series> GetTrendAsync(List<StockValue> stockValues, string name)
        {
            return null;
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

        private void DisplayData(List<Series> seriesList)
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
    }
}
