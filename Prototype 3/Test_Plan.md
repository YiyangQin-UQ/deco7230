# XR Painting System Test Plan (IP3 Prototype Testing Plan)

## 1. Project Overview (Project Pitch)

This prototype extends the previous VR painting system to allow users to experience a more realistic painting process in a virtual environment.  
Based on the existing **Canvas–Palette–Brush** system, this prototype focuses on testing several new features:

### Toolbox System (Tool Switching)
Users can open the toolbox and select different tools such as brushes of various sizes and an eraser, supporting multiple drawing styles.

### Pigment Picking & Placement
Users can pick colors from a pigment box and place them on the palette using a button press, forming visible “paint drops” for mixing.

### Joystick Mixing System
After placing two colors on the palette, users can rotate the **left joystick** to mix them.  
The more the joystick rotates, the higher the mixing ratio and the more blended the colors become.  
This simulates the real-world action of stirring paint, serving as a simplified physical mixing interaction.

### Design Intent
The original idea was to support **real physical mixing** — rotating or stirring the brush in the palette to blend pigments.  
However, due to high technical difficulty, this version uses joystick rotation to represent the mixing ratio.  
The physical stirring concept remains a future design goal for comparison and further study.

In addition, several previous features were optimized. For example, when picking a color, the brush now visually highlights the new color to improve feedback clarity.

---

## 1.1 Original Design Intention vs. Final Implementation

In the initial design concept, my goal was to recreate a fully realistic painting experience within a virtual environment.  
Ideally, users would be able to **physically stir and rotate the brush inside the palette** to mix pigments, with each pigment behaving like real paint — slowly blending, flowing, and affecting the texture and layering of brush strokes on the canvas.

However, during implementation, technical constraints in Unity XR (such as performance limits and shader complexity) made full physical pigment simulation unfeasible.  
Therefore, I adopted a **simplified approach** to achieve a similar sense of realism:

- The **left joystick rotation** now represents the degree of pigment mixing.  
- The **transparency** of the pigment indicates the mixing progress — higher transparency means incomplete mixing; as the pigment becomes opaque, it represents a fully blended state.  

To make up for the lack of physical stirring actions, I enhanced the **visual feedback** in several ways:  
- When the brush successfully picks a color, the **resultBall** (mixing result sphere) now **emits light** and becomes temporarily opaque, clearly signaling successful color pickup.  
- When the mixture is incomplete, strokes on the canvas display **layered or streaked colors**, showing partial mixing.  
- When the mixture is complete, the color becomes uniform and stable, reflecting full blending.  

This approach preserves the core idea of **embodied color mixing** while ensuring smooth performance and intuitive feedback in the XR environment.

---

## 2. Testing Objectives

- Verify whether users can intuitively understand and complete the tool switching task.  
- Verify whether users can correctly pick pigments from the pigment box and place them on the palette.  
- Verify whether users can use joystick rotation to complete the color-mixing task and interpret progress feedback.  
- Evaluate user perception of the mixed color’s visual quality (partial mixing, layered effects).  
- Explore user interest and feedback toward the **physical stirring** concept for future development.

---

## 3. Hypotheses

| ID | Hypothesis |
|----|-------------|
| H1 | Most users can select tools from the toolbox without instruction. |
| H2 | Users can independently complete pigment picking and placement on the palette. |
| H3 | Users can understand that “rotating the joystick = increasing the mixing ratio.” |
| H4 | The visual feedback of mixing progress (progress ring/percentage) is clear. |
| H5 | Users find the mixing process natural and enjoyable, expressing interest in physical stirring in future versions. |

---

## 4. Methodology

- **Environment:** Unity XR (Meta Quest 2/3s), standalone build  
- **Participants:** 5–6 peers or tutors during class sessions  
- **Duration:** 5–6 minutes per participant  

### Data Collection
- Observation logs (whether tasks can be completed independently)  
- Task completion time and error count  
- Post-test questionnaire (5-point Likert + open-ended comments)  
- Verbal and behavioral feedback  

---

## 5. Testing Tasks

