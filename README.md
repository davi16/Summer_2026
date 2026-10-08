# Game Development - Summer 2026 🎮

**Afeka College of Engineering**  
**Students:** David Ben Yakov & Noa Eitan

## Overview
This repository contains the assignments and the final project for the Game Development course taken during the Summer 2026 semester. The project focuses on 3D level design, environment building, C# scripting, AI behaviors, and interactive cross-scene game mechanics using the Unity Engine.

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
- **Note:** Submission was done via a gameplay video recording.

---

## Assignment 3: Multi-Scene Persistence, Chest & Key Mechanics
**Status:** Completed ✅

This assignment introduces cross-scene state management, physics-based line-of-sight interactions, and a key-and-lock progression loop across multiple village interiors.
- **Goal:** Implement an interactive chest-and-key puzzle in a dedicated interior scene (`HouseWithChest`) while maintaining persistent game state across scene transitions.
- **Features Included:**
  - **Dedicated Interior Scene (`HouseWithChest`):** Connected to a separate village house via dynamic `PortalBehavior` triggers.
  - **Cross-Scene State Persistence:** Seamless preservation of collected Gold Coins (`PersistentObjectManager`), weapon states (`HasGun`), and key ownership (`KeyBehavior.HasKey` & `KeyWasCollectedOnce`) when moving between `Outdoors` and interior scenes.
  - **Smart Player Repositioning:** Automatic saving of the entry door's coordinates and rotation in the village, spawning the player right outside the exited building facing forward upon return.
  - **Dual-Condition Chest Interaction (`ChestBehavior`):** Requires both physical proximity (`OnTriggerEnter`) and direct line-of-sight (`Physics.Raycast` with trigger filtering) to display UI prompts and trigger the lid opening animation (`SPACE`), sound effects, and dynamic layer switching (`Ignore Raycast`) to reveal the key inside.
  - **Key & Locked Door System (`KeyBehavior` & `LockedDoorBehavior`):** Collectible key inside the chest that unlocks a specific locked house in the village, displaying context-sensitive UI notifications (locked warning, unlock prompt, and key-used confirmation) and consuming the key from the player's inventory upon unlocking.
- **Note:** Submission was done via a gameplay video recording.

---

## Final Project: Bank Heist - Tactical Action Game
**Status:** In Progress 🚧

The capstone project for the course: an interactive 3D "Bank Heist" game combining multi-scene architecture, role selection, and squad-based NPC AI.
- **Planned Features & Requirements:**
  - **Interactive Main Menu (`StartPanel`):** Role selection allowing the player to play as a (1) Robber, (2) Police Officer / Security Guard, or (3) Bystander, built with custom `TextMeshPro` styling and UI panels.
  - **Multi-Scene World Architecture:** Seamless transitions across at least 3 interconnected scenes (City Street, Main Bank Hall, and Vault Room) with full persistence of `HP`, `Gold`, inventory, and NPC states.
  - **Squad Command & Autonomous NPC AI:** Teammate NPCs powered by `NavMeshAgent` that assist the player/commander (shooting, throwing grenades, retreating) while maintaining formation spacing and avoiding collisions.
  - **Dynamic Bank Queue & Civilian Reactions:** Civilian NPCs queuing inside the bank who dynamically react and drop to the floor when the heist begins.
  - **Combat HUD & Minimap:** World-space `HPBar` sliders above characters, real-time combat/victory/defeat notifications, and a top-down `RenderTexture` Minimap with custom layer culling.

---
*Repository configured with a strict Unity `.gitignore` to prevent LFS issues and keep commits lightweight.*