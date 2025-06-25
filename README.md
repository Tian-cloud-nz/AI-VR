# Boids Bird Simulation

A Unity-based 3D bird flocking simulation that implements the classic Boids algorithm with advanced behavioral features including hunger mechanics, decision trees, and realistic flight physics.

##  Features

### Core Boids Algorithm
- **Cohesion**: Birds are attracted to the center of nearby flockmates
- **Separation**: Birds avoid crowding by maintaining minimum distance
- **Alignment**: Birds align their direction with nearby flockmates
- **Boundary Avoidance**: Birds stay within defined boundaries

### Advanced Behaviors
- **Hunger System**: Birds gradually become hungry and seek food
- **Decision Tree AI**: Intelligent decision-making for foraging vs flocking
- **Food Spawning**: Dynamic food generation system
- **Individuality**: Each bird has unique personality traits and behavior variations
- **Realistic Flight Physics**: Smooth rotation, height maintenance, and ground avoidance

### Visual Features
- **3D Bird Models**: Multiple bird types with realistic animations
- **Smooth Spawn Animation**: Birds enter the scene gradually from the boundaries
- **Height Management**: Birds maintain appropriate flight heights
- **Collision Avoidance**: Prevents birds from colliding with each other and obstacles

## 🎮 Controls

The simulation runs automatically once started. Birds will:
1. Spawn gradually from the boundaries
2. Form natural flocking patterns
3. Seek food when hungry
4. Maintain realistic flight behavior

## 🎯 Technical Details

### Architecture
- **BirdController**: Individual bird behavior and physics
- **BirdsManager**: Flock coordination and spawn management
- **Decision Tree System**: AI decision-making framework
- **Food System**: Dynamic food spawning and consumption

### Key Components
- Unity 3D physics with Rigidbody components
- Custom decision tree implementation
- Configurable behavior weights and parameters
- Smooth interpolation for realistic movement

## �� Project Structure
