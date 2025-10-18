# PBR Rendered Fluid Simulation
---

TODO: clickable vid to brekadown with icon on it like stylization readme, or just a short vid of one scene integrated i ngithub

https://github.com/user-attachments/assets/0193b138-dc22-41c0-a666-946e171a1ed9

https://github.com/user-attachments/assets/d905c8ff-5326-4d59-9d7c-a48f3f3aa5ea

https://github.com/user-attachments/assets/01dfa4bb-26fd-4ff2-8ed1-322f3970d156

https://github.com/user-attachments/assets/06329d2e-abed-46d8-b07a-bce7907244e3


||||
|---|---|---|
| Row 1 Cell 1 | Row 1 Cell 2 | turn to 2by2? |
| Row 2 Cell 1 | Row 2 Cell 2 | turn to 2by2? |
Pour cup, cut purple jelly, the new main thing with smooth water, ?check premiere?

https://gist.githubusercontent.com/trusktr/93175b620d47827ffdedbf52433e3b37/raw/e980fa9116cb28dfbdee0dc5c17adc5ed91df783/image-grid.md


During the summer of 2025, I made a realtime fluid simulation and renderer in Unity.  The features include

- Particle-Based Fluid Simulation based on Smoothed Particle Hydrodynamics (SPH) run using Compute Shaders
- Physically Based Raymarched Fluid Renderer written with HLSL
- Physically Based Screenspace Fluid Renderer written with HLSL (faster than raymarched but not as accurate)
- Particle-Based Foam Simulation for crashing waves run using Compute Shaders
- Foam rendering using using volumetric raymarching or billboard sprites written with HLSL
- Shadows Rendered using either shadow maps or raymarching
- Caustics Rendered using deferred rendering-like techniques or raytracing (mention nvidia article and how i extended it to screenspace)
- Spatial Hashing and different GPU sorting algorithms to allow the simulation to run at high framerates

TODO: table of contents

## Demo Scenes
---

TODO: some more object interaction ones and check yggd, low res exp prolly necessary to integrate to github
TODO: capsule like a water bottle/pill type of thing, water falls in and crashes around the capsule with debugfloats for sizing
TODO: multiple static objects in a scene, water crashes around them, cone and other variety random
TODO: incredibly viscous (clearly obviously viscous) demo scene, small container prolly

EVERYTHING BELOW CAN BE ADDED LATER JUST DO THE TOP TWO FIRST

## Particle Simulation Breakdown
---

### CPU Simulation

To begin coding the particle simulation, I wrote a 2D particle simulation on the CPU that just simulates gravity.

TODO: show that small vid super basic

In order to make the particles more fluid-like, they need to interact, so we need to be able to compute a pressure force.  I used a technique from Smoothed Particle Hydrodynamics where each discrete particle becomes radially 'blurred'.  

TODO: just show small circle being radially blurred

Each particle then represents a small spherical volume of fluid rather than a volumeless point with mass.  We then represent the particle world with a density field, where we want to be able to sample any position and get the density at that position.  

TOOD: now show whole scene of blurred particles making kinda density field labeled with DENSITY FIELD where the whiter parts are denser (make in after effects with blur)

In order to sample this density field efficiently, we need to iterate over all particles near our sample point and add their density contribution.  This is done using a technique called Spatial Hashing; (DROPDOWN START) it splits the world into grid cells and allows us to query a grid cell to get only the particles within; in order to make this work on infinite grid cells with finite memory, some cells will share an ID, so we may get more particles than the one's in the cell we're querying, similar to how collisions occur in Hashmaps.  More information on the technique can be found here (footnote to paper)  (DROPDOWN END) The technique comes from (INSERT).  This technique also needs to sort over all particles in the world, which would cause difficulties when translating the simulation to the GPU.  After we have our density field, we compute our gradient vector field (the direction in which the density increases the fastest).  

TODO: show arrows on that AE visualization

Fluid generally flows from high density to low density, so we can apply a force against the gradient to make the behavior of the particles more fluid-like.  More information on the pressure force can be found from this paper (INSERT).

### GPU Simulation

wrote this in todo.txt but can do with and without certain things like cross compare, also putting that for stick force ican do with and without with water falling down on a sphere and going down and converging and falling down into another stream vs without stick force it just behaves like you'd expect kinda sliding off.  its ok to show the fluid rendering when talking about forces just be like 'The demo video uses fluid rendering that is explained at a *link section cool markdown*'
Small gifs in a table could work well for cross compare, Premiere Pro Export -> CloudConvert

TODO: More particles on debug demo crashing waves red for fast
