import * as vscode from 'vscode';
import { ProfileManager } from './profileManager';
import { AppSageProfile } from '../types/profileTypes';

export class ProfileStatusBarManager {
    private statusBarItem: vscode.StatusBarItem;
    private profileManager: ProfileManager;

    constructor(profileManager: ProfileManager) {
        this.profileManager = profileManager;
        this.statusBarItem = vscode.window.createStatusBarItem(
            vscode.StatusBarAlignment.Left, 
            100
        );
        
        this.statusBarItem.command = 'appsage.selectProfileFromStatusBar';
        this.updateStatusBar();
        this.statusBarItem.show();
    }

    public updateStatusBar(): void {
        const activeProfile = this.profileManager.getActiveProfile();
        
        if (activeProfile) {
            this.statusBarItem.text = `$(circle-filled) AppSage: ${activeProfile.name}`;
            this.statusBarItem.tooltip = `Active AppSage Profile: ${activeProfile.name}\nWorkspace: ${activeProfile.workspacePath}\nOutput: ${activeProfile.outputPath}\n\nClick to switch profiles`;
        } else {
            this.statusBarItem.text = `$(circle-outline) AppSage: No Profile`;
            this.statusBarItem.tooltip = 'No active AppSage profile. Click to create or select a profile.';
        }
    }

    public dispose(): void {
        this.statusBarItem.dispose();
    }
}
