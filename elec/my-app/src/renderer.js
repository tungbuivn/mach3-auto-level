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
// const {clipboard} = require('electron')
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
function setCommandText(s) {
    document.getElementById('cmd').innerHTML=s;
}
function setLog(...args){
    document.getElementById("log").innerHTML=`<pre>${args.join("\n")}</pre>`;
}
function handleCall(fn) {
    singleProm=singleProm.then(async ()=>{
        setLog(...[]);
        var rs=await fn();
        rs=rs||[];
        setLog(...rs);
        await updateCurrentDir();
        if (typeof(rs[1])!="undefined" && rs[1]!="") {
           
        } else {
            alert("done");
        }
        
        return Promise.resolve();
    })
}
window.addEventListener("DOMContentLoaded", (event) => {
    updateCurrentDir();
    // document.getElementById('cmd').onclick=()=>{
    //     clipboard.writeText( document.getElementById('cmd').innerHTML);
    // }
    
    document.getElementById('setmap').onclick =() => {
        mapFile= document.getElementById('sel').innerHTML;
        document.getElementById('mapFile').innerHTML=mapFile;
        
       
    };
    document.getElementById('setnc').onclick =() => {
        
        ncFile= document.getElementById('sel').innerHTML;
        document.getElementById('ncFile').innerHTML=ncFile;
    };
    document.getElementById('flatcam').onclick = () => {
        handleCall(async()=>{
            setCommandText(`gcp ger`)
            return await  window.versions.runFlatCam();
            
        })
        
    };
    document.getElementById('heightMap').onclick =() => {
        handleCall(async()=>{
            setCommandText(`gcp map ${mapFile} ${ncFile}`)
            return await  window.versions.runHeightMap(mapFile,ncFile);
        })
       
    };
    document.getElementById('fusion360').onclick =() => {
        handleCall(async()=>{
            setCommandText(`gcp 360 ${ncFile}`)
             return await window.versions.runFusion360(  ncFile);
            
        })
       
        
    };
    
});


console.log('👋 This message is being logged by "renderer.js", included via webpack');
