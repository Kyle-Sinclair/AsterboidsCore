# AsterboidsCore

 Intention of this project is to bring a boiding protocol, which are very common in C#, into the Unity DOTS system. 

The basic boids core implementation was taken from here: https://github.com/keijiro/Boids/tree/7eb25a1a5a65a04427bbb4070184c59a4af7b354

This implementation has each boid use the Unity Physics system to locate its neighnours inside its own update loop. 

![alt text](https://github.com/Kyle-Sinclair/AsterboidsCore/blob/main/Assets/Screenshots/200%20Boids%20at%20low%20framerate.PNG?raw=true?)

Running 50 boids using this generic implementation of the boiding algorithm runs into some obvious bottlenecks.

Firstly, the physics system uses non allocating sphere checks. This reduces the size of memory needing to be allocated at 
instantiation time for each individual boid but it also means the physics system is creating and destroying huge amounts of 
collider arrays every frame. 
Secondly, since all the objects have their own individual update calls, Unity's update invoke behaviour is producing 
obvious issues ala [10000 update calls](https://blog.unity.com/engine-platform/10000-update-calls)


lots of attempts to optimise it, such as Sebastian Lague here: https://www.youtube.com/watch?v=bqtqltqcQhw&ab_channel=SebastianLague

boids core code was taken from here: https://github.com/keijiro/Boids/tree/7eb25a1a5a65a04427bbb4070184c59a4af7b354


Downside of this method 

- IJobParallelTransformFor interacts with the transform heirarchy's automessaging, meaning that the unity object
  heirarchy can get very messy if you want the max speed up.
  

step 1: Base Line Boids - 50 boids tanks frame rate to 50fps. Bad!

Step 2: Basic job for calculating needed info than paralel job for movement
