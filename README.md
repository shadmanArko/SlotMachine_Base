# Unity Slot Game Framework

A professional, modular slot machine game framework built for Unity using modern architecture patterns and industry best practices.

## Overview

This framework provides a complete, production-ready slot machine implementation featuring clean architecture, dependency injection, reactive programming, comprehensive configuration systems, and Unit Testing. Designed for scalability and maintainability in commercial game development.

## Architecture

### Core Design Patterns

- **Model-View-Controller (MVC)**: Clear separation of game logic, presentation, and user interaction
- **Dependency Injection**: Zenject-based IoC container for loose coupling and testability
- **Observer Pattern**: UniRx reactive extensions for event-driven communication
- **Strategy Pattern**: Configurable game rules and symbol behaviors
- **Factory Pattern**: Dynamic symbol and payline creation

### Technology Stack

- **Unity Engine**: 2021.3 LTS or higher
- **Zenject**: Dependency injection framework
- **UniRx**: Reactive Extensions for Unity
- **DOTween**: Animation and tweening system
- **UniTask**: Async/await support for Unity

## Features

### Core Gameplay
- Configurable reel layouts (default: 5x3 grid)
- Dynamic payline evaluation system
- Real-time win calculation and animation
- Smooth reel spinning with staggered stops
- Symbol highlighting for winning combinations

### Configuration System
- **ScriptableObject-based**: Hot-swappable game configurations
- **Symbol Management**: Custom symbol definitions with sprites, values, and colors
- **Payline Configuration**: Visual payline editor with color coding
- **Game Settings**: Performance, animation, and debug options

### Visual System
- **Modular UI Components**: Reusable slot machine interface elements
- **Animation Framework**: DOTween-powered smooth animations
- **Particle Effects**: Configurable win celebration effects
- **Responsive Design**: Scalable UI for multiple screen resolutions

### Development Tools
- **Editor Extensions**: Custom menu items for rapid configuration setup
- **Debug System**: Comprehensive logging and visualization tools
- **Testing Framework**: Mock random providers for deterministic testing
- **Performance Monitoring**: Built-in performance tracking

## Project Structure

```
├── Configs/
│   ├── Data/                   # Data containers
│   │   ├── PaylineData.cs      # Payline configuration
│   │   └── SymbolData.cs       # Symbol definitions
│   ├── Interfaces/             # Configuration contracts
│   ├── GameSettingsConfigSO.cs # Global game settings
│   └── SlotConfigurationSO.cs  # Main slot configuration
├── Controllers/
│   ├── Interfaces/            # Controller contracts
│   └── SlotGameController.cs  # Main game controller
├── Events/
│   ├── EventBus_Core/         # Core EventBus System
│   ├── OnSpinButtonPressed.cs # Will be called on pressing Spin btn
│   └── SlotGameController.cs  # All Events and Actions
├── Installers/                # Zenject dependency injection
│   ├── SlotGameInstaller.cs   # Scene-level dependencies
├── Model/                     # Game logic layer
│   ├── Data/                  # Data containers
│   │   ├── PaylineResult.cs   # Win calculation results
│   │   ├── SlotSymbol.cs      # Symbol implementation
│   │   └── SpinResult.cs      # Spin outcome data
│   ├── Interfaces/            # Model contracts
│   ├── Services/              # Model Services
│   │   └── SystemRandomProvider.cs # Random number generation
│   └── SlotGameModel.cs     # Core game logic
├── Views/                   # Presentation layer
│   ├── Interfaces/          # View contracts
│   ├── SlotMachineView.cs   # Main UI controller
│   ├── SlotReelView.cs      # Individual reel display
│   └── SlotSymbolView.cs    # Symbol rendering
└── Editor/                  # Development tools
    └── SlotGameMenuItems.cs # Unity editor extensions
```

## Quick Start

### Prerequisites

```
Unity 2021.3 LTS+
Zenject (Extenject)
UniRx
DOTween (Pro recommended)
UniTask
```

### Installation

1. **Clone Repository**
  

2. **Import Dependencies** (Should be imported automatically)
    - Install Zenject via Package Manager or Asset Store
    - Import UniRx, DOTween, and UniTask packages
   
3. **Open GameScene**
    - `Assets/Scenes/GameScene`

4. **Setup Scene** (If setup is incomplete)
    - Use menu item: `SlotGame > Setup Scene`
    - Create configurations: `SlotGame > Create Default Configuration`
    - Assign configurations to SceneContext installer

5. **Configure Game** (If setup is incomplete)
    - Open created SlotConfiguration asset
    - Use context menu: `Create Default Configuration`
    - Customize symbols, paylines, and settings as needed
   
6. **Play Game**
    - Start game in Unity editor


## Configuration

### Symbol Configuration

```csharp
// Define custom symbols
var symbols = new SymbolData[]
{
    new SymbolData(0, "Cherry", 10, Color.red),
    new SymbolData(1, "Bell", 25, Color.green),
    new SymbolData(2, "Seven", 100, Color.gold)
};
```

### Payline Setup

