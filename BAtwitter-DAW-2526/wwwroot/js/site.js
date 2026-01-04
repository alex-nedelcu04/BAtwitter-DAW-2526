// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Handle clicks on echo cards, comment cards, and flock cards using event delegation
document.addEventListener('DOMContentLoaded', function () {
    // Use event delegation on document to handle clicks on all cards (including dynamically added ones)
    document.addEventListener('click', function (e) {
        // Handle echo card clicks
        const echoCard = e.target.closest('.echo-card');
        if (echoCard) {
            // Don't navigate if clicking on links, buttons, or video elements
            if (e.target.closest('a, button, video')) {
                return;
            }

            // Prevent event from bubbling to parent flock-card
            e.stopPropagation();

            const url = echoCard.getAttribute('data-echo-url');
            if (url) {
                window.location.href = url;
                return;
            }
        }

        // Handle comment card clicks
        const commentCard = e.target.closest('.comment-card');
        if (commentCard) {
            // Don't navigate if clicking on links, buttons, or video elements
            if (e.target.closest('a, button, video')) {
                return;
            }

            // Prevent event from bubbling to parent elements
            e.stopPropagation();

            const url = commentCard.getAttribute('data-comment-url');
            if (url) {
                window.location.href = url;
                return;
            }
        }

        // Handle flock card clicks
        const flockCard = e.target.closest('.flock-card');
        if (flockCard) {
            // Don't navigate if clicking on links, buttons, or nested echo/comment cards
            if (e.target.closest('a, button, .echo-card, .comment-card')) {
                return;
            }

            // Don't navigate if clicking on the nested echo card area
            if (e.target.closest('.post-click-area')) {
                return;
            }

            const url = flockCard.getAttribute('data-flock-url');
            if (url) {
                window.location.href = url;
                return;
            }
        }


    });

    document.querySelectorAll('.userprofile-card').forEach(function (card) {
        card.addEventListener('click', function (e) {
            // Navigate only if clicking on the card itself, not on links or buttons
            if (e.target.closest('a, button, video')) {
                return;
            }

            const url = card.getAttribute('data-userprofile-url');
            if (url) {
                window.location.href = url;
            }
        });
    });

    // ============================================
    // OPTIMIZED INTERACTION HANDLER
    // Uses event delegation, request queuing, and optimistic updates
    // ============================================
    
    // Request queue to prevent race conditions
    const interactionQueue = new Map();
    const pendingInteractions = new Set();
    
    // Load pending interactions from localStorage on page load
    function loadPendingInteractions() {
        try {
            const stored = localStorage.getItem('pendingInteractions');
            if (stored) {
                const pending = JSON.parse(stored);
                // Apply pending changes to UI if elements exist
                Object.keys(pending).forEach(key => {
                    const [echoId, type] = key.split('_');
                    updateUIForEcho(parseInt(echoId), type, pending[key]);
                });
            }
        } catch (e) {
            console.error('Error loading pending interactions:', e);
        }
    }
    
    // Save pending interaction to localStorage
    function savePendingInteraction(echoId, type, state) {
        try {
            const key = `${echoId}_${type}`;
            const stored = localStorage.getItem('pendingInteractions') || '{}';
            const pending = JSON.parse(stored);
            pending[key] = state;
            localStorage.setItem('pendingInteractions', JSON.stringify(pending));
        } catch (e) {
            console.error('Error saving pending interaction:', e);
        }
    }
    
    // Remove pending interaction from localStorage
    function removePendingInteraction(echoId, type) {
        try {
            const key = `${echoId}_${type}`;
            const stored = localStorage.getItem('pendingInteractions') || '{}';
            const pending = JSON.parse(stored);
            delete pending[key];
            localStorage.setItem('pendingInteractions', JSON.stringify(pending));
        } catch (e) {
            console.error('Error removing pending interaction:', e);
        }
    }
    
    // Clear all pending interactions (call on successful page navigation)
    function clearPendingInteractions() {
        localStorage.removeItem('pendingInteractions');
    }
    
    // Update UI for a specific echo and interaction type
    function updateUIForEcho(echoId, type, state) {
        const forms = document.querySelectorAll(`.interaction-form[data-echo-id="${echoId}"][data-interaction-type="${type}"]`);
        forms.forEach(form => {
            const button = form.querySelector('button[type="submit"]');
            const icon = button.querySelector('i');
            const countSpan = button.querySelector('span');
            
            if (!icon || !countSpan) return;
            
            if (type === 'like') {
                if (state.isLiked) {
                    icon.classList.remove('bi-heart');
                    icon.classList.add('bi-heart-fill', 'text-danger');
                } else {
                    icon.classList.remove('bi-heart-fill', 'text-danger');
                    icon.classList.add('bi-heart');
                }
                if (state.likesCount !== undefined) {
                    countSpan.textContent = state.likesCount;
                }
            } else if (type === 'rebound') {
                if (state.isRebounded) {
                    icon.classList.add('text-success');
                } else {
                    icon.classList.remove('text-success');
                }
                if (state.reboundCount !== undefined) {
                    countSpan.textContent = state.reboundCount;
                }
            } else if (type === 'bookmark') {
                if (state.isBookmarked) {
                    icon.classList.remove('bi-bookmark');
                    icon.classList.add('bi-bookmark-fill', 'text-primary');
                } else {
                    icon.classList.remove('bi-bookmark-fill', 'text-primary');
                    icon.classList.add('bi-bookmark');
                }
                if (state.bookmarksCount !== undefined) {
                    countSpan.textContent = state.bookmarksCount;
                }
            }
        });
    }
    
    // Get current state from UI
    function getCurrentState(echoId, type) {
        const form = document.querySelector(`.interaction-form[data-echo-id="${echoId}"][data-interaction-type="${type}"]`);
        if (!form) return null;
        
        const icon = form.querySelector('i');
        const countSpan = form.querySelector('span');
        if (!icon || !countSpan) return null;
        
        if (type === 'like') {
            return {
                isLiked: icon.classList.contains('bi-heart-fill'),
                likesCount: parseInt(countSpan.textContent) || 0
            };
        } else if (type === 'rebound') {
            return {
                isRebounded: icon.classList.contains('text-success'),
                reboundCount: parseInt(countSpan.textContent) || 0
            };
        } else if (type === 'bookmark') {
            return {
                isBookmarked: icon.classList.contains('bi-bookmark-fill'),
                bookmarksCount: parseInt(countSpan.textContent) || 0
            };
        }
        return null;
    }
    
    // Process interaction with optimistic updates and queuing
    async function processInteraction(echoId, type) {
        const queueKey = `${echoId}_${type}`;
        
        // If already processing, queue this request
        if (pendingInteractions.has(queueKey)) {
            // Wait for current request to complete
            return new Promise((resolve) => {
                const checkInterval = setInterval(() => {
                    if (!pendingInteractions.has(queueKey)) {
                        clearInterval(checkInterval);
                        resolve(processInteraction(echoId, type));
                    }
                }, 50);
            });
        }
        
        pendingInteractions.add(queueKey);
        
        // Get current state for optimistic update
        const currentState = getCurrentState(echoId, type);
        if (!currentState) {
            pendingInteractions.delete(queueKey);
            return;
        }
        
        // Optimistic UI update
        let optimisticState;
        if (type === 'like') {
            optimisticState = {
                isLiked: !currentState.isLiked,
                likesCount: currentState.isLiked ? currentState.likesCount - 1 : currentState.likesCount + 1
            };
        } else if (type === 'rebound') {
            optimisticState = {
                isRebounded: !currentState.isRebounded,
                reboundCount: currentState.isRebounded ? currentState.reboundCount - 1 : currentState.reboundCount + 1
            };
        } else if (type === 'bookmark') {
            optimisticState = {
                isBookmarked: !currentState.isBookmarked,
                bookmarksCount: currentState.isBookmarked ? currentState.bookmarksCount - 1 : currentState.bookmarksCount + 1
            };
        }
        
        // Apply optimistic update
        updateUIForEcho(echoId, type, optimisticState);
        savePendingInteraction(echoId, type, optimisticState);
        
        // Determine action URL
        let actionUrl = '';
        if (type === 'like') {
            actionUrl = '/Interactions/New_Like';
        } else if (type === 'rebound') {
            actionUrl = '/Interactions/New_Rebound';
        } else if (type === 'bookmark') {
            actionUrl = '/Interactions/New_Bookmark';
        }
        
        // Get anti-forgery token
        const form = document.querySelector(`.interaction-form[data-echo-id="${echoId}"][data-interaction-type="${type}"]`);
        const token = form ? (form.querySelector('input[name="__RequestVerificationToken"]')?.value || 
                              document.querySelector('input[name="__RequestVerificationToken"]')?.value || '') : '';
        
        try {
            // Make AJAX request
            const response = await fetch(actionUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                credentials: 'include',
                body: 'EchoId=' + echoId + (token ? '&__RequestVerificationToken=' + encodeURIComponent(token) : '')
            });
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            const data = await response.json();
            
            if (data.success) {
                // Update UI with server response
                const serverState = {};
                if (type === 'like') {
                    serverState.isLiked = data.isLiked;
                    serverState.likesCount = data.likesCount;
                } else if (type === 'rebound') {
                    serverState.isRebounded = data.isRebounded;
                    serverState.reboundCount = data.reboundCount;
                    
                    // Reload feed if we're on the Index page (both when adding and removing rebound)
                    const currentPath = window.location.pathname;
                    if (currentPath === '/Echoes/Index' || currentPath === '/Echoes' || currentPath === '/Echoes/') {
                        // Reload the page to update the feed (show or hide the rebound)
                        window.location.reload();
                        return;
                    }
                } else if (type === 'bookmark') {
                    serverState.isBookmarked = data.isBookmarked;
                    serverState.bookmarksCount = data.bookmarksCount;
                }
                
                updateUIForEcho(echoId, type, serverState);
                removePendingInteraction(echoId, type);
            } else {
                // Revert optimistic update on error
                updateUIForEcho(echoId, type, currentState);
                removePendingInteraction(echoId, type);
                console.error('Interaction failed:', data.error || 'Unknown error');
            }
        } catch (error) {
            // Revert optimistic update on error
            updateUIForEcho(echoId, type, currentState);
            removePendingInteraction(echoId, type);
            console.error('Error processing interaction:', error);
        } finally {
            pendingInteractions.delete(queueKey);
        }
    }
    
    // Event delegation for interaction forms (single listener for all forms)
    document.addEventListener('submit', function(e) {
        const form = e.target.closest('.interaction-form');
        if (!form) return;
        
        e.preventDefault();
        e.stopPropagation();
        
        const echoId = parseInt(form.getAttribute('data-echo-id'));
        const interactionType = form.getAttribute('data-interaction-type');
        const button = form.querySelector('button[type="submit"]');
        
        if (!echoId || !interactionType || !button) return;
        
        // Disable button during processing
        button.disabled = true;
        
        // Process interaction
        processInteraction(echoId, interactionType).finally(() => {
            button.disabled = false;
        });
    }, true); // Use capture phase to catch early
    
    // Load pending interactions on page load
    loadPendingInteractions();
    
    // Clear pending interactions when navigating away (optional - can be removed if you want persistence)
    window.addEventListener('beforeunload', function() {
        // Only clear if we're actually navigating to a different page
        // (not just refreshing)
        const currentUrl = window.location.href;
        // This is a simple check - you might want to enhance this
    });
});

