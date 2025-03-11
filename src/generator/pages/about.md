---
layout: page
title: About
---

# About SpeakEZ Technologies

SpeakEZ Technologies was founded with a mission to create intelligent systems that respect privacy, deliver reliability, and provide exceptional performance.

## Our Team

Our team consists of experts in:

- Designs that Scale with Intelligence & Decision Support
- Machine Learning & Model Training
- Data Analysis with Advanced Analytics
- High Performance Software Development
- Commercial & Industrial Applications

## Our Mission

We believe that technology should serve people, not the other way around. Our systems are designed with human needs in mind, focusing on user experience while maintaining strict privacy standards.

## Our Approach

At SpeakEZ, we take a holistic approach to system design:

1. **Privacy First**: We design with privacy as a foundational principle, not an afterthought
2. **Performance Matters**: Our systems are optimized for speed and efficiency
3. **User-Centered Design**: Everything we build focuses on the people who use it
4. **Continuous Improvement**: We're always learning and evolving

## Code excerpt

```fsharp
let staticPredicate (projectRoot: string, page: string) =
    let ext = Path.GetExtension page
    let fileShouldBeExcluded =
        ext = ".fsx" ||
        ext = ".md"  ||
        ext = ".html"  ||
        page.EndsWith("tailwind.config.js") ||  // exclude Tailwind config
        page.Contains "_public" ||
        ...
    not fileShouldBeExcluded

```

## Website Build Process

The following diagram illustrates how content flows through our static site generation process:

```mermaid
flowchart TD
    A[Content Creation] --> M[Markdown files]
    M --> B(Fornax Processing)
    B --> C{File Type}
    C -->|Markdown| D[Generate HTML]
    C -->|CSS| E[Tailwind Processing]
    C -->|Static Assets| F[Copy to Output]
    D --> G[Apply Layout]
    E --> G
    F --> H[Final Website]
    G --> H
    
    subgraph "Build Pipeline"
    B
    C
    D
    E
    F
    G
    end
    
    style M fill:#0AF,stroke:#333,stroke-width:2px,rx:10,ry:10,padding:10px;
    style A fill:#F9F,stroke:#333,stroke-width:2px,rx:10,ry:10,padding:10px;
    style H fill:#bbf,stroke:#333,stroke-width:2px,rx:10,ry:10,padding:10px;
    style F fill:#F80,stroke:#333,stroke-width:2px,rx:10,ry:10,padding:10px;
```

This automated process ensures consistency and allows for easy customization through the configuration files.