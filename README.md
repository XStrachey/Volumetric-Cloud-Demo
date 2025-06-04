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

## 🌩️ Physical Principles Behind Volumetric Cloud Rendering

This project implements physically-inspired volumetric cloud rendering using raymarching techniques in a real-time graphics context. The following physical principles and equations are foundational to its implementation:

### ☁️ Participating Media and Radiative Transfer

Clouds are modeled as **participating media**—volumes composed of small water droplets or ice particles that absorb and scatter light. Light interaction in such media is described by the **Radiative Transfer Equation (RTE)**:

$$
\frac{dL(\mathbf{x}, \omega)}{ds} = -\sigma_t(\mathbf{x}) L(\mathbf{x}, \omega) + \sigma_s(\mathbf{x}) \int_{\mathbb{S}^2} p(\omega', \omega) L(\mathbf{x}, \omega') \, d\omega' + Q(\mathbf{x}, \omega)
$$

* $L(\mathbf{x}, \omega)$: Radiance at point $\mathbf{x}$ in direction $\omega$
* $\sigma_t = \sigma_s + \sigma_a$: Extinction coefficient (sum of scattering and absorption)
* $p(\omega', \omega)$: Phase function (probability of scattering from direction $\omega'$ to $\omega$)
* $Q$: Emission term (ignored for non-luminous clouds)

In this project, the implementation assumes **single scattering** and ignores in-scattering from other directions to maintain real-time performance.

---

### 💡 Beer-Lambert Law for Light Absorption

The attenuation of light along a ray through a medium is computed using the **Beer-Lambert law**:

$$
T(d) = \exp\left(-\int_0^d \sigma_t(s) \, ds\right)
$$

In practice, this is approximated by accumulating the extinction term during raymarching steps.

---

### 🌫️ Raymarching Accumulation

For each ray traced from the camera into the volume, a fixed number of samples is taken using **raymarching**. At each step $i$:

$$
L_{\text{acc}} += T_i \cdot \sigma_s(i) \cdot p(\omega_l, \omega_v) \cdot L_{\text{light}} \cdot \Delta s
$$

Where:

* $T_i$: Transmittance from the ray origin to step $i$
* $\omega_l$: Light direction
* $\omega_v$: View direction
* $p$: Phase function
* $\Delta s$: Step length

This accumulation approximates the scattered light contribution along the ray.

---

### 🌈 Henyey-Greenstein Phase Function

The **Henyey-Greenstein phase function** is often used to approximate anisotropic scattering in clouds:

$$
p(\cos\theta) = \frac{1 - g^2}{4\pi (1 + g^2 - 2g\cos\theta)^{3/2}}
$$

* $\theta$: Angle between incoming and outgoing directions
* $g$: Anisotropy parameter, where $g > 0$ favors forward scattering (typical for clouds: $g \approx 0.8$)

---

### 🔊 Density Field and Procedural Noise

Cloud density is procedurally defined using 3D fractal noise functions, such as **Perlin noise** or **FBM (Fractal Brownian Motion)**:

$$
\rho(\mathbf{x}) = \text{FBM}(\mathbf{x}) \cdot \text{heightFalloff}(y)
$$

This gives a realistic, dynamic structure to the cloud volume.

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
