# AsterboidsCore
 
Wanted to make something that actually leverages the job system to do something other than just have many entities doing a basic task

obvious choice is boids. Lot of implementations of boids in raw C#, 
lots of attempts to optimise it, such as Sebastian Lague here: https://www.youtube.com/watch?v=bqtqltqcQhw&ab_channel=SebastianLague

boids core code was taken from here: https://github.com/keijiro/Boids/tree/7eb25a1a5a65a04427bbb4070184c59a4af7b354

step 1: Base Line Boids - 50 boids tanks frame rate to 50fps. Bad!

Step 2: Basic job for calculating needed info than paralel job for movement
