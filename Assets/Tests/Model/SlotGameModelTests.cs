using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using _Scripts.Model;
using Configs.Interfaces;
using Cysharp.Threading.Tasks;

namespace _Scripts.Tests.Model
{
    [TestFixture]
    public class SlotGameModelTests
    {
        private Mock<ISlotConfiguration> _mockConfig;
        private Mock<IRandomProvider> _mockRandomProvider;
        private SlotGameModel _slotGameModel;
        
        private List<ISlotSymbol> _testSymbols;
        private List<IReadOnlyList<int>> _testPaylines;
        
        private const int TestReelCount = 5;
        private const int TestRowCount = 3;
        private const int TestMinMatchCount = 3;

        [SetUp]
        public void SetUp()
        {
            // Arrange - Create test data
            _testSymbols = CreateTestSymbols();
            _testPaylines = CreateTestPaylines();
            
            // Arrange - Setup mocks
            _mockConfig = new Mock<ISlotConfiguration>();
            _mockRandomProvider = new Mock<IRandomProvider>();
            
            SetupConfigurationMock();
            
            // Arrange - Create system under test
            _slotGameModel = new SlotGameModel(_mockConfig.Object, _mockRandomProvider.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _slotGameModel = null;
            _mockConfig = null;
            _mockRandomProvider = null;
        }

        #region Helper Methods for Task Handling

        private T GetTaskResult<T>(UniTask<T> uniTask)
        {
            return uniTask.AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private void WaitForTask(UniTask uniTask)
        {
            uniTask.AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        #endregion

        #region Constructor Tests

        [Test]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Act & Assert
            Assert.That(_slotGameModel, Is.Not.Null);
            Assert.That(_slotGameModel.CanSpin, Is.True);
        }

        [Test]
        public void Constructor_WithNullConfiguration_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new SlotGameModel(null, _mockRandomProvider.Object));
            
            Assert.That(exception.ParamName, Is.EqualTo("config"));
        }

        [Test]
        public void Constructor_WithNullRandomProvider_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new SlotGameModel(_mockConfig.Object, null));
            
