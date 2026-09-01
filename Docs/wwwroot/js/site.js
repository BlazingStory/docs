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

        // Returns whether the request could be carried out. A document that is still being fetched
        // has no heading in the page yet, so the caller has to ask again after the next render
        // instead of giving up and leaving the visitor at the top of the page.
        scrollToAnchor: function (id) {
            if (!id) {
                window.scrollTo({ top: 0 });
                return true;
            }

            const element = document.getElementById(id);
            if (!element) return false;

            element.scrollIntoView();
            return true;
        },

        highlightCode: function () {
            if (window.Prism) window.Prism.highlightAllUnder(document.body);
        }
    };
})();
