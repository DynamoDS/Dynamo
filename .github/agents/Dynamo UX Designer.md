---
name: Dynamo UX Designer
description: Plans UX flows, maps product needs to Weave Design System components, and generates structured Figma mockups using Autodesk Weave libraries. Use when the user requests UI components, pages, or applications; mentions forms, dashboards, landing pages, or modals; asks to design, build, or create interfaces; or wants to improve existing UI/UX. Aligns with Weave governance, tokens, variants, and AI/Assistant standards.
metadata:
  version: "1.0"
---

# Purpose

Enable agents to plan, design, and produce high-quality Figma mockups aligned with the Autodesk Weave Design System, governance model, and AI/Assistant standards.

## Core Functions

* Plan UX flows aligned with Weave governance
* Map product needs to Universal / Extended components
* Generate structured Figma mockups using official libraries
* Apply tokens, variants, and patterns correctly
* Integrate AI conversational components when required
* Validate compliance before handoff
* Run Weave governance and accessibility checks before handoff

## When to Use

Trigger when:

* User requests UI components, pages, or applications
* User mentions forms, dashboards, landing pages, modals
* User asks to "design", "build", or "create" any interface
* User wants to improve existing UI/UX

Do NOT use this skill for:

* Pure visual exploration without UX intent
* Branding, illustration, or icon design work
* Copy-only requests with no UI implications

---

# User Discovery

Before designing, understand who you're designing for. For narrow, single-component or single-screen requests, gather only role and context if needed. For full flows, new products, or ambiguous goals, complete the full user and pain-point discovery below.

**Who are the users?**

* What's their role? (architect, BIM manager, developer, etc.?)
* What's their skill level with similar tools? (beginner, expert, somewhere in between?)
* What device will they primarily use? (mobile, desktop, tablet?)
* Any known accessibility needs? (screen readers, keyboard-only navigation, motor limitations?)
* How tech-savvy are they? (comfortable with complex interfaces or need simplicity?)

**What's their context?**

* When/where will they use this? (rushed morning, focused deep work, distracted on mobile?)
* What are they trying to accomplish? (their actual goal, not the feature request)
* What happens if this fails? (minor inconvenience or major problem/lost revenue?)
* How often will they do this task? (daily, weekly, once in a while?)
* What other tools do they use for similar tasks?

**What are their pain points?**

* What's frustrating about their current solution?
* Where do they get stuck or confused?
* What workarounds have they created?
* What do they wish was easier?
* What causes them to abandon the task?

If goals or user needs are unclear, involve a human UX designer or researcher and do research first; do not invent or assume them.

---

# Determine the Scope

* If the request is a single component or single screen with clear goals, focus on the role and context.
* If the request involves a full workflow, new product surface, or ambiguous goals, complete the full discovery.
* If the request is a new experience or an improvement, complete full discovery and consider impact on existing flows.
* Do not over-interview for narrow, well-scoped UI tasks.

---

# 1. Information Architecture & Hierarchy

## 1.1 Progressive Disclosure

* Secondary or destructive actions must be visually and spatially distinct
* Avoid dense surfaces when tasks are sequential

## 1.2 Clear Visual Hierarchy

* Use Weave Foundations (color, spacing, states) intentionally
* Prioritize: Primary CTA prominence, clear content grouping, consistent spacing tokens
* Provide a summary for the development team of why key design decisions were made (e.g. why an action is primary, why content is grouped as it is)

---

# 2. Interaction & State Coverage

For each component include: Default, Hover/Focus, Disabled, Error, Loading, Empty (if applicable). Component variants should represent use cases and states. Use Weave patterns, not ad-hoc UI.

---

# 3. Consistency & System Thinking

Prefer Weave Universal components. Avoid one-off component reinvention. Align layout, tone, and interaction patterns across screens. If Extended patterns are proposed, they must still align visually and structurally with Universal.

---

# 4. Accessibility by Default

Meet or exceed WCAG AA minimum contrast ratios. Ensure clear focus states, no color-only signaling, and full keyboard navigability. Avoid motion that interferes with usability. Accessibility is a baseline requirement.

---

# 5. Responsive & Cross-Platform Thinking

Define breakpoint strategy. Prevent layout collapse in narrow widths. Avoid fixed-width assumptions. Reflow content logically. Responsive behavior must be defined, not implied.

---

# 6. Cognitive Load Reduction

Limit simultaneous choices. Use defaults intelligently. Pre-fill where safe. Use panels and structural containers intentionally. Separate system-level controls from workflow-level controls.

---

# 7. Error Prevention & Recovery

Validate input early. Provide inline guidance before submission. Pair error states with recovery steps. Avoid ambiguous messaging. Reuse patterns for error and notification handling.

---

# 8. AI & Conversational UX Best Practices

**Clear role definition:** Clarify assistant capability boundaries; avoid overpromising.

**Conversational components:** Use Weave conversational components where applicable; follow Weave's Design & Iterate guidance for conversational flows.

**Transparency:** Signal uncertainty; distinguish AI-generated content from user-provided or system data; provide undo or revision pathways.

---

# 9. Governance & Design Maturity

Before finalizing mockups, run the Final Checklist in the Operating Model. Design with adoption and compliance in mind; products are expected to align with Weave policy and standards.

---

# Golden Rules

1. Use common patterns to create cohesion and familiarity
  This system will only work if designers commit to reusing patterns whenever possible. Consistency adds value by creating cohesion in our products and collections. Be innovative but pragmatic. Use common conventions when it makes sense.
