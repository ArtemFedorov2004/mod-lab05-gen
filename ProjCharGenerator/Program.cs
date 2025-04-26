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

    public class WordGenerator
    {
        public record WordEntry(string Word, int Weight);

        private readonly List<WordEntry> _entries = new();

        private int _totalWeight = 0;

        private readonly Random _random = new();

        public WordGenerator(string filePath)
        {
            _entries = File.ReadLines(filePath)
                .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .Select(parts => new WordEntry(parts[1], (int)(double.Parse(parts[4]) * 10)))
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
                    return entry.Word;
            }

            return string.Empty;
        }

        public double GetWeight(string word)
        {
            var entry = _entries.FirstOrDefault(e => e.Word == word);
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
                        ActualFrequency = double.Parse(parts[1])
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
                .AutoScale();
            plt.Axes
                .SetLimitsY(0, plt.Axes.GetLimits().YRange.Max);

            plt.ShowLegend(Alignment.MiddleRight);

            plt.SavePng(bigramPlotFile, 1800, 700);
        }

        public void CreatePlot(WordGenerator wordGenerator, string wordsPlotDataFile, string wordsPlotFile)
        {
            var data = File.ReadAllLines(wordsPlotDataFile)
                .Select(line =>
                {
                    var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    return new
                    {
                        Word = parts[0],
                        ActualFrequency = double.Parse(parts[1])
                    };
                })
                .OrderBy(_ => Guid.NewGuid())
                .Take(80)
                .ToList();

            double[] actual = data.Select(x => x.ActualFrequency).ToArray();
            double[] expected = data.Select(x => wordGenerator.GetWeight(x.Word)).ToArray();
            string[] labels = data.Select(x => x.Word).ToArray();
            double[] positions = Enumerable.Range(0, labels.Length).Select(x => (double)x * 3).ToArray();

            var plt = new Plot();

            var actualBars = plt.Add.Bars(positions.Select(x => x - 0.4), actual);
            actualBars.Color = Colors.LightBlue;
            actualBars.LegendText = "Полученные значения";

            var expectedBars = plt.Add.Bars(positions.Select(x => x + 0.4).ToArray(), expected);
            expectedBars.Color = Colors.DarkOrange;
            expectedBars.LegendText = "Ожидаемые значения";

            plt.Axes.Bottom.SetTicks(positions.Select(x => x + 0.3).ToArray(), labels);
            plt.Axes.Bottom.TickLabelStyle.Rotation = 70;
            plt.Axes.Bottom.TickLabelStyle.OffsetY = 30;

            plt.XLabel("Слова", size: 20);
            plt.YLabel("Частота", size: 20);
            plt.Title("Реальная и ожидаемая частота слов (80 случайных)", size: 20);

            plt.Axes
                .AutoScale();
            plt.Axes
                .SetLimitsY(0, plt.Axes.GetLimits().YRange.Max);

            plt.ShowLegend(Alignment.MiddleRight);

            plt.SavePng(wordsPlotFile, 1800, 700);
        }
    }

    class Program
    {
        private static string _projectDirectory;

        private static PlotCreator _plotCreator;

        private static void GenerateBigramText(int textLength)
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

        private static void GenerateWordText(int textLength)
        {
            if (textLength < 1000)
            {
                throw new ArgumentOutOfRangeException("Text length must be at least 1000 characters.");
            }

            string wordsFile = Path.Combine(_projectDirectory, "words.txt");
            string outputFile = Path.Combine(_projectDirectory, 
                "..", 
                "Results", 
                "gen-2.txt"
                );

            var sb = new StringBuilder();

            var wordGenerator = new WordGenerator(wordsFile);

            for (int i = 0; i < textLength; i++)
            {
                var word = wordGenerator.GetSymbol();
                sb.Append(word + " ");
            }

            string generatedText = sb.ToString();
            File.WriteAllText(outputFile, generatedText);
            
            string wordsPlotDataFile = Path.Combine(_projectDirectory, "..", "Results", "gen-2__plot_data.txt");
            PrepareDataForWordsPlot(generatedText, wordsPlotDataFile);

            string wordsPlotFile = Path.Combine(_projectDirectory, "..", "Results", "gen-2.png");
            _plotCreator.CreatePlot(wordGenerator, wordsPlotDataFile, wordsPlotFile);
        }

        private static void PrepareDataForWordsPlot(string generatedText, string wordsPlotDataFile)
        {
            var blobData = new SortedDictionary<string, int>();

            foreach (var word in generatedText.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                blobData[word] = blobData.TryGetValue(word, out var value) ? value + 1 : 1;
            }

            var data = blobData
                .Select(kvp => $"{kvp.Key} {kvp.Value / 1000.0:F3}")
                .ToList();

            File.WriteAllLines(wordsPlotDataFile, data);
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

            GenerateWordText(textLength);
        }
    }
}
