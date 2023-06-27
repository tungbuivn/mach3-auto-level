// See the Electron documentation for details on how to use preload scripts:
// https://www.electronjs.org/docs/latest/tutorial/process-model#preload-scripts
const { app, BrowserWindow,contextBridge,ipcRenderer , ipcMain} = require('electron')
contextBridge.exposeInMainWorld('versions', {
    node: () => process.versions.node,
    chrome: () => process.versions.chrome,
    electron: () => process.versions.electron,
    ping: () => ipcRenderer.invoke('ping'),
    getCurrentDir:()=>ipcRenderer.invoke('getWorkingDir'),
    getFiles:()=>ipcRenderer.invoke("getFiles"),
    runFlatCam:(...args)=>ipcRenderer.invoke('runFlatCam',...args),
    runHeightMap:(...args)=>ipcRenderer.invoke("runHeightMap",...args),
    runFusion360:(...args)=>ipcRenderer.invoke("runFusion360",...args),
    // we can also expose variables, not just functions
  })
