# Test Plan (2)

## Project Pitch
This is an XR-based virtual painting application. Users can hold a virtual brush to paint on a canvas, pick colors from a palette, and experience strokes that vary in thickness depending on the brush-to-canvas distance.  
Additionally, the palette can switch between fixed and follow mode to support flexible interaction.  
The purpose is to test whether these interactions are intuitive and whether users can smoothly complete the painting workflow.  

---

## Testing Objectives
- Verify whether users can intuitively understand how to use the brush to paint on the canvas.  
- Verify whether users realize that the palette can be used for color picking and can successfully perform this action.  
- Test whether the varying brush thickness based on distance feels natural and usable.  
- Verify whether users can understand and use the palette follow mode, and assess its usefulness.  
- Collect overall feedback on whether the interaction flow is intuitive and smooth.  

---

## Assumptions to Validate
1. Users will attempt to paint on the canvas without explicit instructions.  
2. Users will independently discover the palette’s color-picking function.  
3. Users will notice that brush strokes change thickness depending on distance.  
4. Users will use and understand the palette follow mode.  
5. Users will consider the overall interaction logic natural and easy to learn.  

---

## Prototype Description
- **Platform**: Unity prototype  
- **Scene elements**: Canvas, Palette, Brush  
- **Implemented features**:  
  - Painting on the canvas with the brush  
  - Picking colors from the palette (A/X button)  
  - Stroke thickness varies with brush-to-canvas distance  
  - Palette toggle between fixed and follow mode (Left Grip button)  

---

## Methodology
- **Testing environment**: In-class, live prototype testing  
- **Duration**: ~5 minutes per participant  
- **Data collection**:  
  - Observation (whether participants need hints, whether they complete tasks)  
  - Post-experience questionnaire (rating scales + open-ended questions)  
  - Verbal/behavioral feedback during the experience  

---

## Testing Procedure
1. **Setup**  
   Open the Unity prototype and ensure the canvas, palette, and brush are functional.  

2. **Instruction to participants**  
   Say:  
   > “Please try using this brush and scene as you would in real life, and see what you can do. I will not give you any extra instructions.”  

3. **Interaction (approx. 3 minutes)**  
   Observe whether the participant:  
   - Paints on the canvas  
   - Attempts to pick colors from the palette  
   - Notices the stroke thickness changes with distance  
   - Uses the palette follow mode  

4. **Questionnaire (approx. 2 minutes)**  
   After the experience, the participant fills out a short questionnaire.  

---

## Data Collection
- **Observation log**: Record whether tasks were completed independently and note any confusion.  
- **Questionnaire**: Includes rating scales (intuitiveness, usability, comfort) and open-ended questions (likes/dislikes, suggestions).  
- **Verbal feedback**: Note immediate comments or visible reactions.  

---

## Expected Results
- Most users will intuitively understand how to paint.  
- Some users may need hints to discover the palette’s color-picking feature.  
- Reactions to the stroke-thickness feature may vary, providing insights for iteration.  
- User evaluations of the palette follow mode will help refine the feature.  
- Feedback will guide improvements to the interaction design and user flow.  
