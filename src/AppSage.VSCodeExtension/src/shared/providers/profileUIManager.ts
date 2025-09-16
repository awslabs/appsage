import * as vscode from 'vscode';
import * as path from 'path';
import { ProfileManager } from './profileManager';
import { CreateProfileOptions, UpdateProfileOptions, AppSageProfile } from '../types/profileTypes';

export class ProfileUIManager {
    constructor(private profileManager: ProfileManager) {}

    async showCreateProfileDialog(): Promise<AppSageProfile | undefined> {
        try {
            // Get profile name
            const name = await vscode.window.showInputBox({
                prompt: 'Enter profile name',
                placeHolder: 'e.g., My Project',
                validateInput: (value) => {
                    if (!value || value.trim().length === 0) {
                        return 'Profile name cannot be empty';
                    }
                    if (this.profileManager.getAllProfiles().some(p => p.name === value.trim())) {
                        return 'Profile name already exists';
                    }
                    return undefined;
                }
            });

            if (!name) {
                return undefined;
            }

            // Get workspace path
            const workspaceFolders = await vscode.window.showOpenDialog({
                canSelectFiles: false,
                canSelectFolders: true,
                canSelectMany: false,
                title: 'Select AppSage Workspace Folder',
                openLabel: 'Select Workspace'
            });

            if (!workspaceFolders || workspaceFolders.length === 0) {
                return undefined;
            }

            const workspacePath = workspaceFolders[0].fsPath;

            // Get output path
            const outputFolders = await vscode.window.showOpenDialog({
                canSelectFiles: false,
                canSelectFolders: true,
                canSelectMany: false,
                title: 'Select AppSage Output Folder',
                openLabel: 'Select Output Folder'
            });

            if (!outputFolders || outputFolders.length === 0) {
                return undefined;
            }

            const outputPath = outputFolders[0].fsPath;

            // Create the profile
            const options: CreateProfileOptions = {
                name: name.trim(),
                workspacePath,
                outputPath
            };

            const profile = this.profileManager.createProfile(options);
            
            vscode.window.showInformationMessage(`Profile "${profile.name}" created successfully!`);
            return profile;

        } catch (error) {
            vscode.window.showErrorMessage(`Failed to create profile: ${error instanceof Error ? error.message : 'Unknown error'}`);
            return undefined;
        }
    }

    async showEditProfileDialog(profileId: string): Promise<AppSageProfile | undefined> {
        try {
            const profile = this.profileManager.getProfileById(profileId);
            if (!profile) {
                vscode.window.showErrorMessage('Profile not found');
                return undefined;
            }

            // Show options for what to edit
            const editOption = await vscode.window.showQuickPick([
                { label: 'Edit Name', value: 'name' },
                { label: 'Edit Workspace Path', value: 'workspace' },
                { label: 'Edit Output Path', value: 'output' },
                { label: 'Edit All', value: 'all' }
            ], {
                title: `Edit Profile: ${profile.name}`
            });

            if (!editOption) {
                return undefined;
            }

            const updateOptions: UpdateProfileOptions = { id: profileId };

            // Handle name editing
            if (editOption.value === 'name' || editOption.value === 'all') {
                const newName = await vscode.window.showInputBox({
                    prompt: 'Enter new profile name',
                    value: profile.name,
                    validateInput: (value) => {
                        if (!value || value.trim().length === 0) {
                            return 'Profile name cannot be empty';
                        }
                        if (value.trim() !== profile.name && 
                            this.profileManager.getAllProfiles().some(p => p.name === value.trim())) {
                            return 'Profile name already exists';
                        }
                        return undefined;
                    }
                });

                if (newName === undefined) {
                    return undefined;
                }

                updateOptions.name = newName.trim();
            }

            // Handle workspace path editing
            if (editOption.value === 'workspace' || editOption.value === 'all') {
                const workspaceFolders = await vscode.window.showOpenDialog({
                    canSelectFiles: false,
                    canSelectFolders: true,
                    canSelectMany: false,
                    title: 'Select New AppSage Workspace Folder',
                    openLabel: 'Select Workspace',
                    defaultUri: vscode.Uri.file(profile.workspacePath)
                });

                if (!workspaceFolders || workspaceFolders.length === 0) {
                    if (editOption.value === 'workspace') {
                        return undefined; // User cancelled workspace-only edit
                    }
                } else {
                    updateOptions.workspacePath = workspaceFolders[0].fsPath;
                }
            }

            // Handle output path editing
            if (editOption.value === 'output' || editOption.value === 'all') {
                const outputFolders = await vscode.window.showOpenDialog({
                    canSelectFiles: false,
                    canSelectFolders: true,
                    canSelectMany: false,
                    title: 'Select New AppSage Output Folder',
                    openLabel: 'Select Output Folder',
                    defaultUri: vscode.Uri.file(profile.outputPath)
                });

                if (!outputFolders || outputFolders.length === 0) {
                    if (editOption.value === 'output') {
                        return undefined; // User cancelled output-only edit
                    }
                } else {
                    updateOptions.outputPath = outputFolders[0].fsPath;
                }
            }

            // Update the profile
            const updatedProfile = this.profileManager.updateProfile(updateOptions);
            
            vscode.window.showInformationMessage(`Profile "${updatedProfile.name}" updated successfully!`);
            return updatedProfile;

        } catch (error) {
            vscode.window.showErrorMessage(`Failed to update profile: ${error instanceof Error ? error.message : 'Unknown error'}`);
            return undefined;
        }
    }

