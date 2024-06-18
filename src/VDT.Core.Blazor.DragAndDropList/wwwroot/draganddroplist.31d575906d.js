
function getElementHeights(container) {
    return Array.from(container.children).map(element => element.offsetHeight);
}

export { getElementHeights }