            Assert.That(exception.ParamName, Is.EqualTo("randomProvider"));
        }

        #endregion

        #region Initialize Tests

        [Test]
        public void Initialize_ShouldSetCanSpinToTrue()
        {
            // Arrange
            _slotGameModel = new SlotGameModel(_mockConfig.Object, _mockRandomProvider.Object);
            
            // Act
            _slotGameModel.Initialize();
            
            // Assert
            Assert.That(_slotGameModel.CanSpin, Is.True);
        }

        #endregion

        #region PerformSpinAsync Tests

        [Test]
        public void PerformSpinAsync_WhenCanSpin_ShouldReturnValidSpinResult()
        {
            // Arrange
            _slotGameModel.Initialize();
            SetupRandomProviderForWinningResult();

            // Act
            var task = _slotGameModel.PerformSpinAsync();
            var result = GetTaskResult(task);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ReelResults, Is.Not.Null);
            Assert.That(result.ReelResults.GetLength(0), Is.EqualTo(TestReelCount));
            Assert.That(result.ReelResults.GetLength(1), Is.EqualTo(TestRowCount));
            Assert.That(result.WinningPaylines, Is.Not.Null);
        }

        [Test]
        public void PerformSpinAsync_ShouldTriggerOnSpinStartedEvent()
        {
            // Arrange
            _slotGameModel.Initialize();
            var eventTriggered = false;
            _slotGameModel.OnSpinStarted += () => eventTriggered = true;
            SetupRandomProviderForWinningResult();

            // Act
            var task = _slotGameModel.PerformSpinAsync();
            WaitForTask(task);

            // Assert
            Assert.That(eventTriggered, Is.True);
        }

        [Test]
        public void PerformSpinAsync_ShouldTriggerOnSpinCompletedEvent()
        {
            // Arrange
            _slotGameModel.Initialize();
            ISpinResult receivedResult = null;
            _slotGameModel.OnSpinCompleted += result => receivedResult = result;
            SetupRandomProviderForWinningResult();

            // Act
            var task = _slotGameModel.PerformSpinAsync();
            var actualResult = GetTaskResult(task);

            // Assert
            Assert.That(receivedResult, Is.Not.Null);
            Assert.That(receivedResult, Is.EqualTo(actualResult));
        }

        [Test]
        public void PerformSpinAsync_WhenSpinning_ShouldSetCanSpinToFalse()
        {
            // Arrange
            _slotGameModel.Initialize();
            SetupRandomProviderForWinningResult();
            var canSpinDuringSpin = true;
            
            _slotGameModel.OnSpinStarted += () => canSpinDuringSpin = _slotGameModel.CanSpin;

            // Act
            var task = _slotGameModel.PerformSpinAsync();
            WaitForTask(task);

            // Assert
            Assert.That(canSpinDuringSpin, Is.False);
            Assert.That(_slotGameModel.CanSpin, Is.True); // Should be true again after spin completes
        }
        

        [Test]
        public void PerformSpinAsync_AfterException_ShouldRestoreCanSpinToTrue()
        {
            // Arrange
            _slotGameModel.Initialize();
            _mockRandomProvider.Setup(x => x.Next(It.IsAny<int>()))
                              .Throws(new InvalidOperationException("Test exception"));

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                var task = _slotGameModel.PerformSpinAsync();
                GetTaskResult(task);
            });
            
            // Assert
            Assert.That(exception.Message, Is.EqualTo("Test exception"));
            Assert.That(_slotGameModel.CanSpin, Is.True);
        }

        #endregion

        #region Spin Result Generation Tests

        [Test]
        public void PerformSpinAsync_ShouldGenerateCorrectReelDimensions()
        {
            // Arrange
            _slotGameModel.Initialize();
            SetupRandomProviderForNoWin();

            // Act
            var task = _slotGameModel.PerformSpinAsync();
            var result = GetTaskResult(task);

            // Assert
            Assert.That(result.ReelResults.GetLength(0), Is.EqualTo(TestReelCount));
            Assert.That(result.ReelResults.GetLength(1), Is.EqualTo(TestRowCount));
        }

        [Test]
        public void PerformSpinAsync_WithWinningCombination_ShouldReturnWinningResult()
        {
            // Arrange
            _slotGameModel.Initialize();
            SetupRandomProviderForWinningResult();

            // Act
            var task = _slotGameModel.PerformSpinAsync();
            var result = GetTaskResult(task);

            // Assert
            Assert.That(result.IsWin, Is.True);
            Assert.That(result.TotalPayout, Is.GreaterThan(0));
            Assert.That(result.WinningPaylines.Count, Is.GreaterThan(0));
        }

        [Test]
        public void PerformSpinAsync_WithNoWinningCombination_ShouldReturnLosingResult()
        {
            // Arrange
            _slotGameModel.Initialize();
            SetupRandomProviderForNoWin();

            // Act
            var task = _slotGameModel.PerformSpinAsync();
            var result = GetTaskResult(task);

            // Assert
            Assert.That(result.IsWin, Is.False);
            Assert.That(result.TotalPayout, Is.EqualTo(0));
            Assert.That(result.WinningPaylines.Count, Is.EqualTo(0));
        }

        [Test]
        public void PerformSpinAsync_ShouldUseAllConfiguredSymbols()
        {
            // Arrange
            _slotGameModel.Initialize();
            var sequence = 0;
            _mockRandomProvider.Setup(x => x.Next(It.IsAny<int>()))
                              .Returns(() => sequence++ % _testSymbols.Count);

            // Act
            var task = _slotGameModel.PerformSpinAsync();
            var result = GetTaskResult(task);

            // Assert
            var usedSymbolIds = new HashSet<int>();
            for (int reel = 0; reel < TestReelCount; reel++)
            {
                for (int row = 0; row < TestRowCount; row++)
                {
                    usedSymbolIds.Add(result.ReelResults[reel, row].Id);
                }
            }

            Assert.That(usedSymbolIds.Count, Is.EqualTo(_testSymbols.Count));
        }

        #endregion

        #region Event Tests

        [Test]
        public void Events_ShouldBeTriggeredInCorrectOrder()
        {
            // Arrange
            _slotGameModel.Initialize();
            SetupRandomProviderForWinningResult();
            
            var events = new List<string>();
            _slotGameModel.OnSpinStarted += () => events.Add("Started");
            _slotGameModel.OnSpinCompleted += _ => events.Add("Completed");

            // Act
            var task = _slotGameModel.PerformSpinAsync();
            WaitForTask(task);

            // Assert
            Assert.That(events.Count, Is.EqualTo(2));
            Assert.That(events[0], Is.EqualTo("Started"));
            Assert.That(events[1], Is.EqualTo("Completed"));
        }

        [Test]
        public void OnSpinCompleted_ShouldReceiveCorrectSpinResult()
        {
            // Arrange
            _slotGameModel.Initialize();
            SetupRandomProviderForWinningResult();
            
            ISpinResult eventResult = null;
            _slotGameModel.OnSpinCompleted += result => eventResult = result;

            // Act
            var task = _slotGameModel.PerformSpinAsync();
            var methodResult = GetTaskResult(task);

            // Assert
            Assert.That(eventResult, Is.Not.Null);
            Assert.That(ReferenceEquals(eventResult, methodResult), Is.True);
        }

        #endregion

        #region Property Tests

        [Test]
        public void CanSpin_InitialState_ShouldBeTrue()
        {
            // Act & Assert
            Assert.That(_slotGameModel.CanSpin, Is.True);
        }

        [Test]
        public void CanSpin_AfterInitialize_ShouldBeTrue()
        {
            // Act
            _slotGameModel.Initialize();

            // Assert
            Assert.That(_slotGameModel.CanSpin, Is.True);
        }

        #endregion

        #region Integration Tests

        [Test]
        public void PerformMultipleSpins_Sequentially_ShouldWorkCorrectly()
        {
            // Arrange
            _slotGameModel.Initialize();
            SetupRandomProviderForVariableResults();
            const int spinCount = 5;

            // Act & Assert
            for (int i = 0; i < spinCount; i++)
            {
                var task = _slotGameModel.PerformSpinAsync();
                var result = GetTaskResult(task);
                
                Assert.That(result, Is.Not.Null);
                Assert.That(_slotGameModel.CanSpin, Is.True);
            }
        }

        [Test]
        public void PerformSpinAsync_WithAllPaylines_ShouldEvaluateAllPaylines()
        {
            // Arrange
            _slotGameModel.Initialize();
            SetupRandomProviderForMultipleWins();

            // Act
            var task = _slotGameModel.PerformSpinAsync();
            var result = GetTaskResult(task);

            // Assert
            Assert.That(result.WinningPaylines.Count, Is.LessThanOrEqualTo(_testPaylines.Count));
            
            // Verify that payline indices are within valid range
            foreach (var paylineResult in result.WinningPaylines)
            {
                Assert.That(paylineResult.PaylineIndex, Is.InRange(0, _testPaylines.Count - 1));
            }
        }

        #endregion

        #region Test Data Creation Methods

        private void SetupConfigurationMock()
        {
            _mockConfig.Setup(x => x.ReelCount).Returns(TestReelCount);
            _mockConfig.Setup(x => x.RowCount).Returns(TestRowCount);
            _mockConfig.Setup(x => x.MinMatchCount).Returns(TestMinMatchCount);
            _mockConfig.Setup(x => x.Symbols).Returns(_testSymbols);
            _mockConfig.Setup(x => x.Paylines).Returns(_testPaylines);
        }

        private List<ISlotSymbol> CreateTestSymbols()
        {
            return new List<ISlotSymbol>
            {
                new SlotSymbol(0, "Cherry", 10),
                new SlotSymbol(1, "Lemon", 15),
                new SlotSymbol(2, "Orange", 20),
                new SlotSymbol(3, "Bell", 25),
                new SlotSymbol(4, "Seven", 50)
            };
        }

        private List<IReadOnlyList<int>> CreateTestPaylines()
        {
            return new List<IReadOnlyList<int>>
            {
                new List<int> { 5, 6, 7, 8, 9 },    // Middle row
                new List<int> { 0, 1, 2, 3, 4 },    // Top row
                new List<int> { 10, 11, 12, 13, 14 }, // Bottom row
                new List<int> { 0, 6, 12, 8, 4 },   // Diagonal
                new List<int> { 10, 6, 2, 8, 14 }   // Diagonal
            };
        }

        private void SetupRandomProviderForWinningResult()
        {
            // Returns same symbol (index 0) for winning combination
            _mockRandomProvider.Setup(x => x.Next(It.IsAny<int>())).Returns(0);
        }

        private void SetupRandomProviderForNoWin()
        {
            // Returns alternating symbols to avoid wins
            var sequence = 0;
            _mockRandomProvider.Setup(x => x.Next(It.IsAny<int>()))
                              .Returns(() => sequence++ % _testSymbols.Count);
        }

        private void SetupRandomProviderForMinimumWin()
        {
            // Returns same symbol for first 3 positions, then different
            var callCount = 0;
            _mockRandomProvider.Setup(x => x.Next(It.IsAny<int>()))
                              .Returns(() => callCount++ < TestMinMatchCount ? 0 : 1);
        }

        private void SetupRandomProviderForInsufficientMatch()
        {
            // Returns same symbol for first 2 positions only
            var callCount = 0;
            _mockRandomProvider.Setup(x => x.Next(It.IsAny<int>()))
                              .Returns(() => callCount++ < TestMinMatchCount - 1 ? 0 : 1);
        }

        private void SetupRandomProviderForVariableResults()
        {
            var random = new Random(42); // Fixed seed for reproducible tests
            _mockRandomProvider.Setup(x => x.Next(It.IsAny<int>()))
                              .Returns((int max) => random.Next(max));
        }

        private void SetupRandomProviderForMultipleWins()
        {
            // Setup for potential multiple payline wins
            var position = 0;
            _mockRandomProvider.Setup(x => x.Next(It.IsAny<int>()))
                              .Returns(() => 
                              {
                                  // Create patterns that could match multiple paylines
                                  return position++ % 3 == 0 ? 0 : 1;
                              });
        }

        #endregion
    }
}