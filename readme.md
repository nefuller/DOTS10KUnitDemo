[screenshot gameplay](Screenshots/screen0.png?raw=true "Gameplay")
[screenshot editor](Screenshots/screen1.png?raw=true "Editor")
[screenshot performance](Screenshots/screen_perf.png?raw=true "Performance")

A performance test of Unity 2021.3.6f1 and ECS with 10,000 RTS style units. The units do not pathfind but instead run a flocking algorithm (alignment, cohesion and separation). A simple grid-based spatial partitioning system is used to reduce the search space for neighboring units. The units are drawn via a single call to Graphics.DrawMeshInstancedIndirect() where the instancing data is passed into the shader via a compute buffer.

Runs at around 80-90 frames per second with burst enabled on my machine.

