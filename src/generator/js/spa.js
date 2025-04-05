// javascript
// filepath: d:\repos\FornaxHome\src\generator\js\spa.js
document.addEventListener('DOMContentLoaded', () => {
    const TRANSITION_DURATION = 250; // Updated to match Tailwind's 250ms
    let currentPath = window.location.pathname;
    let isTransitioning = false;

    // Compare HTML, ignoring minor whitespace differences
    function sameContentIgnoringWhitespace(a, b) {
        if (!a || !b) return false;
        const clean = html => html.replace(/\s+/g, ' ').trim();
        return clean(a.innerHTML) === clean(b.innerHTML);
    }

    // Fade out only #content-area
    function fadeOutContentArea(callback) {
        const content = document.getElementById('content-area');
        if (!content) {
            callback();
            return;
        }
        content.style.transition = `opacity ${TRANSITION_DURATION}ms ease-in-out`;
        content.style.opacity = '0';
        setTimeout(callback, TRANSITION_DURATION);
    }

    // Fade in only #content-area
    function fadeInContentArea() {
        const content = document.getElementById('content-area');
        if (!content) return;
        
        // Make sure opacity is explicitly 0 before starting transition
        content.style.opacity = '0';
        
        // Force browser to acknowledge the opacity:0 state before transition
        void content.offsetWidth; 
        
        // Set up transition and trigger fade-in
        content.style.transition = `opacity ${TRANSITION_DURATION}ms ease-in-out`;
        content.style.opacity = '1';
        
        setTimeout(() => {
            content.style.transition = '';
            isTransitioning = false;
        }, TRANSITION_DURATION);
    }

    function updateNavigationHighlight(href) {
        const navLinks = document.querySelectorAll('.btn[data-nav-target]');
        // Reset all navigation buttons to ghost state
        navLinks.forEach(btn => {
            btn.classList.remove('btn-primary', 'btn-secondary');
            btn.classList.add('btn-ghost');
        });
        
        // Determine which button to highlight based on the URL path
        if (href === '/') {
            const homeNav = document.querySelector('.btn[data-nav-target="Home"]');
            if (homeNav) {
                homeNav.classList.remove('btn-ghost');
                homeNav.classList.add('btn-primary');
            }
        } else if (href.includes('/posts/')) {
            const postsNav = document.querySelector('.btn[data-nav-target="Posts"]');
            if (postsNav) {
                postsNav.classList.remove('btn-ghost');
                postsNav.classList.add('btn-primary');
            }
        } else if (href.includes('/about')) {
            const aboutNav = document.querySelector('.btn[data-nav-target="About"]');
            if (aboutNav) {
                aboutNav.classList.remove('btn-ghost');
                aboutNav.classList.add('btn-primary');
            }
        } else if (href.includes('/contact')) {
            const contactNav = document.querySelector('.btn[data-nav-target="Contact"]');
            if (contactNav) {
                contactNav.classList.remove('btn-ghost');
                contactNav.classList.add('btn-primary');
            }
        }
    }

    // Morph only body’s children; skip hero if unchanged
    function morphPage(oldBody, newBody, newDoc, href) {
        // Get the elements
        const contentArea = oldBody.querySelector('#content-area');
        const newContentArea = newBody.querySelector('#content-area');
        
        // Always completely replace the content area
        if (contentArea && newContentArea) {
            contentArea.innerHTML = newContentArea.innerHTML;
        }
        
        // Update document properties
        document.title = newDoc.title;
        window.history.pushState({}, newDoc.title, href);
        currentPath = href;
    
        // Re-init scripts
        if (window.Prism) Prism.highlightAll();
        if (window.mermaid && typeof mermaid !== 'undefined') {
            mermaid.init(undefined, document.querySelectorAll('.mermaid'));
        }
    }

    function loadPage(href) {
        fetch(href)
            .then(resp => resp.text())
            .then(html => {
                const parser = new DOMParser();
                const newDoc = parser.parseFromString(html, 'text/html');
                const newBody = newDoc.querySelector('body');
                const oldBody = document.querySelector('body');
                if (!newBody || !oldBody) {
                    // fallback
                    window.location.href = href;
                    return;
                }
    
                fadeOutContentArea(() => {
                    morphPage(oldBody, newBody, newDoc, href);
                    // Update navigation highlight based on the new URL
                    updateNavigationHighlight(href);
                    fadeInContentArea();
                });
            })
            .catch(() => {
                window.location.href = href;
            });
    }

    // Intercept link clicks
    document.body.addEventListener('click', e => {
        const link = e.target.closest('a');
        if (!link) return;

        const href = link.getAttribute('href') || '';
        if (!href || href.startsWith('http') || href.startsWith('#') ||
            href === currentPath || isTransitioning) {
            return;
        }

        if (href.startsWith('/')) {
            e.preventDefault();
            isTransitioning = true;
            loadPage(href);
        }
    });

    // On browser navigation
    window.addEventListener('popstate', () => {
        if (window.location.pathname !== currentPath) {
            window.location.reload();
        }
    });
});