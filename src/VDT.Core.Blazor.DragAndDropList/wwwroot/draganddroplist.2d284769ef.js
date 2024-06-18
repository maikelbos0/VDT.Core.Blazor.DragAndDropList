
function getElementHeights(container) {
    const heights = [];

    for (const element of container.children) {
        heights.push(element.offsetHeight);
    }

    return heights;
}

export { getElementHeights }
