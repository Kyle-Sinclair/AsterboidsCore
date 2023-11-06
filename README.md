# AsterboidsCore

 Intention of this project is to bring a boiding protocol, which are very common in C#, into the Unity DOTS system. 

The basic boids core implementation was taken from here: https://github.com/keijiro/Boids/tree/7eb25a1a5a65a04427bbb4070184c59a4af7b354

This implementation has each boid use the Unity Physics system to locate its neighnours inside its own update loop. 

![alt text](https://github.com/Kyle-Sinclair/AsterboidsCore/blob/main/Assets/Screenshots/200%20Boids%20at%20low%20framerate.PNG?raw=true?raw=true)

The obvious target for reducing 


 
Wanted to make something that actually leverages the job system to do something other than just have many entities doing a basic task

obvious choice is boids. Lot of implementations of boids in raw C#, 
lots of attempts to optimise it, such as Sebastian Lague here: https://www.youtube.com/watch?v=bqtqltqcQhw&ab_channel=SebastianLague

boids core code was taken from here: https://github.com/keijiro/Boids/tree/7eb25a1a5a65a04427bbb4070184c59a4af7b354

step 1: Base Line Boids - 50 boids tanks frame rate to 50fps. Bad!

Step 2: Basic job for calculating needed info than paralel job for movement
