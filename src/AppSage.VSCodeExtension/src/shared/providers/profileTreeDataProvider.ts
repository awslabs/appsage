import * as vscode from 'vscode';
import { AppSageProfile } from '../types/profileTypes';
import { ProfileManager } from './profileManager';

export class ProfileTreeItem extends vscode.TreeItem {
    constructor(
        public readonly profile: AppSageProfile,
        public readonly collapsibleState: vscode.TreeItemCollapsibleState
    ) {
        super(profile.name, collapsibleState);
        
        this.tooltip = `${profile.name}\nWorkspace: ${profile.workspacePath}\nOutput: ${profile.outputPath}`;
        this.description = profile.isActive ? '(Active)' : '';
        this.contextValue = 'profile';
        
        // Set icon based on active status
        this.iconPath = new vscode.ThemeIcon(
            profile.isActive ? 'circle-filled' : 'circle-outline'
        );

        // Add children for workspace and output paths
        if (collapsibleState === vscode.TreeItemCollapsibleState.Expanded || 
            collapsibleState === vscode.TreeItemCollapsibleState.Collapsed) {
            this.resourceUri = vscode.Uri.parse(`appsage-profile:${profile.id}`);
        }
    }
}

export class ProfilePathItem extends vscode.TreeItem {
    constructor(
        public readonly label: string,
        public readonly path: string,
        public readonly pathType: 'workspace' | 'output'
    ) {
        super(label, vscode.TreeItemCollapsibleState.None);
        
        this.tooltip = path;
        this.description = path;
        this.contextValue = 'profilePath';
        
        this.iconPath = new vscode.ThemeIcon(
            pathType === 'workspace' ? 'folder' : 'output'
        );

        // Make the path clickable to open in explorer
        this.command = {
            command: 'vscode.openFolder',
            title: 'Open Folder',
            arguments: [vscode.Uri.file(path), { forceNewWindow: false }]
        };
    }
}

export class ProfileTreeDataProvider implements vscode.TreeDataProvider<ProfileTreeItem | ProfilePathItem> {
    private _onDidChangeTreeData: vscode.EventEmitter<ProfileTreeItem | ProfilePathItem | undefined | null | void> = new vscode.EventEmitter<ProfileTreeItem | ProfilePathItem | undefined | null | void>();
    readonly onDidChangeTreeData: vscode.Event<ProfileTreeItem | ProfilePathItem | undefined | null | void> = this._onDidChangeTreeData.event;

    constructor(private profileManager: ProfileManager) {}

    refresh(): void {
        this._onDidChangeTreeData.fire();
    }

    getTreeItem(element: ProfileTreeItem | ProfilePathItem): vscode.TreeItem {
        return element;
    }

    getChildren(element?: ProfileTreeItem | ProfilePathItem): Thenable<(ProfileTreeItem | ProfilePathItem)[]> {
        if (!element) {
            // Root level - return all profiles
            const profiles = this.profileManager.getAllProfiles();
            return Promise.resolve(
                profiles.map(profile => 
                    new ProfileTreeItem(profile, vscode.TreeItemCollapsibleState.Collapsed)
                )
            );
        } else if (element instanceof ProfileTreeItem) {
            // Profile level - return workspace and output paths
            const profile = element.profile;
            return Promise.resolve([
                new ProfilePathItem('Workspace', profile.workspacePath, 'workspace'),
                new ProfilePathItem('Output', profile.outputPath, 'output')
            ]);
        }
        
        return Promise.resolve([]);
    }

    getParent(element: ProfileTreeItem | ProfilePathItem): vscode.ProviderResult<ProfileTreeItem | ProfilePathItem> {
        if (element instanceof ProfilePathItem) {
            // Find the parent profile
            const profiles = this.profileManager.getAllProfiles();
            const parentProfile = profiles.find(p => 
                p.workspacePath === element.path || p.outputPath === element.path
            );
            if (parentProfile) {
                return new ProfileTreeItem(parentProfile, vscode.TreeItemCollapsibleState.Collapsed);
            }
        }
        return undefined;
    }
}
