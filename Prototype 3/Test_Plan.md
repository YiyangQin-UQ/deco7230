# XR Painting System Test Plan (IP3 Prototype Testing Plan)

## 1. Project Overview (Project Pitch)
This prototype extends the previous VR painting system to allow users to experience a more realistic painting process in a virtual environment.  
Based on the existing “Canvas–Palette–Brush” system, this prototype focuses on testing several new features:

1. **Toolbox System (Toolbox Switching):**  
   Users can open the toolbox and select different tools such as brushes of different sizes and an eraser, supporting multiple drawing styles.

2. **Pigment Picking & Placement:**  
   Users can pick colors from a pigment box and place them on the palette using a button press, forming visible “paint drops” for mixing.

3. **Joystick Mixing System:**  
   After placing two colors on the palette, users can rotate the **left joystick** to mix them.  
   The more the joystick rotates, the higher the mixing ratio and the more blended the colors become.  
   This simulates the real-world action of stirring paint, serving as a simplified physical mixing interaction.

**Design Intent:**  
The original idea was to support real physical mixing — rotating or stirring the brush in the palette to blend colors.  
However, due to the high technical difficulty, this version uses joystick rotation to represent the mixing ratio.  
The physical stirring concept is still included as a future design goal for comparison and further research.  
Another technical issue is that the pigment drop system may conflict with the existing color-picking code, which could require rewriting part of the system.  

In addition, some **previous features will be optimized**. For example, when picking a color, the brush will now visually highlight the new color to fix feedback issues noted by tutors and classmates.

---

## 2. Testing Objectives
1. Verify whether users can intuitively understand and complete the tool switching task.  
2. Verify whether users can correctly pick colors from the pigment box and place them on the palette.  
3. Verify whether users can use the joystick rotation to complete the color-mixing task and understand the progress feedback.  
4. Evaluate user perception of the mixed color’s visual quality, including “partial mixing” or “layered color” effects.  
5. Explore user interest and feedback toward the **physical stirring** concept as a possible future feature.  

---

## 3. Hypotheses
| ID | Hypothesis |
|----|-------------|
| H1 | Most users can select tools from the toolbox without instruction. |
| H2 | Users can independently complete picking and placing pigments onto the palette. (A potential challenge is whether users expect to paint directly after picking from the pigment box, or whether the mixed color will still preserve layered effects.) |
| H3 | Users can understand that “rotating the joystick = increasing the mixing ratio” and complete the mixing task (reach ≥80% mixing). |
| H4 | The visual feedback of mixing progress (progress ring/percentage) is clear to users. |
| H5 | Users find the mixing process natural and fun, and express interest in future physical stirring interactions. |

---

## 4. Methodology
- **Testing Environment:**  
  Unity XR (Meta Quest 2/3s), standalone build.  

- **Participants:**  
  5–6 peers or tutors during class session.  

- **Duration:**  
  About 5–6 minutes per participant.  

- **Data Collection Methods:**  
  - Observation logs (whether users can complete tasks independently)  
  - Task completion time and error count  
  - Post-test questionnaire (5-point Likert scale + open-ended questions)  
  - Verbal and behavioral feedback  

---

## 5. Testing Tasks
| Task ID | Description |
|----------|--------------|
| T1 | Find the toolbox and select the “brush” tool. |
| T2 | Pick a pigment from the pigment box and press A/Trigger to place it on the palette. |
| T3 | Pick a second pigment and place it on the palette. |
| T4 | Rotate the left joystick to mix colors until the progress ring shows “Mix ≥ 80%”. |
| T5 | Use the mixed color to paint one stroke on the canvas. |

---

## 6. Testing Procedure
1. **Preparation**  
   - Launch the prototype. Confirm all elements (canvas, toolbox, pigment box, and palette) are working.  
2. **Free Exploration (1 minute)**  
   - No instructions are given. Observe if the participant can understand the interactions naturally.  
3. **Guided Testing (3 minutes)**  
   - Ask participants to complete tasks T1–T5 in order.  
