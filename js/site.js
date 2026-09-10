import { Prism } from '../lib/prism/prism.js';
const applyTheme = (theme) => document.documentElement.setAttribute('data-theme', theme);
export const getTheme = () => document.documentElement.getAttribute('data-theme') || 'light';
export const setTheme = (theme) => {
    applyTheme(theme);
    localStorage.setItem('theme', theme);
};
export const initializeTheme = (dotNetReference) => {
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (event) => {
        if (!localStorage.getItem('theme'))
            return;
        const theme = event.matches ? 'dark' : 'light';
        applyTheme(theme);
        dotNetReference.invokeMethodAsync('OnSystemThemeChanged', theme);
    });
    return getTheme();
};
export const scrollToAnchor = (id) => {
    if (!id) {
        window.scrollTo({ top: 0 });
        return true;
    }
    const element = document.getElementById(id);
    if (!element)
        return false;
    element.scrollIntoView();
    return true;
};
export const highlightCode = () => Prism.highlightAllUnder(document.body);
