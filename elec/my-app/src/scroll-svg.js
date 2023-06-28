module.exports = (dom) => {
    const svg = dom;// document.querySelector('svg');
    if (svg == null) return;
    // zooming
    svg.onwheel = function (event) {
        event.preventDefault();

        // set the scaling factor (and make sure it's at least 10%)
        let scale = event.deltaY / 1000;
        scale = Math.abs(scale) < .2 ? .2 * event.deltaY / Math.abs(event.deltaY) : scale;

        // get point in SVG space
        let pt = new DOMPoint(event.clientX, event.clientY);
        pt = pt.matrixTransform(svg.getScreenCTM().inverse());

        // get viewbox transform
        let [x, y, width, height] = svg.getAttribute('viewBox').split(' ').map(Number);

        // get pt.x as a proportion of width and pt.y as proportion of height
        let [xPropW, yPropH] = [(pt.x - x) / width, (pt.y - y) / height];

        // calc new width and height, new x2, y2 (using proportions and new width and height)
        let [width2, height2] = [width + width * scale, height + height * scale];
        let x2 = pt.x - xPropW * width2;
        let y2 = pt.y - yPropH * height2;

        svg.setAttribute('viewBox', `${x2} ${y2} ${width2} ${height2}`);
    }
};