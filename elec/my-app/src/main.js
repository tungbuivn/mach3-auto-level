const { app, BrowserWindow, ipcMain } = require('electron');
const fs = require("fs");
const child = require("child_process");
const path = require('path');

// Handle creating/removing shortcuts on Windows when installing/uninstalling.
if (require('electron-squirrel-startup')) {
  app.quit();
}

const createWindow = () => {
  // Create the browser window.
  const mainWindow = new BrowserWindow({
    width: 1280,
    height: 960,
    // autoHideMenuBar: true,
    webPreferences: {
      preload: MAIN_WINDOW_PRELOAD_WEBPACK_ENTRY,
    },
  });

  // and load the index.html of the app.
  mainWindow.loadURL(MAIN_WINDOW_WEBPACK_ENTRY);

  // Open the DevTools.
  // mainWindow.webContents.openDevTools();
  return mainWindow;
};
// process.chdir("c:/0pcb/Gerber_PCB_test_2023-02-04/")
app.whenReady().then(() => {

  // var focusedWindow = BrowserWindow.getFocusedWindow();
  // focusedWindow.webContents.executeJavaScript("func();");
  // var wnd=createWindow()
  // wnd.webContents.executeJavaScript("alert(1);");
  ipcMain.handle('ping', () => {
    var a = 1;
    return 'pong';
  });
  ipcMain.handle('getWorkingDir', () => {

    return process.cwd();
  })
  function runapp(...args) {
    var result = child.spawnSync("d:/0cnc-app/bin/GCodeProcess.exe", args, {

      encoding: 'utf-8',

    });
    return [result.stdout.toString(), result.stderr.toString()];
  }
  ipcMain.handle('runFlatCam', (evt, ...args) => {
    return runapp("ger", ...args)
  })
  ipcMain.handle('runHeightMap', (evt, ...args) => {
    return runapp("map", ...args)
  })
  ipcMain.handle('runFusion360', (evt, ...args) => {
    return runapp("360", ...args)
  })
  ipcMain.handle('getFiles', () => {
    return fs.readdirSync(process.cwd()).filter(o => !fs.lstatSync(o).isDirectory());
  })
  ipcMain.handle("getHeightMapContent", (evt, file) => {
    return (fs.readFileSync(process.cwd() + "/" + file) + "").split("\n");
  })

})
// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
app.on('ready', createWindow);

// Quit when all windows are closed, except on macOS. There, it's common
// for applications and their menu bar to stay active until the user quits
// explicitly with Cmd + Q.
app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('activate', () => {
  // On OS X it's common to re-create a window in the app when the
  // dock icon is clicked and there are no other windows open.
  if (BrowserWindow.getAllWindows().length === 0) {
    createWindow();
  }
});

// In this file you can include the rest of your app's specific main process
// code. You can also put them in separate files and import them here.