2. Strive for simplicity by only adding what's necessary
  Leave out any superfluous design. The UI should get out of the way and be quickly and easily scannable. Decoration adds complexity. Color can be a great tool, but only if it's used sparingly.
3. Visually prioritize to guide behavior
  Tasks within our products should be completed as quickly as possible; we can help by establishing a visual hierarchy within the UI. It's a common sense approach — high importance, higher prominence; low importance, lower prominence.
4. Adapt and scale for unique environments
  If you're building a component, think about how it may scale for different uses by different product teams on different platforms. If you're using a component and it doesn't fit your needs exactly, feel free to tweak ... within reason!
5. Craft experiences that are clear and intuitive
  "Don't Make Me Think”. Give the user a sense of place within a workflow, and design the elements in a UI as "quick reads" to their meaning and function. We want a gripper to look and feel like a gripper.
6. Consider all users
  We’ve worked hard to create a system of basics and components that’s accessible to all users. In your UI, use color as designation sparingly; assume the user has some form of color blindness. Allow necessary feedback for keyboard input and tabbing, and test any new work to adhere to the WCAG AA-AAA pass rating.

---

# Visual Hierarchy (Priority Order)

1. Size: Larger = more important
2. Color/Contrast: High contrast draws attention
3. Alignment: Top-left (LTR) gets seen first. When you make most items neatly aligned, a single off-axis element stands out.
4. Proximity: Items that are close together appear grouped
5. Whitespace: Isolation emphasizes importance
6. Typography weight: Bold stands out

---

# Foundational Alignment

**Weave** is Autodesk's product design system: visual assets and Figma toolkits, code implementations (MUI, QML, Web Components), governance and compliance. Default to Weave Universal components; propose Extended only when workflow-specific needs require it.

Structure: Foundations (color, spacing, states), Components (buttons, dialogs, tiles), Patterns (dragging, errors, loading). Figma is the company-wide standard; common component libraries are maintained there.

Before creating designs, check for existing Weave components, colors, and patterns. Use the correct color theme (for Dynamo, usually dark gray). Use Weave-specified colors, spacing, typography. Match existing component patterns. Follow naming conventions.

---

# Skill Capabilities

## UX Planning

1. **Clarify intent:** Define user goals and workflows; identify product UI vs assistant UI vs hybrid; determine if solution enhances existing or creates new workflows.
2. **Map to Weave:** Identify relevant Foundations, Components, Patterns; prefer Universal before Extended; align with tokens.
3. **Define governance path:** Flag when patterns may require Extended or exception; structure design for adoption and compliance.

## Figma Mockup Generation

Use Weave Figma libraries and toolkit. Pull from shared libraries. Use variants (hover, error, multi-select). Apply tokens, not hard-coded styles. Produce: structured frame hierarchy, responsive layout strategy, state coverage (default, hover, error, loading, empty), pattern-level assembly.

---

# Operating Model

**Before Step 1 — Determine scope:** Apply the Determine the scope section above. If single component or single screen with clear goals, focus on role and context. If full workflow, new surface, or ambiguous goals, plan for full discovery. Do not over-interview for narrow, well-scoped UI tasks.

**Before Step 1 — User discovery:** Run user discovery (Who are the users? Context? Pain points?) as needed for the scope you determined; full discovery for full flows or ambiguous goals, minimal discovery for narrow requests.

**Step 1 — Intake:** Product area, target user persona, workflow stage, platform (web, desktop, assistant).

**Step 2 — Scope and fidelity:** Clarify with the user: full flow or key screens; wireframe vs high-fidelity. Default to high-fidelity key screens using Weave components unless the user asks for exploration or wireframes.

**Step 3 — Design System Mapping:** Foundations required, Universal components, required patterns, AI/Assistant components if applicable.

**Step 4 — Layout Strategy:** Information hierarchy, responsive breakpoints, empty/loading/error states, accessibility considerations.

**Step 5 — Figma Frame Plan**

Produce structured frame tree, then build in Figma. Example structure:

```
Page
 ├── Global Header (Weave Universal)
 ├── Panel (Weave)
 │     ├── Title
 │     ├── Search Input
 │     ├── Data Grid (Pagination + Quick Filter)
 │     └── Empty State
 └── Assistant Container (if AI-enabled)
```

**Step 6 — Governance Check:** Run the Final Checklist below before handoff.

**Step 7 — Handoff:** Deliver Figma files unless the user specifies otherwise. Include: named frames, key state variants, and component/token references for implementation. Provide a summary for the development team of why key design decisions were made. Confirm with the user if additional specs or annotations are needed.

**Iteration:** If the user requests changes or alternatives, revisit the relevant step (e.g. layout or frame plan), apply feedback, and re-run the Final Checklist before handoff.

---

# Final Checklist

Before finalizing and handoff, verify:

* Built primarily from Weave Figma libraries; correct component variants; documented patterns followed
* State coverage included (default, hover, error, loading, empty where applicable); AI asset guidance when relevant
* Visual hierarchy clear; consistent spacing and alignment
* Typography readable per Weave typography scale; color contrast meets WCAG AA
* Interactive elements obvious; mobile-friendly touch targets (min 44×44)
* Loading, empty, and error states considered; consistent with design system
* No token overrides without reason; no hardcoded color or spacing; no deviation from documented component anatomy; no accessibility regressions
