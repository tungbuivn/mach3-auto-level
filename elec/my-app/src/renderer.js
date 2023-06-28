/**
 * This file will automatically be loaded by webpack and run in the "renderer" context.
 * To learn more about the differences between the "main" and the "renderer" context in
 * Electron, visit:
 *
 * https://electronjs.org/docs/tutorial/application-architecture#main-and-renderer-processes
 *
 * By default, Node.js integration in this file is disabled. When enabling Node.js integration
 * in a renderer process, please be aware of potential security implications. You can read
 * more about security risks here:
 *
 * https://electronjs.org/docs/tutorial/security
 *
 * To enable Node.js integration in this file, open up `main.js` and enable the `nodeIntegration`
 * flag:
 *
 * ```
 *  // Create the browser window.
 *  mainWindow = new BrowserWindow({
 *    width: 800,
 *    height: 600,
 *    webPreferences: {
 *      nodeIntegration: true
 *    }
 *  });
 * ```
 */

// import './index.css';
import styles from './index.css'
// import './popup.css'
const makerjs = require('makerjs');
// var Popup=require("./popup");
var { define, html, svg, store } = require("./hybrids");
var drawData = {
    count: 0,
    lines: [],
    gcode: []
}
const lsdata = {
    cmd: "aa"
}
const drawStore = {

    count: 0,

    lines: [""],
    gcode: [""]
}
var mapFile = "";
var ncFile = "";
var singleProm = Promise.resolve();

