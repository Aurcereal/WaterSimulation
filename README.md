# PBR Rendered Fluid Simulation
---

https://github.com/user-attachments/assets/0193b138-dc22-41c0-a666-946e171a1ed9

During the summer of 2025, I made a realtime fluid simulation and renderer in Unity.  The features include

- Particle-Based Fluid Simulation based on Smoothed Particle Hydrodynamics (SPH) run using Compute Shaders
- Physically Based Raymarched Fluid Renderer written with HLSL
- Physically Based Screenspace Fluid Renderer written with HLSL (faster than raymarched but not as accurate)
- Particle-Based Foam Simulation for crashing waves run using Compute Shaders
- Foam rendering using using volumetric raymarching or billboard sprites written with HLSL
- Shadows Rendered using either shadow maps or raymarching
- Caustics Rendered using deferred rendering-like techniques or raytracing (mention nvidia article and how i extended it to screenspace)
- Spatial Hashing and different GPU sorting algorithms to allow the simulation to run at high framerates

## Demo Scenes
---

https://github.com/user-attachments/assets/d905c8ff-5326-4d59-9d7c-a48f3f3aa5ea

https://github.com/user-attachments/assets/01dfa4bb-26fd-4ff2-8ed1-322f3970d156

https://github.com/user-attachments/assets/06329d2e-abed-46d8-b07a-bce7907244e3

https://github.com/user-attachments/assets/acd5c6b7-909d-4df0-98d2-aae3b2ace916

TODO: capsule like a water bottle/pill type of thing, water falls in and crashes around the capsule with debugfloats for sizing
TODO: multiple static objects in a scene, water crashes around them, cone and other variety random

EVERYTHING BELOW CAN BE ADDED LATER JUST DO THE TOP TWO FIRST

# Written Breakdown
If you'd prefer a video breakdown, here's a link to the video version of the breakdown.
TODO: clickable vid to brekadown with icon on it like stylization readme, or just a short vid of one scene integrated i ngithub
Before you add video breakdown, delete this 'written breakdown' section and start with particle sim breakdown

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

### Fluid Rendering

To render our fluid particles in a way that looks like fluid, I used two different techniques: raymarching, and a screenspace technique from a (GDC PRESENTATION??).

#### Raymarched Rendering
I implemented the raymarching technique first.  It works by taking the density field from the simulation, caching it in a texture every frame, and using a fullscreen shader to raymarch the cached density field.  We shoot rays out from the camera, refract and reflect them on the water using Fresnel's law, calculate the density along rays going through the water, and compute how light is extinguished from the sun to the camera.  I referred to THIS (FRESNEL PAPER FROM SEB LAGUE) paper for information on how light interacts with fluid.

(INSERT NICE RAYMARCHED RENDER can be vid or img)

#### Screenspace Rendering

While raymarching looks nice, it hurts performance when a lot of fluid is in the world.  To resolve this, I tried a different rendering technique from THIS paper.  It uses a deferred-rendering setup with multiple render passes.  We render the depths of the particles as spheres from the camera, which we then blur.

2x1 grid with image of depth balls, then blurred depth balls, make an array of the textures and we can flip between them with an arrow key debug, small text below saying 'Depth texture' 'Blurred depth texture'
just pause the sim and flip through them taking screenies

The blur's smoothing radius is in world space and it only blurs parts of the screen that are close in depth.  This makes it so only geometry that's actually close to each other is blurred.  With this blurred depth texture as a representation of our screenspace geometry, we run another shader to compute normals from said depth texture.  We can also get positions from depths.  

2x1 grid with normals, positions

Then, we need to get how much fluid each ray that's sent out from the camera refracts through.  For this, we can render our particles at a low opacity with additive blending, so parts of the screen that have lots of particles will be closer to having full alpha.  

density texture

This isn't perfect since it doesn't simulate how rays bend through water.  With the normals and positions of our blurred geometry and the fluid density along our camera rays, we can apply our water PBR techniques to render like how we did in the raymarching shader.  It's faster than raymarched and it can be rendered anywhere, but it doesn't look as good and we can only have a single ray bounce.  It produces a more cartoony style.

(INSERT SCREENSPACE RENDERS)

Both rendering techniques are good for different use cases.  For each of the rendering techniques, I made an SDF Renderer to render a customizable environment around the fluid.

### Foam Rendering

(have a billboard and volume demo of u turning stuff on, u need to show turning features on gradually or smth, also a screenspace foam demo)
To get an appearance of crashing waves, I added foam to the simulation.  When fluid particles have a lot of kinetic energy or are crashing into each other, they generate foam particles.  The foam particles are simulated with compute shaders; they act as rising bubbles beneath the water, foam on the water, and spray when outside of the water.  Many of the techniques I used come from (PAPER).
I first tried rendering the foam as many camera-oriented billboard sprites.  This was good for performance but didn't look that good to me, so I tried rendering them using volumetric raymarching based on (THIS PAPER).  To do this, I had to use the Spatial Hashing technique described earlier for the foam particles.  I liked the result of using both billboard rendering and raymarched rendering.  The screenspace water mode only supports billboard foam while raymarched water mode supports both.  Here are some demos of the foam being simulated and rendered in the raymarched and screenspace mode.

### Shadow Rendering

(cut down stuff like why say matrix who cares)
I also implemented shadows in the environment around the fluid.  I had two different implementations for the raymarched fluid mode and the screenspace fluid mode.  For the raymarching mode, when a ray samples the SDF environment, I send a raymarched ray to the sun to see how much light from the sun gets extinguished through the fluid.  For the screenspace rendering mode, without raymarching, I used shadowmapping.  I re-used the code from when the screenspace rendering mode gets how much density is along each camera ray in a texture by rendering the particles with low opacities in an additive way, except this time, I rendered that texture from the sun, to act as a shadow map.  I then used a view projection matrix when rendering the SDF environment to add shadow mapping to the screenspace fluid mode.  Here are some demos of the shadows in both rendering modes.

### Caustics

fter shadows, I implemented caustics, the visual result of light refracting through the surface of fluid.  Like shadows, this had two separate implementations in the screenspace rendering mode and the raymarching rendering mode.  For the raymarching rendering mode, I referenced this (NVIDIA CAUSTICS ARTICLE).  When a ray samples a point in the SDF environment, it checks if it's in the fluid, and if it is, it samples caustics light by shooting a ray along its normal (in a more realistic renderer, we'd shoot out a lot more rays, but the ray along the normal will likely get the most light because of Lambert's law) and refracting the ray on the surface of the water, and finally, seeing how close that ray is to the sun direction.  
The screenspace rendering implementation is very similar, except for how we get information on how the ray refracts through the water; since we can't use raymarching.  We assume the floor is flat so all the rays we're shooting are along the y-axis.  We then render the positions and normals of the top of the water and use that information to calculate where and how the ray we shoot refracts through the water.
Here are some demos of the caustics in both rendering modes.

