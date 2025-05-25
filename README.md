# Volumetric Cloud & Atmospheric Scattering Demo

A physically-based **volumetric cloud** and **atmospheric scattering** system built on Unity URP.

This project features cloud rendering that conforms to Earth’s curvature and simulates realistic sky colors using Rayleigh, Mie, and ozone scattering. The system includes layered cloud density modeling, light cone sampling, and dynamic weather-driven cloud coverage.

---

## ✨ Features

### ☁️ Volumetric Cloud System

* **Full spherical geometry**: cloud layer is modeled as a shell between two radii over the Earth sphere.
* **Physically motivated density profile**:

  * Supports **Stratus**, **Stratocumulus**, and **Cumulus** formations via gradient height curves.
* **Noise-driven shape formation**:

  * Multi-octave **Perlin + Worley** base shape
  * **High-frequency erosion** for detailed boundaries
  * **Curl noise rolling** for dynamic displacement
* **Weather control** via tiling 2D weather map:

  * Per-cell cloud type, coverage, and anvil strength
* **Wind drift** and vertical lift shaping
* **Cone-shaped light sampling** (with kernel offsets) for volumetric light transmission
* **Energy-aware shading**:

  * Phase-based Mie scattering
  * Altitude-aware sunset coloration (`ApproxAtmosphere` model)

### 🌌 Atmospheric Scattering

* Physically-based **Rayleigh + Mie + Ozone** scattering
* Modeled as a spherical shell surrounding the Earth
* Integrated **optical depth computation** with multi-sample ray integration

---

## 🔧 Implementation Details

* Implemented fully as **URP `ScriptableRenderPass`** feature
* Separated passes for:

  * Atmosphere scattering
  * Volumetric cloud raymarching
* All core math done in HLSL

---

## 📦 Project Core Structure

| File/Folder                 | Description                                     |
| --------------------------- | ----------------------------------------------- |
| `CloudShape.hlsl`           | Cloud base density, weather sampling, erosion   |
| `CloudShade.hlsl`           | Light sampling through cone, shading logic      |
| `AtmosphereScattering.hlsl` | Atmospheric integration (Rayleigh/Mie/Ozone)    |
| `Intersection.hlsl`         | Ray-sphere / cloud shell intersection functions |
| `VolumetricCloudPass.cs`    | Main cloud render pass                          |
| `AtmospherePass.cs`         | Background atmosphere render pass               |

---

## 🛠️ Future Work

* Temporal accumulation / TAA
* Ground/cloud shadow projection
* Full day-night cycle with sun elevation
* Earth surface & ocean curvature rendering
