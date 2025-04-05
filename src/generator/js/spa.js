document.addEventListener('DOMContentLoaded', function() {
    // Track the current URL
    let currentPath = window.location.pathname;
    let isTransitioning = false;
    
    // Transition duration in milliseconds - used for ALL timings
    const TRANSITION_DURATION = 300;
    
    // Listen for clicks on internal links
    document.body.addEventListener('click', function(e) {
        // Find if a link was clicked
        let linkElement = e.target.closest('a');
        
        if (!linkElement) return; // Not a link click
        
        const href = linkElement.getAttribute('href');
        
        // Skip: external links, anchor links, same page, or during transition
        if (!href || 
            href.startsWith('http') || 
            href.startsWith('#') || 
            href === currentPath || 
            isTransitioning) {
            return;
        }
        
        // Only handle internal navigation
        if (href.startsWith('/')) {
            e.preventDefault();
            isTransitioning = true;
            
            // Update navigation button states immediately
            const navLinks = document.querySelectorAll('.btn[data-nav-target]');
            navLinks.forEach(link => {
                // Reset all buttons to ghost state
                link.classList.remove('btn-primary', 'btn-secondary');
                link.classList.add('btn-ghost');
            });
            
            // Set clicked button to active state
            if (linkElement.hasAttribute('data-nav-target')) {
                linkElement.classList.remove('btn-ghost');
                linkElement.classList.add('btn-primary');  // Changed from btn-secondary to btn-primary
            } else if (href.includes('/posts/')) {
                // When clicking on individual posts, keep Posts nav button active
                const postsNavButton = document.querySelector('.btn[data-nav-target="Posts"]');
                if (postsNavButton) {
                    postsNavButton.classList.remove('btn-ghost');
                    postsNavButton.classList.add('btn-primary');  // Changed from btn-secondary to btn-primary
                }
            }
            
            // Find the content area - DO NOT touch the hero
            const contentArea = document.getElementById('content-area');
            
            if (!contentArea) {
                console.error('Content area not found');
                window.location.href = href;
                return;
            }
            
            // 1. Fade out content area ONLY
            contentArea.style.transition = `opacity ${TRANSITION_DURATION}ms ease`;
            contentArea.style.opacity = '0';
            
            // 2. After fade out completes, fetch and update
            setTimeout(() => {
                fetch(href)
                    .then(response => response.text())
                    .then(html => {
                        const parser = new DOMParser();
                        const newDoc = parser.parseFromString(html, 'text/html');
                        
                        // Get new content
                        const newContent = newDoc.getElementById('content-area');
                        
                        if (!newContent) {
                            console.error('New content area not found');
                            window.location.href = href;
                            return;
                        }
                        
                        // 3. Replace content with new page content
                        contentArea.innerHTML = newContent.innerHTML;
                        
                        // 4. Update page data
                        document.title = newDoc.title;
                        window.history.pushState({}, newDoc.title, href);
                        currentPath = href;
                        
                        // 5. Initialize any scripts in new content
                        if (window.Prism) Prism.highlightAll();
                        if (window.mermaid && typeof mermaid !== 'undefined') {
                            mermaid.init(undefined, document.querySelectorAll('.mermaid'));
                        }
                        
                        // 6. Force reflow before starting transition
                        void contentArea.offsetWidth;
                        
                        // 7. Fade in content
                        contentArea.style.transition = `opacity ${TRANSITION_DURATION}ms ease`;
                        contentArea.style.opacity = '1';
                        
                        // 8. Reset transition after fade-in completes
                        setTimeout(() => {
                            contentArea.style.transition = '';
                            isTransitioning = false;
                        }, TRANSITION_DURATION);
                    })
                    .catch(error => {
                        console.error('Error loading page:', error);
                        window.location.href = href;
                        isTransitioning = false;
                    });
            }, TRANSITION_DURATION);
        }
    });
    
    // Handle browser back/forward
    window.addEventListener('popstate', function() {
        if (window.location.pathname !== currentPath) {
            window.location.reload();
        }
    });
});