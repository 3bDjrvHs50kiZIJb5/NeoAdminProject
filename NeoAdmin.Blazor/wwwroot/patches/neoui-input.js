// Input component JavaScript module
// Manages input event handling, validation tooltips, error states, and UpdateOn mode behavior
//
// NeoAdmin override (NeoUI.Blazor 4.1.7): skip Blazor sync during IME composition to avoid
// pinyin letters being committed when Blazor Server round-trips are slow.

// Track elements with active input handling (event listeners, debounce timers, etc.)
const inputState = new Map();

// Track elements with active validation and their update modes
const validationState = new Map();

/**
 * Safely invoke a .NET method with consistent error handling
 * @param {object} dotNetRef - The DotNetObjectReference
 * @param {string} methodName - The method name to invoke
 * @param  {...any} args - Arguments to pass to the method
 * @returns {Promise} - The result of the invocation or undefined on error
 */
async function safeInvoke(dotNetRef, methodName, ...args) {
    if (!dotNetRef) {
        console.warn(`Cannot invoke ${methodName}: dotNetRef is null`);
        return;
    }
    
    try {
        return await dotNetRef.invokeMethodAsync(methodName, ...args);
    } catch (err) {
        // Ignore disposed object errors (normal during component disposal)
        if (err.message?.includes('disposed') || err.message?.includes('released')) {
            return;
        }
        console.error(`Error invoking ${methodName}:`, err);
        throw err;
    }
}

/**
 * Deliver an input value to Blazor (with optional debounce).
 * @param {object} state - Per-element input state
 * @param {string} elementId - The element ID
 * @param {object} dotNetRef - DotNetObjectReference for callbacks
 * @param {string} value - Current element value
 */
function deliverInputValue(state, elementId, dotNetRef, value) {
    if (value === state.lastSentValue) {
        if (state.debounceTimer) {
            clearTimeout(state.debounceTimer);
            state.debounceTimer = null;
        }
        return;
    }

    if (state.debounceDelay > 0) {
        if (state.debounceTimer) {
            clearTimeout(state.debounceTimer);
        }

        state.debounceTimer = setTimeout(() => {
            const currentState = inputState.get(elementId);
            if (currentState && currentState.dotNetRef) {
                currentState.lastSentValue = value;
                currentState.debounceTimer = null;
                safeInvoke(currentState.dotNetRef, 'OnInputChanged', value);
            }
        }, state.debounceDelay);
    } else {
        state.lastSentValue = value;
        safeInvoke(dotNetRef, 'OnInputChanged', value);
    }
}

/**
 * Initialize validation tracking for an element
 * @param {string} elementId - The element ID
 * @param {string} updateOn - 'input' or 'change'
 */
export function initializeValidation(elementId, updateOn = 'input') {
    const element = document.getElementById(elementId);
    if (!element) return;

    disposeValidation(elementId);

    const state = {
        updateOn: updateOn.toLowerCase(),
        hasError: false,
        inputHandler: null
    };

    if (state.updateOn === 'change') {
        state.inputHandler = () => {
            if (state.hasError) {
                element.setCustomValidity('');
                state.hasError = false;
            }
        };
        element.addEventListener('input', state.inputHandler);
    }

    validationState.set(elementId, state);
}

export function disposeValidation(elementId) {
    const element = document.getElementById(elementId);
    const state = validationState.get(elementId);
    
    if (element) {
        element.setCustomValidity('');
        if (state && state.inputHandler) {
            element.removeEventListener('input', state.inputHandler);
        }
    }
    
    validationState.delete(elementId);
}

export function setValidationError(elementId, message, shouldFocus = true) {
    const element = document.getElementById(elementId);
    if (!element) return;

    element.setCustomValidity(message);
    
    if (shouldFocus) {
      requestAnimationFrame(() => {
        element.reportValidity();
        element.focus();
      });
    }

    const state = validationState.get(elementId);
    if (state) {
        state.hasError = true;
    }
}

export function setValidationErrorSilent(elementId, message) {
    const element = document.getElementById(elementId);
    if (!element) return;

    element.setCustomValidity(message);

    const state = validationState.get(elementId);
    if (state) {
        state.hasError = true;
    }
}

export function clearValidationError(elementId) {
    const element = document.getElementById(elementId);
    if (!element) return;

    element.setCustomValidity('');

    const state = validationState.get(elementId);
    if (state) {
        state.hasError = false;
    }
}

export function showValidationError(elementId, message) {
    setValidationError(elementId, message);
}

