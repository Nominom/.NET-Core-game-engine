# .NET-Core Game Engine

## An experimentation in creating a game engine from scratch with C# and .NET Core

This project is part of my Bachelor's thesis about writing a game engine in C#.

A lot of the ispiration for this engine was taken from Unity's new data-oriented design and what it brought with it (cache-friendly ecs, job-system, memory pooling). 
The code is my own interpretation of what Unity shared from their new architecture in the many presentations and documents I've seen, with some of my own ideas sprinkled in.
While the architecture is definitely inspired, I didn't look at any of Unity's internal code to implement any of the systems. In the end, I succeeded in achieving
a highly-performant architecture that is nearly 4-times faster than non-ECS Unity in a (arguably designed to favor good cpu-performance) test-game of 3D-asteroids. 

There's still a lot of work to be done and I might pick this up again at some point. For now, feel free to peek around the code and use it as you see fit.

## Features

 * ECS architecture with a messaging system
 * Multi-threaded job system to greatly utilize multiple cores on the CPU
 * Cache-friendly memory layout (components are laid out sequentially in memory so cache-prefetching gives a huge performance boost)
 * Memory pooling for component data
 * Vulkan graphics (graphics pipeline is fully customizable)
 * Zero-allocation game loop to avoid gc pauses (at least engine doesn't allocate. User code might)
 * Asset packing pipeline with compile-time texture compression (check out my other project [BCnEncoder.NET](https://github.com/Nominom/BCnEncoder.NET))
 * A built-in multi-threaded profiler
 * Built-in physics engine with [BEPUPhysics 2](https://github.com/bepu/bepuphysics2)
 
## What's missing?

 * A graphical editor
 * Sounds
 * A more complete graphics API