4. **Feedback (1–2 minutes)**  
   - Participants complete a short questionnaire and share verbal comments.  

---

## 7. Metrics
| Metric Type | Details |
|--------------|----------|
| Behavioral | Task completion rate (T1–T5), average completion time, error count |
| Cognitive | Whether the user understands the joystick-mixing logic (Yes/No) |
| Subjective | Mixing naturalness (1–5), clarity of feedback (1–5), intuitiveness of tool switching (1–5) |
| Aesthetic | Interest in “layered color” effect and physical mixing idea (1–5 + open comments) |

---

## 8. Expected Results
1. Over 80% of participants can complete most tasks without instruction.  
   Some confusion is expected — for example, users may try to paint immediately after picking a pigment instead of placing it on the palette.  
   This is a known limitation caused by avoiding code conflicts between the pigment and palette systems.  
2. Understanding of mixing feedback (progress ring and percentage) will reach an average Likert score ≥4.0.  
3. At least half of participants mention “wanting real stirring actions” or “more realistic mixing texture” in open responses.  
4. Toolbox function will be rated intuitive and effective (average score ≥4.0).  

---

## 9. Research Summary
This project aims to let users—especially those without painting experience—enjoy a **realistic, material-based painting process** in XR without physical costs or art supplies.  
Unlike common tablet drawing software, this design emphasizes **embodied interaction** and **materiality**, helping users reconnect with the physical relationship between the brush, pigment, and canvas in a virtual space.

Research shows that when digital interactions align with real-world bodily actions, immersion and creative motivation increase significantly [1].  
The “flow experience” theory by Csikszentmihalyi [2] explains that continuous feedback and bodily engagement are central to emotional satisfaction during creative activities.  
Zhang et al. [3] demonstrated that combining visual and haptic feedback in virtual painting tools improves realism and user control.  
Gallace and Spence [4] proposed the *Haptic Aesthetic Processing Model*, showing that aesthetic experience depends not only on vision but also on touch and motion feedback.  
Finally, Elswit [5] highlighted how body awareness extends between digital and physical spaces, suggesting that movement and sensory connection create psychological resonance in virtual art creation.

Building on these findings, this project uses a simplified interaction chain—**pick, place, and mix**—to simulate the tactile process of real painting.  
Although physical stirring is not yet implemented, the intention reflects the core value of “embodied artistic experience,” providing a theoretical and experimental basis for future iterations involving tactile feedback and realistic pigment-blending algorithms.

### References
[1] P. Dourish, *Where the Action Is: The Foundations of Embodied Interaction*, MIT Press, 2001.  
[2] M. Csikszentmihalyi, *Flow: The Psychology of Optimal Experience*, Harper & Row, 1990.  
[3] M. Otsuki, K. Sugihara, A. Toda, F. Shibata, and A. Kimura, “A brush device with visual and haptic feedback for virtual painting of 3D virtual objects,” *Virtual Reality*, vol. 22, pp. 1–15, June 2018, doi: 10.1007/s10055-017-0317-0.  
[4] A. Gallace and C. Spence, “A model for haptic aesthetic processing and its implications for design,” *Proceedings of the IEEE*, vol. 100, no. 9, pp. 1–11, 2012, doi: 10.1109/JPROC.2012.2219831.  
[5] K. Elswit, “Dancing with Coronaspheres: Expanded Breath Bodies and the Politics of Public Movement in the Age of COVID-19,” *Cultural Studies*, vol. 37, no. 6, pp. 894–916, 2023, doi: 10.1080/09502386.2022.2073459.

---

## 10. Future Work
- Implement **real stirring motion recognition**, allowing brush rotation to trigger pigment blending.  
- Add **AR color sampling**, enabling users to capture real-world colors and bring them into the palette.  
- Improve **color-mixing algorithms** (e.g., Oklab or RYB models) for more natural transitions.  
- Add **visual and haptic feedback**, such as vibration and sound effects, to enhance immersion.  