const updateCurrentDir = async () => {
    const response = await window.versions.getCurrentDir();
    document.getElementById('dir').innerHTML = response;
    const files = await window.versions.getFiles();
    var domFile = document.getElementById('files');
    domFile.innerHTML = "";
    var listFiles = [];

    files.forEach(o => {
        var el = document.createElement("div");
        el.setAttribute("class", "file");
        el.innerHTML = o;
        el.onclick = () => {
            // debugger;
            document.getElementById('sel').innerHTML = o;

            // myPopup.show();
        };
        domFile.appendChild(el);
        listFiles.push(el);
    });

    // console.log(response) // prints out 'pong'
}
function setCommandText(s) {
    document.getElementById('cmd').innerHTML = s;
}
function setLog(...args) {
    document.getElementById("log").innerHTML = `<pre>${args.join("\n")}</pre>`;
}
function handleCall(fn) {
    singleProm = singleProm.then(async () => {
        setLog(...[]);
        var rs = await fn();
        rs = rs || [];
        setLog(...rs);
        await updateCurrentDir();
        if (typeof (rs[1]) != "undefined" && rs[1] != "") {

        } else {
            alert("done");
        }

        return Promise.resolve();
    })
}
var count = 0;
window.addEventListener("DOMContentLoaded", (event) => {
    updateCurrentDir();

    // },1000)

    // document.getElementById('cmd').onclick=()=>{
    //     clipboard.writeText( document.getElementById('cmd').innerHTML);
    // }

    document.getElementById('setmap').onclick = () => {
        mapFile = document.getElementById('sel').innerHTML;
        document.getElementById('mapFile').innerHTML = mapFile;
        var data = window.versions.getHeightMapContent(mapFile);
        // setTimeout(()=>{
        data.then((lines) => {
            drawData.lines = lines;
            // drawData.lines

            // document.querySelector('my-draw').lines=lines;
            store.set(drawStore, { count: count++, lines: lines });
        })

    };
    document.getElementById('setnc').onclick = () => {

        ncFile = document.getElementById('sel').innerHTML;
        document.getElementById('ncFile').innerHTML = ncFile;

        var data = window.versions.getHeightMapContent(ncFile);
        // setTimeout(()=>{
        data.then((lines) => {
            drawData.gcode = lines;
            // drawData.lines

            // document.querySelector('my-draw').lines=lines;
            store.set(drawStore, { count: count++, gcode: lines });
        })
    };
    document.getElementById('flatcam').onclick = () => {
        handleCall(async () => {
            setCommandText(`gcp ger`)
            return await window.versions.runFlatCam();

        })

    };
    document.getElementById('heightMap').onclick = () => {
        handleCall(async () => {
            setCommandText(`gcp map ${mapFile} ${ncFile}`)
            return await window.versions.runHeightMap(mapFile, ncFile);
        })

    };
    document.getElementById('fusion360').onclick = () => {
        handleCall(async () => {
            setCommandText(`gcp 360 ${ncFile}`)
            return await window.versions.runFusion360(ncFile);

        })


    };


});
define({
    tag: "my-draw",
    name: 'xxxx',
    dataList: store(drawStore),
    // lines: "store.get(drawStore).count",
    render: ({ dataList }) => {

        return html`
        ${store.ready(dataList) && html`<div class="svg" innerHTML="${draw(dataList)}"></div>`}
        
    
        `.css`
        svg {
            width: 100%;
            height: ${document.querySelector('#files').clientHeight}px;
            // max-height: 100%;
          }
          
          #mapPoint {
            stroke: red;
          }
        `
    },
});
function draw(xxx) {
    // debugger;
    var lines = drawData.lines;
    var gcode = drawData.gcode;
    var svg = '';
    var model = {
        // id:"qqqqqqqq",
        // paths: pathArray,
        //  ,
        models: {
            //  measureRect: obj

        }
    };
    if (lines || false) {
        var m1 = lines.map(o => o.replace('\r', ""))
            .filter(o => o != "")
            .map(o => o.split(/[,\sxyz]/gi).filter(p => p != ""))

        // debugger;
        var pathArray = m1
            .map(e => {

                return {
                    type: 'circle',
                    origin: [parseFloat(e[0]), parseFloat(e[1])],
                    radius: 0.2
                };
            });
        model.models.mapPoint = {
            paths: pathArray
        }

        // var pathArray = [ line, circle ];


    }
    if (gcode || false) {
        function extract(sig, i) {
            if (i < 0) return false;
            var re = new RegExp("(" + sig + ".[^\\sa-z]*)", "i");
            var ma = gcode[i].match(re);
            if (ma) {
                return ma[0].substr(1);
            } else {

                return extract(sig, i - 1);
            }
        }
        var gce = [];
        for (var i = 0; i < gcode.length; i++) {
            // debugger;
            gcode[i] = gcode[i].replace(/[\r\n]/gi, "").toUpperCase();
            if (gcode[i] == "") continue;
            gcode[i] = gcode[i].split(/[;\(]/gi)[0];
            if (gcode[i] != "") {
                // debugger;
                var m = extract('G', i);
                // debugger;
                if (m) {
                    // debugger;
                    var ind = parseInt(m);
                    switch (ind) {
                        case 1:
                            // debugger;
                            var x = extract('x', i);
                            var y = extract('y', i);

                            var x1 = extract('x', i - 1);
                            var y1 = extract('y', i - 1);
                            if (x && y && x1 && y1) {

                                var p = [parseFloat(x), parseFloat(y), parseFloat(x1), parseFloat(y1)].filter(o => !isNaN(o));
                                if (p.length == 4) {
                                    // debugger;
                                    gce.push({

                                        type: 'line',
                                        origin: [p[0], p[1]],

                                        end: [p[2], p[3]]
                                        // radius: 0.2

                                    })
                                }

                                // debugger;
                            }
                            break;
                        // g02
                        case 2:
                        case 3:
                            var points = [extract('x', i - 1), extract('y', i - 1), extract('x', i), extract('y', i)].filter(o => o !== false);
                            if (points.length == 4) {
                                points = points.map(o => parseFloat(o)).filter(o => !isNaN(o));
                                // check i,j or r
                                var pij = [extract('i', i), extract('j', i)].filter(o => o !== false);
                                if (points.length == 4 && pij.length == 2) {
                                    pij = pij.map(o => parseFloat(o)).filter(o => !isNaN(o));
                                    if (pij.length == 2) {
                                        // return ccw angle
                                        function angle(x1, y1, x2, y2) {
                                            [x1, y1, x2, y2] = [x2, y2, x1, y1];
                                            var dot = x1 * x2 + y1 * y2;      // Dot product between[v1x, v1y] and[v2x, v2y]
                                            var det = x1 * y2 - y1 * x2;      // Determinant
                                            var angle = Math.atan2(det, dot);
                                            var goc = angle * 180 / Math.PI;
                                            if (det > 0) {
                                                // ccw from v1 to v2 
                                                // debugger;
                                            } else {
                                                // cw from v1 to v2
                                                angle += Math.PI * 2;
                                            }

                                            return angle * 180 / Math.PI;
                                        }

                                        var cenx = points[0] + pij[0];
                                        var ceny = points[1] + pij[1];
                                        var v1x = -pij[0];
                                        var v1y = -pij[1];
                                        var v2x = points[2] - cenx;
                                        var v2y = points[3] - ceny;
                                        var p = [v1x, v1y, v2x, v2y];
                                        var dist = Math.sqrt(v1x * v1x + v1y * v1y);
                                        // angle(-1, -1, 1, 0);
                                        // [v2x, v2y, v1x, v1y] = p;
                                        var start = angle(v1x, v1y, 1, 0);
                                        var end = angle(v2x, v2y, 1, 0);
                                        // debugger;
                                        var ma = end * 180 / Math.PI;  // atan2(y, x) or atan2(sin, cos)
                                        // g2 start<end
                                        // if (ind == 3) {
                                        //     if (start < end) end = end - 360;
                                        // } else {
                                        //     if (end < start) end = end + 360;
                                        // }
                                        // var xxx = dist * Math.cos(end) + cenx;
                                        // debugger;
                                        var arc = {
                                            type: 'arc',
                                            origin: [cenx, ceny],
                                            radius: dist,
                                            startAngle: ind == 2 ? end : start,
                                            endAngle: ind == 2 ? start : end
                                            // startAngle: end,
                                            // endAngle: start
                                        };
                                        gce.push(arc);
                                    }
                                }
                            }
                            break;
                    }

                }
            }

        }
        model.models.gcode = {
            paths: gce
        }
    }
    // debugger;
    var svg = makerjs.exporter.toSVG(model, { useSvgPathOnly: false });
    return svg;
}



console.log('👋 This message is being logged by "renderer.js", included via webpack');
