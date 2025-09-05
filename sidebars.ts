import type { SidebarsConfig } from "@docusaurus/plugin-content-docs";

// This runs in Node.js - Don't use client-side code here (browser APIs, JSX...)

/**
 * Creating a sidebar enables you to:
 - create an ordered group of docs
 - render a sidebar for each doc of that group
 - provide next/previous navigation

 The sidebars can be generated from the filesystem, or explicitly defined here.

 Create as many sidebars as you want.
 */
const sidebars: SidebarsConfig = {
  sidebar: [
    {
      type: "category",
      label: "Introduction",
      collapsed: false,
      collapsible: false,
      items: [
        { type: "doc", id: "overview", label: "🏠 Overview" },
        { type: "doc", id: "getting-started" },
        { type: "doc", id: "structure-of-stories" },
      ],
    },
    {
      type: "category",
      label: "Documentation",
      collapsed: false,
      collapsible: false,
      items: [
        { type: "doc", id: "enhancing-documentation" },
        { type: "doc", id: "custom-pages-and-markdown" },
      ],
    },
    {
      type: "category",
      label: "Configuration",
      collapsed: false,
      collapsible: false,
      items: [
        { type: "doc", id: "configure-layouts" },
        { type: "doc", id: "configure-arguments" },
        { type: "doc", id: "sorting-navigation-tree-items" },
      ],
    },
    {
      type: "category",
      label: "Appearance",
      collapsed: false,
      collapsible: false,
      items: [
        { type: "doc", id: "prefers-color-scheme" },
        { type: "doc", id: "customize-title-and-brand-logo", label: "🎀 Customize title and logo" },
      ],
    },
    {
      type: "category",
      label: "Development",
      collapsed: false,
      collapsible: false,
      items: [
        { type: "doc", id: "include-custom-css-and-js", label: "🪄 Include custom CSS and JS" },
        { type: "doc", id: "mcp-server-feature" },
        { type: "doc", id: "hot-reloading", label: "🔥 Hot Reloading [Preview]" },
      ],
    },
    {
      type: "category",
      label: "General Info",
      collapsed: false,
      collapsible: false,
      items: [
        { type: "doc", id: "disclaimer" },
        { type: "doc", id: "attention" },
        { type: "doc", id: "faq", label: "🤔 FAQ" },
        { type: "doc", id: "release-notes" },
        { type: "doc", id: "license-and-3rd-party-notice", label: "📢 License & 3rd Party Notice" },
      ],
    },
  ],
};

export default sidebars;