```csharp
// Create payline patterns
var paylines = new PaylineData[]
{
    // Horizontal lines
    new PaylineData(new[] {0,1,2,3,4}, Color.yellow),      // Top row
    new PaylineData(new[] {5,6,7,8,9}, Color.red),         // Middle row
    new PaylineData(new[] {10,11,12,13,14}, Color.blue),   // Bottom row
    
    // Diagonal lines
    new PaylineData(new[] {0,6,12,8,4}, Color.green),      // Diagonal down
    new PaylineData(new[] {10,6,2,8,14}, Color.cyan)       // Diagonal up
};
```

### Performance Settings

```csharp
// Game settings configuration
public class GameSettings
{
    public int targetFrameRate = 60;
    public bool enableWinAnimations = true;
    public float winDisplayDuration = 3f;
    public bool enableDebugLogs = false;
}
```

## API Reference

### Core Interfaces

#### ISlotGameModel
```csharp
public interface ISlotGameModel
{
    bool CanSpin { get; }
    event Action OnSpinStarted;
    event Action<ISpinResult> OnSpinCompleted;
    
    UniTask<ISpinResult> PerformSpinAsync();
    void Initialize();
}
```

#### ISlotMachineView
```csharp
public interface ISlotMachineView
{
    IObservable<Unit> OnSpinRequested { get; }
    
    void SetSpinButtonEnabled(bool enabled);
    void ShowMessage(string message);
    void UpdatePayoutDisplay(int payout);
    void StartReelAnimations();
    void StopReelAnimations(ISlotSymbol[,] results);
    void ShowWinEffects();
    void HighlightWinningSymbols(int[] positions);
}
```

#### ISlotConfiguration
```csharp
public interface ISlotConfiguration
{
    int ReelCount { get; }
    int RowCount { get; }
    int MinMatchCount { get; }
    float SpinDuration { get; }
    
    IReadOnlyList<ISlotSymbol> Symbols { get; }
    IReadOnlyList<IReadOnlyList<int>> Paylines { get; }
}
```

## Testing

### Unit Tests

```csharp
[Test]
public void SlotGameModel_PerformSpin_ReturnsValidResult()
{
    // Arrange
    var mockRandom = new MockRandomProvider(0, 1, 2);
    var model = new SlotGameModel(_config, mockRandom);
    
    // Act
    var result = await model.PerformSpinAsync();
    
    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual(5, result.ReelResults.GetLength(0));
}
```

### Integration Tests

```csharp
[UnityTest]
public IEnumerator SlotGameController_Initialize_CompletesSuccessfully()
{
    // Setup scene with test installer
    yield return LoadSceneAsync("TestScene");
    
    var controller = Container.Resolve<ISlotGameController>();
    bool initialized = false;
    
    controller.OnGameInitialized.Subscribe(_ => initialized = true);
    
    yield return new WaitUntil(() => initialized);
    Assert.IsTrue(initialized);
}
```

## Performance Optimization

### Memory Management
- Object pooling for symbol instances
- Efficient payline calculation algorithms
- Optimized animation sequences
- Automatic cleanup of disposable resources

### Rendering Optimization
- Sprite atlas usage for symbols
- UI batching for reel displays
- Culling for off-screen elements
- Optimized particle systems

## Extension Points

### Custom Symbol Types
```csharp
public class WildSymbol : SlotSymbol
{
    public override bool CanSubstituteFor(ISlotSymbol other) => true;
}
```

### Custom Win Evaluators
```csharp
public class ScatterWinEvaluator : IWinEvaluator
{
    public IPaylineResult EvaluateWin(ISlotSymbol[,] results) 
    {
        // Custom scatter win logic
    }
}
```

### Animation Extensions
```csharp
public class CustomReelAnimation : IReelAnimation
{
    public UniTask AnimateSpinStart(SlotReelView reel) 
    {
        // Custom spin animation
    }
}
```

## Debugging

### Debug Features
- **Visual Payline Display**: Overlay paylines on reels
- **Symbol ID Display**: Show symbol identifiers in development
- **Performance Metrics**: Frame rate and memory usage monitoring
- **Event Logging**: Comprehensive game event tracking

### Debug Console Commands
```
/slot spin           - Trigger manual spin
/slot reset          - Reset game state
/slot config reload  - Reload configuration
/slot debug toggle   - Toggle debug visualization
```

## Contributing

### Development Setup
1. Fork the repository
2. Create feature branch: `git checkout -b feature/amazing-feature`
3. Follow coding standards and add tests
4. Commit changes: `git commit -m 'Add amazing feature'`
5. Push branch: `git push origin feature/amazing-feature`
6. Submit pull request

### Coding Standards
- Follow Unity C# coding conventions
- Use async/await for asynchronous operations
- Implement proper disposal patterns
- Add comprehensive XML documentation
- Include unit tests for new features

### Code Review Checklist
- [ ] All tests passing
- [ ] Performance impact assessed
- [ ] Memory leaks checked
- [ ] Documentation updated
- [ ] Backward compatibility maintained

## Deployment

### Build Configuration
```csharp
// Production build settings
public static class BuildSettings
{
    public const bool ENABLE_DEBUG_LOGS = false;
    public const bool ENABLE_PROFILER = false;
    public const int TARGET_FRAME_RATE = 60;
    public const bool ENABLE_ANALYTICS = true;
}
```

### Platform Considerations
- **Mobile**: Optimized touch controls and performance settings
- **WebGL**: Browser-specific optimizations and loading strategies
- **Desktop**: High-resolution support and keyboard shortcuts
- **Console**: Platform-specific input handling and achievements