    async showDeleteProfileDialog(profileId: string): Promise<boolean> {
        try {
            const profile = this.profileManager.getProfileById(profileId);
            if (!profile) {
                vscode.window.showErrorMessage('Profile not found');
                return false;
            }

            const confirmText = profile.isActive ? 
                `Are you sure you want to delete the active profile "${profile.name}"? This action cannot be undone.` :
                `Are you sure you want to delete profile "${profile.name}"? This action cannot be undone.`;

            const choice = await vscode.window.showWarningMessage(
                confirmText,
                { modal: true },
                'Delete',
                'Cancel'
            );

            if (choice === 'Delete') {
                const success = this.profileManager.deleteProfile(profileId);
                if (success) {
                    vscode.window.showInformationMessage(`Profile "${profile.name}" deleted successfully!`);
                    return true;
                } else {
                    vscode.window.showErrorMessage('Failed to delete profile');
                    return false;
                }
            }

            return false;

        } catch (error) {
            vscode.window.showErrorMessage(`Failed to delete profile: ${error instanceof Error ? error.message : 'Unknown error'}`);
            return false;
        }
    }

    async showSetActiveProfileDialog(profileId: string): Promise<boolean> {
        try {
            const profile = this.profileManager.getProfileById(profileId);
            if (!profile) {
                vscode.window.showErrorMessage('Profile not found');
                return false;
            }

            if (profile.isActive) {
                vscode.window.showInformationMessage(`Profile "${profile.name}" is already active`);
                return true;
            }

            const success = this.profileManager.setActiveProfile(profileId);
            if (success) {
                vscode.window.showInformationMessage(`Profile "${profile.name}" is now active`);
                return true;
            } else {
                vscode.window.showErrorMessage('Failed to set active profile');
                return false;
            }

        } catch (error) {
            vscode.window.showErrorMessage(`Failed to set active profile: ${error instanceof Error ? error.message : 'Unknown error'}`);
            return false;
        }
    }

    async showProfileQuickPick(): Promise<AppSageProfile | undefined> {
        const profiles = this.profileManager.getAllProfiles();
        
        if (profiles.length === 0) {
            vscode.window.showInformationMessage('No profiles found. Create a profile first.');
            return undefined;
        }

        const items = profiles.map(profile => ({
            label: profile.name,
            description: profile.isActive ? '(Active)' : '',
            detail: `Workspace: ${profile.workspacePath} | Output: ${profile.outputPath}`,
            profile
        }));

        const selected = await vscode.window.showQuickPick(items, {
            title: 'Select AppSage Profile',
            placeHolder: 'Choose a profile to work with'
        });

        return selected?.profile;
    }

    async showExportImportMenu(): Promise<void> {
        const choice = await vscode.window.showQuickPick([
            { label: 'Export Profiles', value: 'export' },
            { label: 'Import Profiles', value: 'import' }
        ], {
            title: 'Profile Export/Import'
        });

        if (choice?.value === 'export') {
            await this.exportProfiles();
        } else if (choice?.value === 'import') {
            await this.importProfiles();
        }
    }

    private async exportProfiles(): Promise<void> {
        try {
            const exportData = this.profileManager.exportProfiles();
            
            const saveLocation = await vscode.window.showSaveDialog({
                title: 'Export AppSage Profiles',
                defaultUri: vscode.Uri.file('appsage-profiles.json'),
                filters: {
                    'JSON Files': ['json'],
                    'All Files': ['*']
                }
            });

            if (saveLocation) {
                await vscode.workspace.fs.writeFile(saveLocation, Buffer.from(exportData, 'utf8'));
                vscode.window.showInformationMessage(`Profiles exported to ${saveLocation.fsPath}`);
            }
        } catch (error) {
            vscode.window.showErrorMessage(`Failed to export profiles: ${error instanceof Error ? error.message : 'Unknown error'}`);
        }
    }

    private async importProfiles(): Promise<void> {
        try {
            const openLocation = await vscode.window.showOpenDialog({
                title: 'Import AppSage Profiles',
                canSelectFiles: true,
                canSelectFolders: false,
                canSelectMany: false,
                filters: {
                    'JSON Files': ['json'],
                    'All Files': ['*']
                }
            });

            if (openLocation && openLocation.length > 0) {
                const fileContent = await vscode.workspace.fs.readFile(openLocation[0]);
                const jsonData = Buffer.from(fileContent).toString('utf8');
                
                const success = this.profileManager.importProfiles(jsonData);
                if (success) {
                    vscode.window.showInformationMessage('Profiles imported successfully!');
                } else {
                    vscode.window.showErrorMessage('Failed to import profiles. Please check the file format.');
                }
            }
        } catch (error) {
            vscode.window.showErrorMessage(`Failed to import profiles: ${error instanceof Error ? error.message : 'Unknown error'}`);
        }
    }
}
