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
});