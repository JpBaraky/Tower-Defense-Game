# 🧱 Hexguard  
### A 3D Roguelike Hex Tower Defense

![Unity](https://img.shields.io/badge/Engine-Unity_3D-black?logo=unity)
![C#](https://img.shields.io/badge/Language-C%23-239120?logo=csharp)
![License](https://img.shields.io/badge/License-Custom-blue)
![Status](https://img.shields.io/badge/Status-In_Development-orange)

**Hexguard** is a **3D roguelike tower defense game** built in **Unity**, featuring a **hexagonal grid system**, **height-based terrain**, and a future **deckbuilder-style progression** system.  

Every match is a new tactical challenge — combining procedural maps, evolving upgrades, and card-based strategy.

---

## 🎮 Gameplay Overview

Defend your core from enemy waves that cross a dynamically generated **hex battlefield**.  
Each hex tile has unique height and terrain properties, influencing how and where you build towers.

🗺️ **Hex Terrain**  
🏗️ **Height-Based Bonuses**  
⚔️ **Procedural Roguelike Maps**  
🃏 **(Planned) Deckbuilder System for Towers & Upgrades**

---

## ⚙️ Core Systems

### 🧠 Procedural Generation
- The map is generated **randomly each run**, with:
  - Variable height ranges  
  - Random biomes / tile types  
  - Elevation-based visual blending  
- Ensures each match is **unique and replayable**.

---


### 🃏 Deckbuilder System *(Planned Feature)*
A future update will introduce a **deckbuilding roguelike** layer:

- Build your **deck of tower cards** before each run.  
- Each card represents a **tower type, upgrade, or modifier**.  
- As you progress, **draw cards** to:
  - Place or upgrade towers  
  - Trigger temporary boosts  
  - Modify map tiles (e.g., raise terrain or create walls)  
- Between runs, **unlock new cards** and synergies to expand your deck.

> The goal: mix *Slay the Spire*-style progression with classic tower defense and procedural terrain — for infinite replayability.

---

<details>
<summary>🧩 Example: Tower Card Types</summary>

| Card Type | Description |
|------------|-------------|
| 🔫 **Basic Tower** | Standard single-target tower |
| 💥 **Explosive Tower** | Area damage on hit |
| 🌩️ **Storm Node** | Chain lightning between tiles |
| 🪨 **Terraform** | Raise/lower terrain height |
| 🧱 **Reinforce** | Temporarily boosts nearby towers |

</details>

---

## 🧰 Tech Stack

| Tool / Framework | Purpose |
|------------------|----------|
| **Unity 6** | Core game engine |
| **C#** | Gameplay programming |

---

## 🚧 Progress Overview

| System | Status | Notes |
|---------|---------|-------|
| Hex Tile Generator | ✅ Done | Procedural mesh placement working |
| Tower Placement | ✅ Done | Preview alignment under refinement |
| Height System | ✅ Done | Used for bonus and tower logic |
| Procedural Map | 🚧 Prototype | Expanding biomes next |
| Deckbuilder Layer | 🧩 Planned | Future roguelike upgrade system |
| UI & UX | 🧩 Planned | Tower cards and upgrade menus |

---

## 🎨 Visual Style

A bright stylized world inspired by:
- *For The King*  
- *Hexonia*  
- *Into The Breach*  
- *Kingdom Rush*

The design focuses on clarity, clean elevation contrasts, and small-scale diorama vibes.

---

## 👤 Developer

**Created by:** [João Pedro Baraky (JpBaraky)](https://github.com/JpBaraky)  
🎮 Solo developer exploring all aspects of game development — from programming and procedural generation to art and localization.  
🌍 The final version will support **Portuguese (PT-BR)** and **English**.

---

## 🗓️ Roadmap

- [ ] Implement card-based deckbuilder prototype  
- [ ] Add enemy AI & pathfinding  
- [ ] Expand terrain types (lava, forest, crystal, etc.)  
- [ ] Add tower upgrade UI  
- [ ] Implement meta-progression system  
- [ ] Localization & language options  
- [ ] Release playable demo  

---

## 🕹️ How to Play (Soon)

When available:
1. Clone this repository  
2. Open in **Unity 2022 LTS** or **Unity 6**  
3. Press **Play** in the editor  
4. Use the mouse to preview and place towers  

---

## 🧩 License

This project is available for **learning and non-commercial use**.  
© 2025 João Pedro Baraky. All rights reserved.

---
