/**
 * Custom Dropdown Functionality
 * Baseball Monster - 2025
 * 
 * Usage:
 *   <script src="custom-dropdown.js"></script>
 * 
 * HTML Structure Required:
 *   <div class="bm-custom-select" data-name="filterName">
 *     <div class="custom-select-trigger">
 *       <span class="custom-select-value">Default Option</span>
 *       <span class="custom-select-arrow"></span>
 *     </div>
 *     <div class="custom-select-options">
 *       <div class="custom-select-option selected" data-value="value1">Option 1</div>
 *       <div class="custom-select-option" data-value="value2">Option 2</div>
 *     </div>
 *     <select name="filterName">
 *       <option value="value1" selected>Option 1</option>
 *       <option value="value2">Option 2</option>
 *     </select>
 *   </div>
 */

(function() {
    'use strict';
    
    // Initialize all custom dropdowns on the page
    function initCustomDropdowns() {
        document.querySelectorAll('.bm-custom-select').forEach(customSelect => {
            const trigger = customSelect.querySelector('.bm-custom-select-trigger');
            const options = customSelect.querySelector('.bm-custom-select-options');
            const valueDisplay = customSelect.querySelector('.bm-custom-select-value');
            const hiddenSelect = customSelect.querySelector('select');
            
            if (!trigger || !options || !valueDisplay || !hiddenSelect) {
                console.warn('Custom dropdown missing required elements:', customSelect);
                return;
            }
            
            // Toggle dropdown open/close
            trigger.addEventListener('click', function(e) {
                e.stopPropagation();
                
                // Close other dropdowns
                document.querySelectorAll('.bm-custom-select.open').forEach(other => {
                    if (other !== customSelect) {
                        other.classList.remove('open');
                    }
                });
                
                // Toggle this dropdown
                customSelect.classList.toggle('open');
            });
            
            // Handle option selection
            options.querySelectorAll('.bm-custom-select-option').forEach(option => {
                option.addEventListener('click', function(e) {
                    e.stopPropagation();
                    
                    // Remove selected class from all options
                    options.querySelectorAll('.bm-custom-select-option').forEach(opt => {
                        opt.classList.remove('selected');
                    });
                    
                    // Add selected class to clicked option
                    option.classList.add('selected');
                    
                    // Update display value
                    valueDisplay.textContent = option.textContent;
                    
                    // Update hidden select value
                    const value = option.getAttribute('data-value');
                    hiddenSelect.value = value;
                    
                    // Trigger change event on hidden select
                    const changeEvent = new Event('change', { bubbles: true });
                    hiddenSelect.dispatchEvent(changeEvent);
                    
                    // Close dropdown
                    customSelect.classList.remove('open');
                    
                    console.log('Dropdown selection changed:', {
                        name: customSelect.getAttribute('data-name'),
                        value: value,
                        text: option.textContent
                    });
                });
            });
        });
        
        // Close all dropdowns when clicking outside
        document.addEventListener('click', function() {
            document.querySelectorAll('.bm-custom-select.open').forEach(select => {
                select.classList.remove('open');
            });
        });
    }
    
    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initCustomDropdowns);
    } else {
        initCustomDropdowns();
    }
    
    // Expose function for manual initialization (if needed after dynamic content)
    window.initCustomDropdowns = initCustomDropdowns;
    
})();