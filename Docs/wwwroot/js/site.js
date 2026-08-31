window.blazingStoryDocs = (function () {
    function readStoredTheme() {
        try { return localStorage.getItem('theme'); } catch (e) { return null; }
    }

    function applyTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
    }

    return {
        getTheme: function () {
            return document.documentElement.getAttribute('data-theme') || 'light';
        },

        setTheme: function (theme) {
            applyTheme(theme);
            try { localStorage.setItem('theme', theme); } catch (e) { }
        },

        initializeTheme: function (dotNetReference) {
            window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', function (event) {
                // An explicit choice by the visitor always wins over the operating system setting.
                if (readStoredTheme()) return;
                const theme = event.matches ? 'dark' : 'light';
                applyTheme(theme);
                dotNetReference.invokeMethodAsync('OnSystemThemeChanged', theme);
            });
            return this.getTheme();
        },

        scrollToAnchor: function (id) {
            const element = id ? document.getElementById(id) : null;
            if (element) {
                element.scrollIntoView();
            } else {
                window.scrollTo({ top: 0 });
            }
        },

        highlightCode: function () {
            if (window.Prism) window.Prism.highlightAllUnder(document.body);
        }
    };
})();
