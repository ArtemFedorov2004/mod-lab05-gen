using System.Text;

namespace ProjCharGenerator.Tests
{
    public class UnitTests : IDisposable
    {

        private readonly string _projectDirectory;

        private readonly string _mockBigramsFile;

        private string _mockWordsFile;

        public UnitTests()
        {
            _projectDirectory = Directory.GetParent(Environment.CurrentDirectory)
                .Parent?
                .Parent?
                .FullName;

            _mockBigramsFile = Path.Combine(_projectDirectory, "mock_bigrams.txt");

            string bigramsContent = "1 аа 1\n" +
                                 "2 аб 2\n" +
                                 "3 ав 3\n" +
                                 "4 аг 4";
            File.WriteAllText(_mockBigramsFile, bigramsContent);

            _mockWordsFile = Path.Combine(_projectDirectory, "test_words.txt");

            string wordsContent = "1 €блоко €блоко €блоко 0,5\n" +
                                 "2 банан банан банан 0,3\n" +
                                 "3 апельсин апельсин апельсин 0,2";

            File.WriteAllText(_mockWordsFile, wordsContent);
        }

        public void Dispose()
        {
            if (File.Exists(_mockBigramsFile))
            {
                File.Delete(_mockBigramsFile);
            }

            if (File.Exists(_mockWordsFile))
            {
                File.Delete(_mockWordsFile);
            }
        }

        [Fact]
        public void BigramGenerator_ShouldReturnSymbol_WhenCalled()
        {
            var generator = new BigramGenerator(_mockBigramsFile);

            var symbol = generator.GetSymbol();

            Assert.Contains(symbol, new[] { "аа", "аб", "ав", "аг" });
        }

        [Fact]
        public void BigramGenerator_ShouldReturnCorrectWeight_WhenSymbolIsGiven1()
        {
            var generator = new BigramGenerator(_mockBigramsFile);

            var weight = generator.GetWeight("аа");


            Assert.Equal(1.0 / 10, weight);
        }

        [Fact]
        public void BigramGenerator_ShouldThrowException_WhenFileDoesNotExist()
        {
            var nonExistentFilePath = Path.Combine(Directory.GetCurrentDirectory(), "non_existent_file.txt");

            Assert.Throws<FileNotFoundException>(() => new BigramGenerator(nonExistentFilePath));
        }

        [Fact]
        public void BigramGenerator_ShouldGenerateText_WhenCalledMultipleTimes()
        {
            var generator = new BigramGenerator(_mockBigramsFile);
            var sb = new StringBuilder();

            for (int i = 0; i < 1000; i++)
            {
                sb.Append(generator.GetSymbol());
            }

            string generatedText = sb.ToString();

            Assert.Equal(2000, generatedText.Length);
        }

        [Fact]
        public void BigramGenerator_ShouldGenerateText_WhenCalledMultipleTimes2()
        {
            var generator = new BigramGenerator(_mockBigramsFile);
            var sb = new StringBuilder();

            for (int i = 0; i < 1500; i++)
            {
                sb.Append(generator.GetSymbol());
            }

            string generatedText = sb.ToString();

            Assert.Equal(3000, generatedText.Length);
        }

        [Fact]
        public void BigramGenerator_ShouldGenerateText_WhenCalledMultipleTimes3()
        {
            var generator = new BigramGenerator(_mockBigramsFile);
            var sb = new StringBuilder();

            for (int i = 0; i < 2000; i++)
            {
                sb.Append(generator.GetSymbol());
            }

            string generatedText = sb.ToString();

            Assert.Equal(4000, generatedText.Length);
        }

        [Fact]
        public void WordGenerator_ShouldReturnSymbol_WhenCalled()
        {
            var generator = new WordGenerator(_mockWordsFile);

            var symbol = generator.GetSymbol();

            Assert.Contains(symbol, new[] { "€блоко", "банан", "апельсин" });
        }

        [Fact]
        public void WordGenerator_ShouldReturnCorrectWeight_WhenWordIsGiven1()
        {
            var generator = new WordGenerator(_mockWordsFile);

            var weight = generator.GetWeight("€блоко");

            Assert.Equal(0.5, weight);
        }

        [Fact]
        public void WordGenerator_ShouldReturnCorrectWeight_WhenWordIsGiven2()
        {
            var generator = new WordGenerator(_mockWordsFile);

            var weight = generator.GetWeight("банан");

            Assert.Equal(0.3, weight);
        }

        [Fact]
        public void WordGenerator_ShouldReturnCorrectWeight_WhenWordIsGiven3()
        {
            var generator = new WordGenerator(_mockWordsFile);

            var weight = generator.GetWeight("апельсин");

            Assert.Equal(0.2, weight);
        }

        [Fact]
        public void WordGenerator_ShouldThrowException_WhenFileDoesNotExist()
        {
            var nonExistentFilePath = Path.Combine(Directory.GetCurrentDirectory(), "non_existent_file.txt");

            Assert.Throws<FileNotFoundException>(() => new WordGenerator(nonExistentFilePath));
        }

        [Fact]
        public void WordGenerator_ShouldCorrectlySumWeights()
        {
            var generator = new WordGenerator(_mockWordsFile);

            double totalWeight = generator.GetWeight("€блоко") + generator.GetWeight("банан") + generator.GetWeight("апельсин");

            Assert.Equal(1.0, totalWeight, 3);
        }

        [Fact]
        public void WordGenerator_ShouldReturnCorrectSymbolDistribution()
        {
            var generator = new WordGenerator(_mockWordsFile);
            int appleCount = 0;
            int bananaCount = 0;
            int orangeCount = 0;
            int totalCount = 10000;

            for (int i = 0; i < totalCount; i++)
            {
                var symbol = generator.GetSymbol();
                if (symbol == "€блоко") appleCount++;
                if (symbol == "банан") bananaCount++;
                if (symbol == "апельсин") orangeCount++;
            }

            Assert.InRange((double)appleCount / totalCount, 0.48, 0.52);
            Assert.InRange((double)bananaCount / totalCount, 0.28, 0.32);
            Assert.InRange((double)orangeCount / totalCount, 0.18, 0.22);
        }
    }
}