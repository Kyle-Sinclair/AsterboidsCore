# AsterboidsCore

Intention of this project is to bring a boiding protocol, which are very common in C#, into the Unity DOTS system. 

The basic boids core implementation was taken from here: https://github.com/keijiro/Boids/tree/7eb25a1a5a65a04427bbb4070184c59a4af7b354

This implementation has each boid use the Unity Physics system to locate its neighnours inside its own update loop. 

![alt text](https://github.com/Kyle-Sinclair/AsterboidsCore/blob/main/Assets/Screenshots/200%20Boids%20at%20low%20framerate.PNG?raw=true?)

Running 50 boids using this generic implementation of the boiding algorithm runs into some obvious bottlenecks.

Firstly, the physics system uses non allocating sphere checks. 

![alt text](https://github.com/Kyle-Sinclair/AsterboidsCore/blob/main/Assets/Screenshots/Non-alloc%20Physics%20check.png).

This reduces the size of memory needing to be allocated at instantiation time for each individual boid but it also means
the physics system is creating and destroying huge amounts of collider arrays every frame, giving us the large orange section taking up 
roughly 30% of our main thread per frame. Secondly, since all the objects have their own individual update calls, Unity's update invoke behaviour is producing 
obvious issues ala [10000 update calls](https://blog.unity.com/engine-platform/10000-update-calls)
![alt text](https://github.com/Kyle-Sinclair/AsterboidsCore/blob/main/Assets/Screenshots/Profiler-PhysicsAllocUpdateCost.png).

Since ultimately all that needs to be found in the physics sphere check is the position of nearby boids, this area is obviously ripe for
incorporation into a burst job. 





Downside of this method 

- IJobParallelTransformFor interacts with the transform heirarchy's automessaging, meaning that the unity object
  heirarchy can get very messy if you want the max speed up.
  

step 1: Base Line Boids - 50 boids tanks frame rate to 50fps. Bad!

Step 2: Basic job for calculating needed info than paralel job for movement
