/**
 * Shared utilities for AppSage webview components
 * Contains globally useful functions that can be used across all webview components
 */

/**
 * Simple HTML sanitization utility
 * Escapes HTML characters to prevent XSS
 * @param {string|null|undefined} str - The string to sanitize
 * @returns {string} - The sanitized string
 */
function sanitize(str) {
    if (str == null) return '';
    
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#x27;');
}

// Make it globally available
if (typeof window !== 'undefined') {
    window.sanitize = sanitize;
}

// Export for modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        sanitize
    };
}
