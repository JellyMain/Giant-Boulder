# Unity Game Project

## Authors
- **Yehor Kuchurin (w68225) - Game Logic** 
- **Olzhas Kabi (w68635) - Visuals**

## Overview
This is a Unity-based game project built with Unity 2022.3.59f1 and targeting .NET Framework 4.7.1. The project implements a game with gameplay, menu systems, quests, player progression, and various game mechanics.

## Project Structure

### Core Directories
- **Assets/** - Main Unity assets folder containing all game resources
  - **Scripts/** - C# code organized by functionality
  - **Scenes/** - Game scenes (Boot, Menu, Gameplay, Test)
  - **Prefabs/** - Reusable game objects
  - **Resources/** - Runtime-loadable assets
  - **StaticData/** - Configuration data
  - **Visuals/** - Visual assets (models, textures, etc.)
  - **Sounds/** - Audio files
  - **Animations/** - Animation assets
  - **URP/** - Universal Render Pipeline assets
- **Packages/** - Unity packages
- **ProjectSettings/** - Unity project settings

### Code Organization
The codebase follows a modular, service-oriented architecture with clear separation of concerns:

- **Infrastructure/** - Core systems and bootstrapping
- **GameLoop/** - Game state management and core gameplay logic
- **Player/** - Player-related functionality
- **PlayerInput/** - Input handling systems
- **PlayerCamera/** - Camera control and behavior
- **UI/** - User interface elements
- **Quests/** - Quest system and management
- **Progress/** - Save/load and progression tracking
- **Stats/** - Character and game statistics
- **Coins/** - In-game currency system
- **Enemies/** - Enemy AI and behavior
- **Upgrades/** - Player and game upgrades
- **Structures/** - In-game structures and buildings
- **TerrainGenerator/** - Procedural terrain generation
- **StructuresSpawner/** - Spawning logic for structures
- **Utils/** - Utility functions and helpers
- **Const/** - Constants and configuration values

## Game Features

### Core Systems
- **State Machine** - Manages game states (menu, gameplay, etc.)
- **Save/Load System** - Persistent player progress
- **Scene Management** - Handles scene transitions

### Gameplay Elements
- **Quest System** - In-game quests and objectives
- **Currency System** - In-game economy
- **Player Progression** - Stats and upgrades
- **Enemy AI** - Enemy behavior and interactions

### Technical Features
- **Terrain Generation** - Procedural world generation
- **Structure Spawning** - Dynamic placement of game objects
- **Data Persistence** - Save/load functionality
- **Input Management** - Cross-platform input handling


### Core Mechanics Implementation

#### Save/Load System
The game uses a centralized `SaveLoadService` that handles all persistence operations:
- Player progress is automatically saved at key gameplay moments (completing quests, ending game sessions)
- Save data is stored in JSON format for easy debugging and maintenance
- Progress includes: player stats, completed quests, currency, and unlocked upgrades

#### Quest System
The quest system is managed by the `QuestsService`:
- Quests have multiple states: NotStarted, InProgress, JustCompleted, and Completed
- Each quest has associated rewards (currency, stats boosts, etc.)
- Quest progress is tracked and persisted between game sessions
- Developers can use the debugging console to force-complete quests during testing

#### Game State Management
The game uses a state machine architecture:
- Core states include: Boot, LoadMeta, MainMenu, LoadLevel, Gameplay
- State transitions handle loading screens and resource initialization
- The state machine ensures clean separation between different game phases
- Scene loading is handled by a dedicated `SceneLoader` service that supports callbacks

#### Currency and Progression
- The `CurrencyService` manages all in-game resources
- Player stats evolve through the `StatsService` which handles level-ups and stat distribution
- The upgrade system allows players to spend currency on permanent improvements
- All progression systems persist between sessions through the save/load mechanics

#### Terrain Generation
The game generates terrain procedurally using the following approach:
- Heightmap-based generation with multiple noise layers
- Biome distribution based on temperature and moisture gradients
- Structure placement follows natural terrain features
- Level of detail system for performance optimization


---

© 2025 - All Rights Reserved
