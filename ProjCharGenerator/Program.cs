using ScottPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Generator
{
    public class BigramGenerator
    {
        public record BigramEntry(string Symbol, int Weight);

        private readonly List<BigramEntry> _entries = new();

        private int _totalWeight = 0;

        private readonly Random _random = new();

        public BigramGenerator(string filePath)
        {
            _entries = File.ReadLines(filePath)
                   .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                   .Select(parts => new BigramEntry(parts[1], int.Parse(parts[2])))
                   .ToList();

            _totalWeight = _entries.Sum(e => e.Weight);
        }

        public string GetSymbol()
        {
            int cumulative = 0;
            int target = _random.Next(_totalWeight);

            foreach (var entry in _entries)
            {
                cumulative += entry.Weight;
                if (target < cumulative)
                    return entry.Symbol;
            }

            return string.Empty;
        }

        public double GetWeight(string bigram)
        {
            var entry = _entries.FirstOrDefault(e => e.Symbol == bigram);
            return (double)entry.Weight / _totalWeight;
        }
    }

    public class PlotCreator
    {
        public void CreatePlot(BigramGenerator bigramGenerator, string bigramPlotDataFile, string bigramPlotFile)
        {
            var data = File.ReadAllLines(bigramPlotDataFile)                
                .Select(line =>
                {
                    var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    return new
                    {
                        Bigram = parts[0],
                        ActualFrequency = double.Parse(parts[1].Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture)
                    };
                })
                .OrderBy(x => Guid.NewGuid())
                .Take(80)
                .ToList();

            double[] actual = data.Select(x => x.ActualFrequency)
                .ToArray();

            double[] expected = data.Select(x => bigramGenerator.GetWeight(x.Bigram))
                .ToArray();

            string[] labels = data.Select(x => x.Bigram)
                .ToArray();

            double[] positions = Enumerable.Range(0, labels.Length)
                .Select(x => (double)x * 3)
                .ToArray();            

            Plot plt = new();

            var actualBars = plt.Add
                .Bars(positions.Select(x => x - 0.4), actual);
            actualBars.Color = Colors.LightGreen;
            actualBars.LegendText = "Полученные значения";

            var expectedBars = plt.Add
                .Bars(positions.Select(x => x + 0.4).ToArray(), expected);
            expectedBars.Color = Colors.DarkRed;
            expectedBars.LegendText = "Ожидаемые значения";

            plt.Axes
                .Bottom
                .SetTicks(positions.Select(x => x + 0.3).ToArray(), labels);
            
            plt.XLabel(" ", size: 20);
            plt.YLabel("Частота", size: 20);
            plt.Title("Реальная и ожидаемая частота биграмм (80 значений для примера)", size: 20);
                        
            plt.Axes
                .SetLimitsY(0, plt.Axes.GetLimits().YRange.Max);
            plt.Axes
                .AutoScale();

            plt.ShowLegend(Alignment.MiddleRight);

            plt.SavePng(bigramPlotFile, 1800, 700);
        }
    }

    class Program
    {
        private static string _projectDirectory;

        private static PlotCreator _plotCreator;

        static void GenerateBigramText(int textLength)
        {
            if (textLength < 1000)
            {
                throw new ArgumentOutOfRangeException("Text length must be at least 1000 characters.");
            }

            string bigramsFile = Path.Combine(_projectDirectory, "bigrams.txt");
            string outputFile = Path.Combine(_projectDirectory, 
                "..", 
                "Results", 
                "gen-1.txt"
                );

            var sb = new StringBuilder();

            var bigramGenerator = new BigramGenerator(bigramsFile);
            
            for (int i = 0; i < textLength; i++)
            {
                var symbol = bigramGenerator.GetSymbol();
                sb.Append(symbol);
            }

            string generatedText = sb.ToString();
            File.WriteAllText(outputFile, generatedText);

            string bigramPlotDataFile = Path.Combine(_projectDirectory, "..", "Results", "gen-1__plot_data.txt");
            PrepareDataForBigramPlot(generatedText, bigramPlotDataFile);
            
            string bigramPlotFile = Path.Combine(_projectDirectory, "..", "Results", "gen-1.png");
            _plotCreator.CreatePlot(bigramGenerator, bigramPlotDataFile, bigramPlotFile);
        }

        private static void PrepareDataForBigramPlot(string generatedText, string bigramPlotDataFile)
        {
            var blobData = new SortedDictionary<string, int>();

            foreach (var i in Enumerable.Range(0, generatedText.Length / 2))
            {
                string symbol = generatedText.Substring(i * 2, 2);

                blobData[symbol] = blobData.TryGetValue(symbol, out var count) ? count + 1 : 1;
            }

            var data = blobData
                .Select(kvp => $"{kvp.Key} {kvp.Value / 1000f:F3}")
                .ToList();

            File.WriteAllLines(bigramPlotDataFile, data);
        }

        static void Main()
        {
            _projectDirectory = Directory.GetParent(Environment.CurrentDirectory)
                .Parent?
                .Parent?
                .FullName;
            _plotCreator = new PlotCreator();
            const int textLength = 1000;

            GenerateBigramText(textLength);
        }
    }
}