export function initializeInput(elementId, updateOn = 'change', debounceDelay = 0, dotNetRef = null, enableBlurValidation = false) {
    const element = document.getElementById(elementId);
    if (!element) {
        console.warn(`Input element with id '${elementId}' not found`);
        return;
    }

    disposeInput(elementId);

    const state = {
        updateOn: updateOn.toLowerCase(),
        debounceDelay: debounceDelay,
        dotNetRef: dotNetRef,
        enableBlurValidation: enableBlurValidation,
        debounceTimer: null,
        inputHandler: null,
        changeHandler: null,
        blurHandler: null,
        compositionStartHandler: null,
        compositionEndHandler: null,
        isComposing: false,
        lastSentValue: element.value
    };

    if (state.updateOn === 'input' && dotNetRef) {
        state.inputHandler = (e) => {
            if (e.isComposing || state.isComposing) {
                return;
            }

            deliverInputValue(state, elementId, dotNetRef, e.target.value);
        };
        element.addEventListener('input', state.inputHandler);

        state.compositionStartHandler = () => {
            state.isComposing = true;
            if (state.debounceTimer) {
                clearTimeout(state.debounceTimer);
                state.debounceTimer = null;
            }
        };
        state.compositionEndHandler = (e) => {
            state.isComposing = false;
            deliverInputValue(state, elementId, dotNetRef, e.target.value);
        };
        element.addEventListener('compositionstart', state.compositionStartHandler);
        element.addEventListener('compositionend', state.compositionEndHandler);
    }

    if (dotNetRef) {
        state.changeHandler = (e) => {
            const value = e.target.value;
            
            if (state.updateOn === 'change') {
                state.lastSentValue = value;
                safeInvoke(dotNetRef, 'OnInputChanged', value);
            }
        };
        element.addEventListener('change', state.changeHandler);

        state.blurHandler = (e) => {
            const value = e.target.value;
            
            if (state.debounceTimer) {
                clearTimeout(state.debounceTimer);
                state.debounceTimer = null;
            }

            state.isComposing = false;
            
            if (state.enableBlurValidation) {
                safeInvoke(dotNetRef, 'ValidateAndClamp');
            }
            else if (state.updateOn === 'input') {
                if (value !== state.lastSentValue) {
                    state.lastSentValue = value;
                    safeInvoke(dotNetRef, 'OnInputChanged', value);
                }
            }
        };
        element.addEventListener('blur', state.blurHandler);
    }

    inputState.set(elementId, state);
}

export function updateValue(elementId, value) {
    const element = document.getElementById(elementId);
    if (!element) return;

    const state = inputState.get(elementId);

    if (document.activeElement === element && element.value !== (value || '')) return;
    if (state?.isComposing) return;

    if (element.value !== value) {
        element.value = value || '';
    }

    if (state) state.lastSentValue = value || '';
}

export function disposeInput(elementId) {
    const state = inputState.get(elementId);
    if (!state) return;

    const element = document.getElementById(elementId);
    
    if (state.debounceTimer) {
        clearTimeout(state.debounceTimer);
    }

    if (element) {
        if (state.inputHandler) {
            element.removeEventListener('input', state.inputHandler);
        }
        if (state.compositionStartHandler) {
            element.removeEventListener('compositionstart', state.compositionStartHandler);
        }
        if (state.compositionEndHandler) {
            element.removeEventListener('compositionend', state.compositionEndHandler);
        }
        if (state.changeHandler) {
            element.removeEventListener('change', state.changeHandler);
        }
        if (state.blurHandler) {
            element.removeEventListener('blur', state.blurHandler);
        }
    }

    inputState.delete(elementId);
}

const commandInputState = new Map();

export function initializeCommandInput(elementId, dotNetRef, autoFocus = false) {
    const element = document.getElementById(elementId);
    if (!element) {
        console.warn(`Command input element with id '${elementId}' not found`);
        return;
    }

    disposeCommandInput(elementId);

    const navigationKeys = ['ArrowDown', 'ArrowUp', 'Home', 'End', 'Enter'];

    const keydownHandler = (e) => {
        if (navigationKeys.includes(e.key)) {
            e.preventDefault();
            safeInvoke(dotNetRef, 'HandleNavigationKey', e.key);
        }
    };

    element.addEventListener('keydown', keydownHandler, { capture: true });

    commandInputState.set(elementId, { keydownHandler });

    if (autoFocus) {
      setTimeout(() =>
        requestAnimationFrame(() => {
            element.focus();
        }), 10);
    }
}

export function focusCommandInput(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.focus();
    }
}

export function disposeCommandInput(elementId) {
    const state = commandInputState.get(elementId);
    if (!state) return;

    const element = document.getElementById(elementId);
    if (element && state.keydownHandler) {
        element.removeEventListener('keydown', state.keydownHandler, { capture: true });
    }

    commandInputState.delete(elementId);
}

const maxLengthState = new Map();

export function enforceMaxLength(elementId, maxLength) {
    const element = document.getElementById(elementId);
    if (!element) {
        console.warn(`Element with id '${elementId}' not found for maxLength enforcement`);
        return;
    }

    disposeMaxLength(elementId);

    const inputHandler = (e) => {
        if (e.isComposing) {
            return;
        }

        const value = e.target.value;
        
        if (value.length > maxLength) {
            e.target.value = value.slice(0, maxLength);
            const newEvent = new Event('input', { bubbles: true });
            e.target.dispatchEvent(newEvent);
            e.stopImmediatePropagation();
        }
    };

    element.addEventListener('input', inputHandler, { capture: true });

    maxLengthState.set(elementId, { inputHandler });
}

export function disposeMaxLength(elementId) {
    const state = maxLengthState.get(elementId);
    if (!state) return;

    const element = document.getElementById(elementId);
    if (element && state.inputHandler) {
        element.removeEventListener('input', state.inputHandler, { capture: true });
    }

    maxLengthState.delete(elementId);
}
