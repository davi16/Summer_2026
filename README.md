# Game Development - Summer 2026 🎮

**Afeka College of Engineering**  
**Student:** David Ben Yakov 

## Overview
This repository contains the assignments and the final project for the Game Development course taken during the Summer 2026 semester. The project focuses on 3D level design, environment building, C# scripting, and interactive game mechanics using the Unity Engine.

---

## Assignment 1: Legendary Village Environment
**Status:** Completed ✅

In this assignment, we created the foundational 3D environment for a legendary village situated on both banks of a river.
- **Goal:** Design a rich, atmospheric 3D environment using Unity's Terrain system and Asset packages.
- **Features Included:**
  - Sculpted terrain with a central river and water shaders.
  - A village layout distributed across two river banks connected by a bridge.
  - Placed environmental assets: houses, a pub, merchant stalls, and dirt paths.
  - Painted foliage (trees, bushes, and grass) for a mythical atmosphere.
  - Fixed scaling and proportions for imported 3D models.
- **Note:** Submission was done via a gameplay video recording.

---

## Assignment 2: The Pub Interior & Character Animations
**Status:** Completed ✅

This assignment expands one of the village buildings into a fully furnished, multi-story pub with animated NPCs and audio.
- **Goal:** Design an interior space and implement character animations with state transitions.
- **Requirements:**
  - Convert one of the houses from Assignment 1 into a pub.
  - Furnish the interior with tables, chairs, a fireplace, lighting, and other relevant props.
  - Add NPC characters with appropriate animations.
  - Include background music and character voice audio.
  - Build a second floor connected by stairs.
  - Configure Animator transitions (e.g., transitioning from walking to stopping, sitting down, or walking up/down stairs).
- **Extra Implemented Features (Ahead of schedule):**
  - **Advanced AI:** Autonomous animal NPCs featuring Raycast obstacle avoidance, curved wandering paths, and a proximity-based fleeing system.
  - **Interactive Portals:** Custom scene transitions requiring explicit player input ('E'), synced with animation/sound delays, and integrated with a `PersistentObjectManager` for accurate spawn point saving.
  - **Collision Optimization:** Smooth stair climbing mechanics utilizing invisible ramp colliders.
  - **Environment Polish:** Terrain leveling (`Set Height`) for stable building foundations, optimized 3D audio sources, and lightweight particle systems for realistic candle flames.
  - **Smart Interaction:** Custom C# scripts for interactive doors and drawers with proximity detection and automated state resets.
- **Note:** Submission is via a gameplay video recording.

---

## Assignment 3: [Placeholder for Assignment 3]
**Status:** Pending ⏳

*Details for the third course assignment will be added here once provided.*

---

## Final Project: Complete Playable Game
**Status:** Pending ⏳

*The capstone project for the course. This will evolve our assignments into a fully ready, small-scale playable game. Details to be announced.*

---
*Repository configured with a strict Unity `.gitignore` to prevent LFS issues and keep commits lightweight.*