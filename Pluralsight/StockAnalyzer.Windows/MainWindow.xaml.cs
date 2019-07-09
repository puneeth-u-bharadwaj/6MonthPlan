﻿using StockAnalyzer.Core.Domain;
using StockAnalyzer.Windows.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace StockAnalyzer.Windows
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Processing a collection of data in parallel

        CancellationTokenSource cancellationTokenSource = null;

        private async void Search_Click(object sender, RoutedEventArgs e)
        {

            #region Before loading stock data
            Stopwatch watch = new Stopwatch();
            watch.Start();
            StockProgress.Visibility = Visibility.Visible;
            StockProgress.IsIndeterminate = true;

            Search.Content = "Cancel";
            #endregion

            #region Cancellation
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource = null;
                return;
            }

            cancellationTokenSource = new CancellationTokenSource();

            cancellationTokenSource.Token.Register(() =>
            {
                Notes.Text += "Cancellation requested" + Environment.NewLine;
            });
            #endregion

            try
            {
                #region Load One or Many Tickers
                string[] tickers = Ticker.Text.Split(',', ' ');

                StockService service = new StockService();

                List<Task<IEnumerable<StockPrice>>> tickerLoadingTasks = new List<Task<IEnumerable<StockPrice>>>();
                foreach (string ticker in tickers)
                {
                    Task<IEnumerable<StockPrice>> loadTask = service.GetStockPricesFor(ticker, cancellationTokenSource.Token);

                    tickerLoadingTasks.Add(loadTask);
                }
                #endregion

                IEnumerable<StockPrice>[] loadedStocks = await Task.WhenAll(tickerLoadingTasks);

                ConcurrentBag<StockCalculation> values = new ConcurrentBag<StockCalculation>();

                ParallelLoopResult executionResult = Parallel.ForEach(loadedStocks,
                    new ParallelOptions { MaxDegreeOfParallelism = 2 },
                    (stocks, state) =>
                    {
                        string ticker = stocks.First().Ticker;

                        Debug.WriteLine($"Start processing {ticker}");

                        if (ticker == "MSFT")
                        {
                            Debug.WriteLine($"Found {ticker}, breaking");

                            state.Stop();

                            return;
                        }

                        if (state.IsStopped) return;

                        decimal result = CalculateExpensiveComputation(stocks);

                        StockCalculation data = new StockCalculation
                        {
                            Ticker = ticker,
                            Result = result
                        };

                        values.Add(data);

                        Debug.WriteLine($"Completed processing {ticker}");
                    });

                Notes.Text = $"Ran to complation: {executionResult.IsCompleted}" + Environment.NewLine;
                Notes.Text += $"Lowest break iteration: {executionResult.LowestBreakIteration}";

                Stocks.ItemsSource = values.ToArray();
            }
            catch (Exception ex)
            {
                Notes.Text += ex.Message + Environment.NewLine;
            }
            finally
            {
                cancellationTokenSource = null;
            }

            #region After stock data is loaded
            StocksStatus.Text = $"Loaded stocks for {Ticker.Text} in {watch.ElapsedMilliseconds}ms";
            StockProgress.Visibility = Visibility.Hidden;
            Search.Content = "Search";
            #endregion
        }

        #endregion

        #region Invoke operations in parallel

        //CancellationTokenSource cancellationTokenSource = null;

        //private async void Search_Click(object sender, RoutedEventArgs e)
        //{
        //    #region Before loading stock data
        //    Stopwatch watch = NewMethod();
        //    watch.Start();
        //    StockProgress.Visibility = Visibility.Visible;
        //    StockProgress.IsIndeterminate = true;

        //    Search.Content = "Cancel";
        //    #endregion

        //    #region Cancellation
        //    if (cancellationTokenSource != null)
        //    {
        //        cancellationTokenSource.Cancel();
        //        cancellationTokenSource = null;
        //        return;
        //    }

        //    cancellationTokenSource = new CancellationTokenSource();

        //    cancellationTokenSource.Token.Register(() =>
        //    {
        //        Notes.Text += "Cancellation requested" + Environment.NewLine;
        //    });
        //    #endregion

        //    try
        //    {
        //        #region Load One or Many Tickers
        //        string[] tickers = Ticker.Text.Split(',', ' ');

        //        StockService service = new StockService();

        //        List<Task<IEnumerable<StockPrice>>> tickerLoadingTasks = new List<Task<IEnumerable<StockPrice>>>();
        //        foreach (string ticker in tickers)
        //        {
        //            Task<IEnumerable<StockPrice>> loadTask = service.GetStockPricesFor(ticker, cancellationTokenSource.Token);

        //            tickerLoadingTasks.Add(loadTask);
        //        }
        //        #endregion

        //        IEnumerable<StockPrice> loadedStocks = (await Task.WhenAll(tickerLoadingTasks)).SelectMany(stock => stock);

        //        Parallel.Invoke(new ParallelOptions { CancellationToken = cancellationTokenSource.Token },
        //                            () =>
        //                            {
        //                                #region Starting
        //                                Debug.WriteLine("Starting Operation 1");
        //                                #endregion

        //                                CalculateExpensiveComputation(loadedStocks);

        //                                #region Ending
        //                                Debug.WriteLine("Completed Operation 1");
        //                                #endregion
        //                            },
        //                            () =>
        //                            {
        //                                #region Starting
        //                                Debug.WriteLine("Starting Operation 2");
        //                                #endregion

        //                                CalculateExpensiveComputation(loadedStocks);

        //                                #region Ending
        //                                Debug.WriteLine("Completed Operation 2");
        //                                #endregion
        //                            },

        //                            () =>
        //                            {
        //                                #region Starting
        //                                Debug.WriteLine("Starting Operation 3");
        //                                #endregion

        //                                CalculateExpensiveComputation(loadedStocks);

        //                                #region Ending
        //                                Debug.WriteLine("Completed Operation 3");
        //                                #endregion
        //                            },

        //                            () =>
        //                            {
        //                                #region Starting
        //                                Debug.WriteLine("Starting Operation 4");
        //                                #endregion

        //                                CalculateExpensiveComputation(loadedStocks);

        //                                #region Ending
        //                                Debug.WriteLine("Completed Operation 4");
        //                                #endregion
        //                            }
        //                        );

        //        Stocks.ItemsSource = loadedStocks;
        //    }
        //    catch (Exception ex)
        //    {
        //        Notes.Text += ex.Message + Environment.NewLine;
        //    }
        //    finally
        //    {
        //        cancellationTokenSource = null;
        //    }

        //    #region After stock data is loaded
        //    StocksStatus.Text = $"Loaded stocks for {Ticker.Text} in {watch.ElapsedMilliseconds}ms";
        //    StockProgress.Visibility = Visibility.Hidden;
        //    Search.Content = "Search";
        //    #endregion
        //}

        //private static Stopwatch NewMethod()
        //{
        //    return new Stopwatch();
        //}

        #endregion

        Random random = new Random();
        private decimal CalculateExpensiveComputation(IEnumerable<StockPrice> stocks)
        {
            Thread.Yield();

            decimal computedValue = 0m;

            foreach (var stock in stocks)
            {
                for (int i = 0; i < stocks.Count() - 2; i++)
                {
                    for (int a = 0; a < random.Next(50, 60); a++)
                    {
                        computedValue += stocks.ElementAt(i).Change + stocks.ElementAt(i + 1).Change;
                    }
                }
            }

            return computedValue;
        }

        private decimal Compute(StockPrice stock)
        {
            Thread.Yield();

            decimal x = 0;
            for (var a = 0; a < 10; a++)
            {
                for (var b = 0; b < 20; b++)
                {
                    x += a + stock.Change;
                }
            }

            return x;
        }

        private Task<List<string>> SearchForStocks(CancellationToken cancellationToken)
        {
            Task<List<string>> loadLinesTask = Task.Run(async () =>
            {
                List<string> lines = new List<string>();

                using (var stream = new StreamReader(File.OpenRead(@"C:\Code\StockData\StockPrices_small.csv")))
                {
                    string line;
                    while ((line = await stream.ReadLineAsync()) != null)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return lines;
                        }
                        lines.Add(line);
                    }
                }

                return lines;
            }, cancellationToken);

            return loadLinesTask;
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));

            e.Handled = true;
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }

    class StockCalculation
    {
        public string Ticker { get; set; }
        public decimal Result { get; set; }
    }
}
