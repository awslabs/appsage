import * as vscode from 'vscode';
import { GraphViewer } from './handlers/graph/graphViewer';
import { TableViewer } from './handlers/table/tableViewer';
import { GraphPropertyPanel } from './handlers/graph/components/graphPropertyPanel';
import { AppSageLogger } from './shared/logging';
import { ProfileManager } from './shared/providers/profileManager';
import { ProfileTreeDataProvider } from './shared/providers/profileTreeDataProvider';
import { ProfileUIManager } from './shared/providers/profileUIManager';
import { ProfileStatusBarManager } from './shared/providers/profileStatusBarManager';
import { AppSageConfigurationAPI } from './shared/providers/appSageConfigurationAPI';

export function activate(context: vscode.ExtensionContext) {
    // Initialize centralized logger
    const logger = AppSageLogger.initialize(context);
    const extensionLogger = logger.forComponent('Extension');
    
    extensionLogger.info('AppSage Extension activating...');

    // Initialize Profile Management
    const profileManager = ProfileManager.getInstance(context);
    const profileTreeDataProvider = new ProfileTreeDataProvider(profileManager);
    const profileUIManager = new ProfileUIManager(profileManager);
    const profileStatusBarManager = new ProfileStatusBarManager(profileManager);
    const configurationAPI = AppSageConfigurationAPI.getInstance(profileManager);
    
    context.subscriptions.push(profileStatusBarManager);
    extensionLogger.info('Profile management system initialized');
    
    // Register Profile Tree View
    const profileTreeView = vscode.window.createTreeView('appsage.profileExplorer', {
        treeDataProvider: profileTreeDataProvider,
        showCollapseAll: true,
        canSelectMany: false
    });
    context.subscriptions.push(profileTreeView);
    extensionLogger.info('Profile tree view registered');

    // Set up the property panel
    const propertyPanel = GraphPropertyPanel.getInstance();
    propertyPanel.setLogger(logger);
    extensionLogger.info('Graph property panel initialized');

    // Register Graph Viewer
    const graphProvider = new GraphViewer(context);
    context.subscriptions.push(
        vscode.window.registerCustomEditorProvider(
            'appsage.graphViewer',
            graphProvider,
            {
                webviewOptions: {
                    retainContextWhenHidden: true,
                },
                supportsMultipleEditorsPerDocument: false,
            }
        )
    );
    extensionLogger.info('Graph viewer registered');

    // Register Table Viewer
    const tableProvider = new TableViewer(context);
    context.subscriptions.push(
        vscode.window.registerCustomEditorProvider(
            'appsage.tableViewer',
            tableProvider,
            {
                webviewOptions: {
                    retainContextWhenHidden: true,
                },
                supportsMultipleEditorsPerDocument: false,
            }
        )
    );
    extensionLogger.info('Table viewer registered');

    // Register commands
    context.subscriptions.push(
        vscode.commands.registerCommand('appsage.showGraphProperties', () => {
            extensionLogger.info('Show Graph Properties command executed - no longer needed with integrated panel');
            vscode.window.showInformationMessage('Properties are now integrated into the graph view. Use the View dropdown in the graph viewer.');
        })
    );

    // Register Profile Management Commands
    context.subscriptions.push(
        vscode.commands.registerCommand('appsage.createProfile', async () => {
            extensionLogger.info('Create Profile command executed');
            const newProfile = await profileUIManager.showCreateProfileDialog();
            if (newProfile) {
                profileTreeDataProvider.refresh();
                profileStatusBarManager.updateStatusBar();
                extensionLogger.info(`Profile created: ${newProfile.name}`);
            }
        })
    );

    context.subscriptions.push(
        vscode.commands.registerCommand('appsage.deleteProfile', async (item) => {
            extensionLogger.info('Delete Profile command executed');
            if (item && item.profile) {
                const success = await profileUIManager.showDeleteProfileDialog(item.profile.id);
                if (success) {
                    profileTreeDataProvider.refresh();
                    profileStatusBarManager.updateStatusBar();
                    extensionLogger.info(`Profile deleted: ${item.profile.name}`);
                }
            }
        })
    );

    context.subscriptions.push(
        vscode.commands.registerCommand('appsage.editProfile', async (item) => {
            extensionLogger.info('Edit Profile command executed');
            if (item && item.profile) {
                const updatedProfile = await profileUIManager.showEditProfileDialog(item.profile.id);
                if (updatedProfile) {
                    profileTreeDataProvider.refresh();
                    profileStatusBarManager.updateStatusBar();
                    extensionLogger.info(`Profile updated: ${updatedProfile.name}`);
                }
            }
        })
    );

    context.subscriptions.push(
        vscode.commands.registerCommand('appsage.setActiveProfile', async (item) => {
            extensionLogger.info('Set Active Profile command executed');
            if (item && item.profile) {
                const success = await profileUIManager.showSetActiveProfileDialog(item.profile.id);
                if (success) {
                    profileTreeDataProvider.refresh();
                    profileStatusBarManager.updateStatusBar();
                    extensionLogger.info(`Active profile set: ${item.profile.name}`);
                }
            }
        })
    );

    context.subscriptions.push(
        vscode.commands.registerCommand('appsage.refreshProfiles', () => {
            extensionLogger.info('Refresh Profiles command executed');
            profileTreeDataProvider.refresh();
        })
    );

    context.subscriptions.push(
        vscode.commands.registerCommand('appsage.selectProfileFromStatusBar', async () => {
            extensionLogger.info('Select Profile from Status Bar command executed');
            const selectedProfile = await profileUIManager.showProfileQuickPick();
            if (selectedProfile && !selectedProfile.isActive) {
                const success = await profileUIManager.showSetActiveProfileDialog(selectedProfile.id);
                if (success) {
                    profileTreeDataProvider.refresh();
                    profileStatusBarManager.updateStatusBar();
                    extensionLogger.info(`Active profile changed to: ${selectedProfile.name}`);
                }
            }
        })
    );

    extensionLogger.info('Commands registered');

    extensionLogger.info('AppSage Extension activated successfully');
}

export function deactivate() {}

/**
 * Get the AppSage Configuration API instance
 * This allows other parts of the extension to access profile settings
 */
export function getAppSageConfiguration(): AppSageConfigurationAPI {
    return AppSageConfigurationAPI.getInstance();
}
