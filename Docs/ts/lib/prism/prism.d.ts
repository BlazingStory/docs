interface PrismStatic {
    highlightAll(async?: boolean, callback?: (element: Element) => void): void;
    highlightAllUnder(container: ParentNode, async?: boolean, callback?: (element: Element) => void): void;
    highlightElement(element: Element, async?: boolean, callback?: (element: Element) => void): void;
}

export declare const Prism: PrismStatic;
