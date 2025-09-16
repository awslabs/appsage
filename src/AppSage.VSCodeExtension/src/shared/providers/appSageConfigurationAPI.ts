import { ProfileManager } from './profileManager';
import { AppSageProfile } from '../types/profileTypes';

/**
 * AppSage Configuration API
 * Provides easy access to current profile settings for other parts of the extension
 */
export class AppSageConfigurationAPI {
    private static instance: AppSageConfigurationAPI;
    private profileManager: ProfileManager;

    private constructor(profileManager: ProfileManager) {
        this.profileManager = profileManager;
    }

    public static getInstance(profileManager?: ProfileManager): AppSageConfigurationAPI {
        if (!AppSageConfigurationAPI.instance && profileManager) {
            AppSageConfigurationAPI.instance = new AppSageConfigurationAPI(profileManager);
        } else if (!AppSageConfigurationAPI.instance) {
            throw new Error('AppSageConfigurationAPI must be initialized with ProfileManager first');
        }
        return AppSageConfigurationAPI.instance;
    }

    /**
     * Get the currently active profile
     */
    public getActiveProfile(): AppSageProfile | undefined {
        return this.profileManager.getActiveProfile();
    }

    /**
     * Get the workspace path from the active profile
     */
    public getWorkspacePath(): string | undefined {
        const activeProfile = this.getActiveProfile();
        return activeProfile?.workspacePath;
    }

    /**
     * Get the output path from the active profile
     */
    public getOutputPath(): string | undefined {
        const activeProfile = this.getActiveProfile();
        return activeProfile?.outputPath;
    }

    /**
     * Check if there is an active profile configured
     */
    public hasActiveProfile(): boolean {
        return this.getActiveProfile() !== undefined;
    }

    /**
     * Get all available profiles
     */
    public getAllProfiles(): AppSageProfile[] {
        return this.profileManager.getAllProfiles();
    }

    /**
     * Get the active profile name for display purposes
     */
    public getActiveProfileName(): string {
        const activeProfile = this.getActiveProfile();
        return activeProfile?.name || 'No Profile';
    }

    /**
     * Validate if the current configuration is ready for AppSage operations
     */
    public validateConfiguration(): { isValid: boolean; errors: string[] } {
        const errors: string[] = [];
        const activeProfile = this.getActiveProfile();

        if (!activeProfile) {
            errors.push('No active profile selected');
            return { isValid: false, errors };
        }

        if (!activeProfile.workspacePath || activeProfile.workspacePath.trim().length === 0) {
            errors.push('Workspace path is not configured');
        }

        if (!activeProfile.outputPath || activeProfile.outputPath.trim().length === 0) {
            errors.push('Output path is not configured');
        }

        return {
            isValid: errors.length === 0,
            errors
        };
    }

    /**
     * Get configuration as object for easy consumption
     */
    public getConfiguration(): { workspacePath?: string; outputPath?: string; profileName?: string } {
        const activeProfile = this.getActiveProfile();
        
        if (!activeProfile) {
            return {};
        }

        return {
            workspacePath: activeProfile.workspacePath,
            outputPath: activeProfile.outputPath,
            profileName: activeProfile.name
        };
    }
}
