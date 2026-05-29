---
name: Terminal Nexus
colors:
  surface: '#0b1326'
  surface-dim: '#0b1326'
  surface-bright: '#31394d'
  surface-container-lowest: '#060e20'
  surface-container-low: '#131b2e'
  surface-container: '#171f33'
  surface-container-high: '#222a3d'
  surface-container-highest: '#2d3449'
  on-surface: '#dae2fd'
  on-surface-variant: '#c2c6d6'
  inverse-surface: '#dae2fd'
  inverse-on-surface: '#283044'
  outline: '#8c909f'
  outline-variant: '#424754'
  surface-tint: '#adc6ff'
  primary: '#adc6ff'
  on-primary: '#002e6a'
  primary-container: '#4d8eff'
  on-primary-container: '#00285d'
  inverse-primary: '#005ac2'
  secondary: '#ddb7ff'
  on-secondary: '#490080'
  secondary-container: '#6f00be'
  on-secondary-container: '#d6a9ff'
  tertiary: '#4cd7f6'
  on-tertiary: '#003640'
  tertiary-container: '#009eb9'
  on-tertiary-container: '#002f38'
  error: '#ffb4ab'
  on-error: '#690005'
  error-container: '#93000a'
  on-error-container: '#ffdad6'
  primary-fixed: '#d8e2ff'
  primary-fixed-dim: '#adc6ff'
  on-primary-fixed: '#001a42'
  on-primary-fixed-variant: '#004395'
  secondary-fixed: '#f0dbff'
  secondary-fixed-dim: '#ddb7ff'
  on-secondary-fixed: '#2c0051'
  on-secondary-fixed-variant: '#6900b3'
  tertiary-fixed: '#acedff'
  tertiary-fixed-dim: '#4cd7f6'
  on-tertiary-fixed: '#001f26'
  on-tertiary-fixed-variant: '#004e5c'
  background: '#0b1326'
  on-background: '#dae2fd'
  surface-variant: '#2d3449'
typography:
  headline-xl:
    fontFamily: Inter
    fontSize: 40px
    fontWeight: '800'
    lineHeight: 48px
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: Inter
    fontSize: 30px
    fontWeight: '700'
    lineHeight: 38px
    letterSpacing: -0.01em
  headline-md:
    fontFamily: Inter
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 32px
  body-lg:
    fontFamily: Inter
    fontSize: 18px
    fontWeight: '400'
    lineHeight: 28px
  body-md:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
  label-md:
    fontFamily: JetBrains Mono
    fontSize: 14px
    fontWeight: '500'
    lineHeight: 20px
  label-sm:
    fontFamily: JetBrains Mono
    fontSize: 12px
    fontWeight: '500'
    lineHeight: 16px
  headline-lg-mobile:
    fontFamily: Inter
    fontSize: 24px
    fontWeight: '700'
    lineHeight: 32px
rounded:
  sm: 0.125rem
  DEFAULT: 0.25rem
  md: 0.375rem
  lg: 0.5rem
  xl: 0.75rem
  full: 9999px
spacing:
  container-max: 1280px
  gutter: 1.5rem
  margin-mobile: 1rem
  stack-xs: 0.25rem
  stack-sm: 0.5rem
  stack-md: 1rem
  stack-lg: 2rem
---

## Brand & Style
The design system is engineered for a high-end, developer-centric community. It balances the austere efficiency of a technical IDE with the social warmth of a vibrant community hub. The aesthetic is "SaaS-Deep," utilizing layered dark tones to create a sense of focused workspace.

The brand personality is authoritative yet welcoming, emphasizing clarity for long-form technical discussions. The visual style leans heavily into **Minimalism** with a **Glassmorphic** twist: high-information density is managed through generous whitespace and subtle translucency, ensuring that content remains the primary focus while the interface provides a premium, tactile feel.

## Colors
The palette is built on a foundation of deep slates and true blacks to minimize eye strain during extended coding or reading sessions. 

- **Primary Accent:** Modern Electric Blue (#3b82f6) is used for primary actions, active states, and critical navigation.
- **Secondary Gradients:** A mix of Purple (#a855f7) and Cyan (#06b6d4) is reserved for high-value gamification elements like reputation badges, contributor levels, and premium highlights.
- **Neutral Scale:** Uses a cool-toned slate range to maintain a "tech-first" atmosphere, avoiding the muddiness of pure greys.
- **Support for Light Mode:** While the primary experience is dark, light mode uses a crisp white background with #f8fafc surfaces and #64748b text to maintain readability.

## Typography
The system utilizes **Inter** for all primary interface and prose elements due to its exceptional legibility and modern geometric feel. 

- **Weight Hierarchy:** Bold weights (700+) are used for thread titles to ensure they pop in dense feeds.
- **Monospaced Accents:** **JetBrains Mono** is utilized for labels, metadata, and reputation scores to lean into the developer aesthetic.
- **Readability:** Body text uses a slightly increased line height (1.5x) to facilitate comfortable reading of long-form technical explanations.

## Layout & Spacing
This design system employs a **Fixed Grid** on desktop and a **Fluid Grid** on mobile.

- **Desktop:** A central content column of 720px for threads, flanked by a 240px sidebar for community metadata and a 280px navigation rail.
- **Spacing Rhythm:** Based on an 8px base unit. Component internal padding is typically 16px (stack-md), while vertical spacing between feed items is 24px-32px to provide breathing room.
- **Information Density:** Lists and comment threads use tighter vertical spacing (8px) to allow more content to be visible on screen at once, mimicking IDE-like efficiency.

## Elevation & Depth
Depth is created through a "Stacking Surface" model rather than traditional heavy shadows.

- **Base Level:** Deepest black/slate background.
- **Layer 1 (Cards/Feeds):** Subtle glassmorphism with a 1px border (#ffffff 8% opacity) and a backdrop blur of 12px.
- **Layer 2 (Modals/Popovers):** Higher translucency, 1px border (#ffffff 15% opacity), and a soft ambient shadow (0px 10px 30px rgba(0,0,0,0.5)) to lift it off the interface.
- **Active State:** Elements like active sidebar items use a subtle inner glow or a left-side 2px primary blue accent line.

## Shapes
The shape language is "Technical Soft." By using a **Soft (0.25rem)** base roundedness, the UI feels modern and engineered without becoming overly bubbly or consumer-grade.

- **Small Components:** Checkboxes and small tags use 4px (rounded-sm).
- **Primary Cards:** Use 8px (rounded-lg) to define the main content areas clearly.
- **Badges:** Reputation badges and community avatars use circular (pill) shapes to differentiate them from functional UI components.

## Components
- **Nested Comments:** Use vertical "thread lines" (1px width, slate-800) to the left of replies. Hovering a thread line should highlight the entire branch in primary blue.
- **Rich Feed Cards:** Title in Headline-MD, metadata (author, time, tags) in Label-SM. Content preview is limited to 3 lines with a subtle fade-out.
- **Buttons:** 
    - *Primary:* Solid Electric Blue with white text. 
    - *Secondary:* Ghost style with 1px border and subtle hover fill.
- **Reputation Badges:** Compact labels with a gradient background (Purple to Cyan) and white JetBrains Mono text.
- **Input Fields:** Dark slate background, 1px border. Focus state triggers a 1px Primary Blue border and a subtle blue outer glow.
- **Platform Shell:** A fixed-position sidebar for navigation and a top bar with a global "Command Palette" style search bar.