| Task ID | Description |
|----------|-------------|
| T1 | Find the toolbox and select the “brush” tool. |
| T2 | Pick a pigment from the pigment box and press Trigger to place it on the palette. |
| T3 | Pick a second pigment and place it on the palette. |
| T4 | Rotate the left joystick to mix colors until the progress ring shows “Mix ≥ 80%.” |
| T5 | Use the mixed color to paint one stroke on the canvas. |

---

## 6. Testing Procedure

### Preparation
Launch the prototype and confirm all elements (canvas, toolbox, pigment box, and palette) are functional.

### Free Exploration (1 minute)
No instructions are given. Observe whether participants can understand the interactions naturally.

### Guided Testing (3 minutes)
Ask participants to complete tasks **T1–T5** in order.

### Feedback (1–2 minutes)
Participants fill out a short questionnaire and share verbal comments.

---

## 7. Step-by-Step Operation Manual

### Step 1: Toolbox Interaction
1. After entering the scene, locate the **toolbox** in front of you.  
2. Use the controller to **grab different tools** — try various brushes and the eraser.  
3. Switch between tools freely and test how they behave on the canvas.  

### Step 2: Painting Experience
1. Hold a brush and **press the Trigger button** to start painting on the canvas.  
2. Observe how brush thickness changes with distance:  
   - Closer → **thicker** strokes  
   - Farther → **thinner** strokes  
3. Adjusting distance simulates pressure control in real painting.  

### Step 3: Pigment Placement and Mixing
1. Find the **pigment section** in the toolbox and grab a pigment ball.  
2. **Insert** it into the palette — a visual cue confirms successful placement.  
3. Pick another pigment of a different color and place it on the palette.  
4. **Rotate the left joystick** to mix pigments:  
   - At the beginning, pigments appear **semi-transparent**, meaning partial mixing.  
   - As you rotate more, transparency decreases — pigments gradually blend.  
5. When pigments become **fully opaque**, they are completely mixed.  
6. Touch the palette with your brush to pick the mixed color — the **resultBall glows**, indicating successful color pickup.  

### Step 4: Painting with Mixed Colors
1. Use the newly mixed color to paint on the canvas.  
2. If mixing is incomplete, strokes show **layered/streaked color effects**.  
3. If fully mixed, colors appear **uniform and smooth**.  

---

## 8. Metrics

| Type | Details |
|------|----------|
| **Behavioral** | Task completion rate (T1–T5), average time, error count |
| **Cognitive** | Whether users understand joystick-mixing logic (Yes/No) |
| **Subjective** | Mixing naturalness (1–5), feedback clarity (1–5), tool intuitiveness (1–5) |
| **Aesthetic** | Interest in “layered color” and physical mixing (1–5 + comments) |

---

## 9. Expected Results

- Over 80% of participants can complete most tasks without instruction.  
- Some confusion may occur (e.g., users painting immediately after pigment pickup).  
- Mixing feedback (progress ring/opacity) expected average ≥ 4.0.  
- At least half of users express interest in **real stirring actions** or **more realistic pigment texture**.  
- Toolbox usability rated intuitive and effective (≥ 4.0 average).

---

## 10. Research Summary

This project allows users—especially those without painting experience—to enjoy a **realistic, material-based painting process** in XR without real-world costs.  
Unlike tablet drawing apps, this design emphasizes **embodied interaction** and **materiality**, reconnecting users with the physical relationship between brush, pigment, and canvas.

Previous studies (Dourish, Csikszentmihalyi, Otsuki, Gallace & Spence, Elswit) show that combining bodily movement and sensory feedback enhances immersion, flow, and creative satisfaction.  
By implementing a simplified interaction chain — **pick, place, and mix** — this system provides a foundation for future physical mixing and tactile feedback research.

---

## 11. Future Work

- Implement **real stirring motion recognition**, allowing brush rotation to trigger blending.  
- Add **AR color sampling** to import real-world colors into the palette.  
- Improve **color-mixing algorithms** (e.g., Oklab or RYB) for more natural results.  
- Add **vibration and sound feedback** to enhance immersion.  
