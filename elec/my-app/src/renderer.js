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

import './index.css';
var mapFile="";
var ncFile="";
var singleProm=Promise.resolve();
const updateCurrentDir = async () => {
    const response = await window.versions.getCurrentDir();
    document.getElementById('dir').innerHTML = response;
    const files = await window.versions.getFiles();
    var domFile = document.getElementById('files');
    domFile.innerHTML="";
    var listFiles = [];
    files.forEach(o => {
        var el = document.createElement("div");
        el.setAttribute("class","file");
        el.innerHTML = o;
        el.onclick = () => {
            document.getElementById('sel').innerHTML = o;
        };
        domFile.appendChild(el);
        listFiles.push(el);
    });

    // console.log(response) // prints out 'pong'
}
window.addEventListener("DOMContentLoaded", (event) => {
    updateCurrentDir();
    document.getElementById('setmap').onclick =() => {
        mapFile= document.getElementById('sel').innerHTML;
        document.getElementById('mapFile').innerHTML=mapFile;
        
       
    };
    document.getElementById('setnc').onclick =() => {
        
        ncFile= document.getElementById('sel').innerHTML;
        document.getElementById('ncFile').innerHTML=ncFile;
    };
    document.getElementById('flatcam').onclick = () => {
        singleProm=singleProm.then(async ()=>{
            await  window.versions.runFlatCam();
            await updateCurrentDir();
            alert("done");
            return Promise.resolve();
        })
        
    };
    document.getElementById('heightMap').onclick =() => {
        singleProm=singleProm.then(async ()=>{
            await  window.versions.runHeightMap(  mapFile,ncFile||"");
            await updateCurrentDir();
            alert("done");
            return Promise.resolve();
        })
       
    };
    document.getElementById('fusion360').onclick =() => {
        singleProm=singleProm.then(async ()=>{
            await window.versions.runFusion360(  ncFile);
            await updateCurrentDir();
            alert("done");
            return Promise.resolve();
        })
        
    };
    
});


console.log('👋 This message is being logged by "renderer.js", included via webpack');
