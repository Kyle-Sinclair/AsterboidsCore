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
incorporation into a burst job. By storing every asteroids position, rotation and velocity into a contiguous arrays, we can guarentee cache coherency
for every neighbour calculation. 

![alt text](https://github.com/Kyle-Sinclair/AsterboidsCore/blob/main/Assets/Screenshots/BoidDirectionJob%20-%20pre%20parallel.png?raw=true).

The actual movement of asterboids then occurs as a separate Transform job, using the transform access arrays. 

![alt text](https://github.com/Kyle-Sinclair/AsterboidsCore/blob/main/Assets/Screenshots/MoveAsterboidJob.png?raw=true).

An additional job then updates the asterboid positions and rotations into the native arrays at the end of the frame.
These methods together can achieve 60 FPS for about 5000 boids, 1000 each centered one of 5 boid controllers. 

![alt text](https://github.com/Kyle-Sinclair/AsterboidsCore/blob/main/Assets/Screenshots/5000%20blobs%2060%20FPS.png).

However, there are some new bottlenecks emerging at this point. Almost half of the CPU thread's per frame calculation time is now
spent on jobs idling on dependencies waiting to complete. We may be able to get some improvements by organising these calls to the jobs in a more logical fashion. 
In addition, the boid direction calculation job is not parallelised at this point. This could also improve the speed of the job. 

Starting with the parallelism allows us to simulate 10000 boids, orbiting 5 controllers at 20 FPS. However, our boid direction calculation is still taking us 
16 ms each frame. 

![alt text](https://github.com/Kyle-Sinclair/AsterboidsCore/blob/main/Assets/Screenshots/10000%20boids%20at%2020%20FPS.png).

Looking at this from the timeline view it becomes apparent that waiting on job dependencies is costing us almost 10 ms per frame. However, given how
quickly the jobs need one another, this may not be possible to remove. Nonetheless, let us try to coordinate job scheduling more logically. 

![alt text](https://github.com/Kyle-Sinclair/AsterboidsCore/blob/main/Assets/Screenshots/10000%20boids%20timeline%20view.png).

We can try sorting out the jobs to be scheduled into distinct called phases of the frame, coordinated from a manager class.
Each phase has a distinct responsibility, with the first storing which asterboid indexes are now "dead", and then the early phase scheduling
the boid direction calculation job. The last phase then calls the other two jobs that rely on the boid direction job.

![alt text](https://github.com/Kyle-Sinclair/AsterboidsCore/blob/main/Assets/Screenshots/Logical%20boids.png).

Sadly, this does not cause any improvements in speed. In all likelihood given the complexity of the boid steering calculation, this job probably cannot be compressed
any further without exploring methods such as vectorisation. 

Rendering and physics are also becoming bottlenecks, so exploring GPU instanced meshes and ECS' collisiion system would probably also become more 
realistic methods for moving forward. 

#Dead Transforms

Also of importance with a game like this is minimizing allocations which can cause delays or complicated data rearrangements in our collection when an 
object is killed. In order to avoid complicated asteroid death methods that dynamically reallocate dead asteroid's data to one end of the array, I 
simply started keeping track of currently 'alive' asteroids as a boolean array, and skipping dead asteroids contribution to the boid calculations.



![alt text](https://github.com/Kyle-Sinclair/AsterboidsCore/blob/main/Assets/Screenshots/live%20asterboids%20array.png?raw=true).

![alt text]([https://github.com/Kyle-Sinclair/AsterboidsCore/blob/main/Assets/Screenshots/live%20asterboids%20check.png?raw=true).
