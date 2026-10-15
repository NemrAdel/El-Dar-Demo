const electron = require('electron');
const path = require('path');
require('dotenv').config();
const { autoUpdater } = require('electron-updater');
const { dialog, BrowserWindow, app } = electron;
const fs = require('fs');
const { spawn } = require('child_process');
const http = require('http');

require('dotenv').config({
    path: app.isPackaged
        ? path.join(process.resourcesPath, '.env')
        : path.join(__dirname, '.env')
});

autoUpdater.logger = require('electron-log');
autoUpdater.logger.transports.file.level = 'info';
autoUpdater.logger.info("🚀 El-Dar System Has Started Successfully!");

let serverProcess = null;

function initializeDatabaseFiles() {
    const userDataPath = app.getPath('userData');
    const dbFolder = path.join(userDataPath, 'data');
    const dbFiles = 'ElDar.db';
    const targetPath = path.join(dbFolder, dbFiles);
    const sourcePath = path.join(__dirname, 'data', dbFiles);

    autoUpdater.logger.info(`📁 [DEBUG] UserData Path: ${userDataPath}`);
    autoUpdater.logger.info(`🎯 [DEBUG] Target Path: ${targetPath}`);
    autoUpdater.logger.info(`📦 [DEBUG] Source Path: ${sourcePath}`);

    if (!fs.existsSync(dbFolder)) {
        fs.mkdirSync(dbFolder, { recursive: true });
    }

    if (fs.existsSync(sourcePath)) {
        try {
            let shouldCopy = false;
            if (!fs.existsSync(targetPath)) {
                shouldCopy = true;
            } else {
                const sourceStats = fs.statSync(sourcePath);
                const targetStats = fs.statSync(targetPath);

                if (targetStats.size < sourceStats.size) {
                    shouldCopy = true;
                    autoUpdater.logger.info(`🔄 [INFO] تم اكتشاف ملف قديم أو غير مكتمل. سيتم استبداله بالملف الأصلي.`);
                }
            }

            if (shouldCopy) {
                fs.copyFileSync(sourcePath, targetPath);
                autoUpdater.logger.info(`✅ [SUCCESS] تم نقل ملف قاعدة البيانات بنجاح إلى: ${targetPath}`);
            } else {
                autoUpdater.logger.info(`ℹ️ [INFO] ملف قاعدة البيانات متوفر ومساحته مطابقة.`);
            }

        } catch (err) {
            autoUpdater.logger.error(`❌ [ERROR] فشل نقل الملف: ${err.message}`);
        }
    } else {
        autoUpdater.logger.warn(`⚠️ [WARN] ملف قاعدة البيانات الأولية غير موجود في: ${sourcePath}`);
    }
}

if (app) {
    initializeDatabaseFiles();
}

function handleShortcuts(webContents) {
    webContents.on('before-input-event', (event, input) => {
        if (input.control && (input.key === '+' || input.key === '=')) {
            const currentZoom = webContents.getZoomFactor();
            webContents.setZoomFactor(currentZoom + 0.1);
            event.preventDefault();
        }
        if (input.control && input.key === '-') {
            const currentZoom = webContents.getZoomFactor();
            webContents.setZoomFactor(currentZoom - 0.1);
            event.preventDefault();
        }
        if (input.key === 'F12') {
            webContents.toggleDevTools();
        }
    });
}

function startServerAndLoad(mainWindow) {
    const serverUrl = 'http://localhost:3005';

    const checkAndLoad = () => {
        http.get(serverUrl, () => {
            mainWindow.loadFile(path.join(__dirname, 'index.html'));
        }).on('error', () => {
            setTimeout(checkAndLoad, 500);
        });
    };

    if (app.isPackaged) {
        const serverExecutable = path.join(process.resourcesPath, 'publish', 'ElDarPl.exe');

        if (fs.existsSync(serverExecutable)) {
            serverProcess = spawn(serverExecutable, ['--electron'], {
                cwd: path.join(process.resourcesPath, 'publish'),
                env: {
                    ...process.env,
                    ASPNETCORE_URLS: 'http://localhost:3005'
                }
            });

            serverProcess.on('error', (err) => {
                autoUpdater.logger.error(`❌ [Failed To Run Server]: ${err.message}`);
            });

            serverProcess.stdout.on('data', (data) => {
                autoUpdater.logger.info(`[Server]: ${data}`);
            });

            serverProcess.stderr.on('data', (data) => {
                autoUpdater.logger.error(`[Server ERROR]: ${data}`);
            });
        } else {
            autoUpdater.logger.error(`❌ [File Of Server Not Found]: ${serverExecutable}`);
            dialog.showErrorBox('File Not Found', `The server file was not found:\n${serverExecutable}`);
            return;
        }
    }

    checkAndLoad();
}

function createWindow() {
    const mainWindow = new BrowserWindow({
        width: 1200,
        height: 800,
        backgroundColor: '#1a1a1a',
        autoHideMenuBar: true,
        icon: path.join(__dirname, 'el-dar.ico'),
        webPreferences: {
            nodeIntegration: true,
            contextIsolation: false,
            webviewTag: true
        }
    });

    handleShortcuts(mainWindow.webContents);

    startServerAndLoad(mainWindow);

    mainWindow.on('closed', () => {
        if (serverProcess) serverProcess.kill();
        app.quit();
    });
}

app.whenReady().then(() => {
    createWindow();

    app.on('web-contents-created', (event, webContents) => {
        handleShortcuts(webContents);
    });

    if (app.isPackaged) {
        autoUpdater.setFeedURL({
            provider: 'github',
            owner: 'NemrAdel',
            repo: 'El-Dar',
            private: true,
            token: process.env.GH_TOKEN
        });
        autoUpdater.disableDifferentialDownload = true;
        autoUpdater.checkForUpdatesAndNotify();
    }
});

app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) createWindow();
});

app.on('window-all-closed', () => {
    if (serverProcess) serverProcess.kill();
    if (process.platform !== 'darwin') {
        app.quit();
    }
});

autoUpdater.on('update-available', () => {
    dialog.showMessageBox({
        type: 'info',
        title: '✨ Update Available 🚀',
        message: '⚡ A new version has been found! Downloading the updates in the background... Please wait. ⏳',
        buttons: ['OK 👍']
    });
});

autoUpdater.on('download-progress', (progressObj) => {
    autoUpdater.logger.info(`⏬ Download speed: ${progressObj.bytesPerSecond} - Downloaded ${Math.round(progressObj.percent)}%`);
});

autoUpdater.on('update-downloaded', () => {
    dialog.showMessageBox({
        type: 'info',
        title: 'Update Ready 🎉',
        message: '✅ The new version has been successfully downloaded! The application will restart now to apply the updates. 🔄',
        buttons: ['Install & Restart 🛠️']
    }).then(() => {
        autoUpdater.quitAndInstall();
    });
});

autoUpdater.on('error', (err) => {
    autoUpdater.logger.error("❌ Auto-update error:", err);
    dialog.showMessageBox({
        type: 'error',
        title: 'Update Error ❌',
        message: `An error occurred while downloading the update:\n\n${err.stack || err.message || err.toString()}`,
        buttons: ['OK']
    });
});