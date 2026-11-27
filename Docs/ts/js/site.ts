import type { Theme } from '../types/blazing-story-docs.d.ts';
import type { DotNetObjectReference } from '../types/blazor.d.ts';

const applyTheme = (theme: Theme) => document.documentElement.setAttribute('data-theme', theme);

export const getTheme = () => document.documentElement.getAttribute('data-theme') || 'light';

export const setTheme = (theme: Theme) => {
    applyTheme(theme);
    localStorage.setItem('theme', theme);
}

export const initializeTheme = (dotNetReference: DotNetObjectReference) => {
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (event) => {
        // An explicit choice by the visitor always wins over the operating system setting.
        if (!localStorage.getItem('theme')) return;
        const theme = event.matches ? 'dark' : 'light';
        applyTheme(theme);
        dotNetReference.invokeMethodAsync('OnSystemThemeChanged', theme);
    });
    return getTheme();
};

// "navigator.platform" is deprecated, but "navigator.userAgentData" that replaces it is Chromium only.
export const isApplePlatform = () => /Mac|iPhone|iPad|iPod/.test(navigator.platform);

export const focusAndSelect = (element: HTMLInputElement) => {
    element.focus();
    element.select();
};

// Returns whether the request could be carried out. A document that is still being fetched
// has no heading in the page yet, so the caller has to ask again after the next render
// instead of giving up and leaving the visitor at the top of the page.
export const scrollToAnchor = (id: string | null | undefined) => {
    if (!id) {
        window.scrollTo({ top: 0 });
        return true;
    }

    const element = document.getElementById(id);
    if (!element) return false;

    element.scrollIntoView();
    return true;
};
