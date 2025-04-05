document.addEventListener('DOMContentLoaded', function() {
    // Track the current URL to prevent unnecessary reloads
    let currentPath = window.location.pathname;
    
    // Select all navigation links with data-nav-target
    document.querySelectorAll('a[data-nav-target], a[href^="/"]:not([href^="//"])').forEach(link => {
        link.addEventListener('click', function(e) {
            const href = this.getAttribute('href');
            
            // Skip external links, anchor links, or same page links
            if (href.startsWith('http') || href.startsWith('#') || href === currentPath) {
                return;
            }
            
            e.preventDefault();
            
            // Check if this is a post link or back button
            const isPostPage = currentPath.includes('/posts/') || href.includes('/posts/');
            const isPostBackButton = this.innerText && this.innerText.includes('Back to Posts');
            
            console.log(`⚡ Navigating to: ${href} (Post page: ${isPostPage}, Back button: ${isPostBackButton})`);
            
            // Fetch the new page content
            fetch(href)
                .then(response => response.text())
                .then(html => {
                    // Create a temporary element to parse the HTML
                    const parser = new DOMParser();
                    const newDoc = parser.parseFromString(html, 'text/html');
                    
                    // Define SPECIFIC sections that might change - we'll check each one individually
                    const sections = [
                        { id: 'static-hero-container', shouldMorph: true },
                        { id: 'content-area', shouldMorph: true },
                        { id: 'navbar', shouldMorph: false } // Don't transition navbar
                    ];
                    
                    // Track sections that need morphing
                    const sectionsToMorph = [];
                    
                    // Phase 1: Identify sections that actually changed, and ONLY fade those out
                    sections.forEach(section => {
                        const currentSection = document.getElementById(section.id);
                        const newSection = newDoc.getElementById(section.id);
                        
                        if (!currentSection || !newSection || !section.shouldMorph) return;
                        
                        // For post-related navigation, ALWAYS use clean replacement for content-area
                        if ((isPostPage || isPostBackButton) && section.id === 'content-area') {
                            console.log(`✅ Using CLEAN replacement for ${section.id} - post-related page`);
                            
                            // Start transition ONLY for this section
                            currentSection.style.transition = 'opacity 300ms ease-in-out';
                            currentSection.style.opacity = '0.2';
                            
                            sectionsToMorph.push({
                                currentSection,
                                newSection,
                                forceReplaceContent: true // Special flag for post navigation
                            });
                            return;
                        }
                        
                        // Normal comparison for other cases
                        const currentContent = currentSection.innerHTML.replace(/\s+/g, ' ').trim();
                        const newContent = newSection.innerHTML.replace(/\s+/g, ' ').trim();
                        
                        if (currentContent !== newContent) {
                            console.log(`✅ Section ${section.id} content changed - will transition`);
                            
                            // ONLY apply transition to THIS specific section
                            currentSection.style.transition = 'opacity 300ms ease-in-out';
                            currentSection.style.opacity = '0.2'; // Dim out just this section
                            
                            // Store for phase 2
                            sectionsToMorph.push({
                                currentSection,
                                newSection
                            });
                        } else {
                            console.log(`⏺️ Section ${section.id} remains STATIC - NO TRANSITION`);
                        }
                    });
                    
                    // If no content changes, just update the URL
                    if (sectionsToMorph.length === 0) {
                        console.log("📌 No content changes detected, just updating URL");
                        window.history.pushState({}, newDoc.title, href);
                        currentPath = href;
                        return;
                    }
                    
                    // Phase 2: After fade-out completes, update ONLY the changed DOM sections
                    setTimeout(() => {
                        // Update only the sections that needed changing
                        sectionsToMorph.forEach(({ currentSection, newSection, forceReplaceContent }) => {
                            console.log(`🔄 Morphing section ${currentSection.id}`);
                            
                            // For post pages or complex changes, use direct replacement
                            if (forceReplaceContent || isPostPage) {
                                console.log(`🔄 Using DIRECT replacement for ${currentSection.id}`);
                                // Completely replace the content - clean slate
                                currentSection.innerHTML = newSection.innerHTML;
                            } else {
                                // Use morphdom for normal cases - with careful options
                                morphdom(currentSection, newSection, {
                                    onBeforeElUpdated: function(fromEl, toEl) {
                                        // Skip if elements are identical
                                        if (fromEl.isEqualNode(toEl)) {
                                            return false;
                                        }
                                        return true;
                                    }
                                });
                            }
                            
                            // Force a reflow to ensure transition will work
                            void currentSection.offsetWidth;
                            
                            // Start fade-in ONLY for this specific section
                            currentSection.style.transition = 'opacity 300ms ease-in-out';
                            currentSection.style.opacity = '1';
                        });
                        
                        // Update page title and history
                        document.title = newDoc.title;
                        window.history.pushState({}, newDoc.title, href);
                        currentPath = href;
                        
                        // Re-initialize any scripts that need to run on the new page
                        if (window.Prism) Prism.highlightAll();
                        if (window.mermaid) {
                            setTimeout(() => mermaid.init(undefined, document.querySelectorAll('.mermaid')), 100);
                        }
                        
                        // Clean up transition styles after fade-in completes
                        setTimeout(() => {
                            sectionsToMorph.forEach(({ currentSection }) => {
                                // Only clear transitions, leave opacity at 1
                                currentSection.style.transition = '';
                            });
                        }, 350);
                    }, 350);
                })
                .catch(error => {
                    console.error('❌ Error loading page:', error);
                    window.location.href = href; // Fallback to normal navigation
                });
        });
    });
    
    // Handle browser back/forward navigation
    window.addEventListener('popstate', function() {
        if (window.location.pathname !== currentPath) {
            window.location.reload();
        }
    });
});