import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs';
import { AppSageProfile, ProfileConfiguration, CreateProfileOptions, UpdateProfileOptions } from '../types/profileTypes';

export class ProfileManager {
    private static instance: ProfileManager;
    private configuration: ProfileConfiguration;
    private readonly configPath: string;
    private readonly configFileName = 'appsage-profiles.json';

    private constructor(private context: vscode.ExtensionContext) {
        this.configPath = path.join(context.globalStorageUri.fsPath, this.configFileName);
        this.configuration = this.loadConfiguration();
        this.ensureStorageDirectory();
    }

    public static getInstance(context?: vscode.ExtensionContext): ProfileManager {
        if (!ProfileManager.instance && context) {
            ProfileManager.instance = new ProfileManager(context);
        } else if (!ProfileManager.instance) {
            throw new Error('ProfileManager must be initialized with a context first');
        }
        return ProfileManager.instance;
    }

    private ensureStorageDirectory(): void {
        const storageDir = path.dirname(this.configPath);
        if (!fs.existsSync(storageDir)) {
            fs.mkdirSync(storageDir, { recursive: true });
        }
    }

    private loadConfiguration(): ProfileConfiguration {
        try {
            if (fs.existsSync(this.configPath)) {
                const data = fs.readFileSync(this.configPath, 'utf8');
                const config = JSON.parse(data);
                // Convert date strings back to Date objects
                config.profiles = config.profiles.map((profile: any) => ({
                    ...profile,
                    createdAt: new Date(profile.createdAt),
                    lastModified: new Date(profile.lastModified)
                }));
                return config;
            }
        } catch (error) {
            console.error('Error loading profile configuration:', error);
        }
        
        return { profiles: [] };
    }

    private saveConfiguration(): void {
        try {
            fs.writeFileSync(this.configPath, JSON.stringify(this.configuration, null, 2));
        } catch (error) {
            console.error('Error saving profile configuration:', error);
            vscode.window.showErrorMessage('Failed to save profile configuration');
        }
    }

    public getAllProfiles(): AppSageProfile[] {
        return [...this.configuration.profiles];
    }

    public getActiveProfile(): AppSageProfile | undefined {
        return this.configuration.profiles.find(p => p.isActive);
    }

    public getProfileById(id: string): AppSageProfile | undefined {
        return this.configuration.profiles.find(p => p.id === id);
    }

    public createProfile(options: CreateProfileOptions): AppSageProfile {
        // Validate paths
        if (!this.isValidPath(options.workspacePath)) {
            throw new Error('Invalid workspace path');
        }
        if (!this.isValidPath(options.outputPath)) {
            throw new Error('Invalid output path');
        }

        // Check for duplicate names
        if (this.configuration.profiles.some(p => p.name === options.name)) {
            throw new Error('Profile name already exists');
        }

        const newProfile: AppSageProfile = {
            id: this.generateId(),
            name: options.name,
            workspacePath: options.workspacePath,
            outputPath: options.outputPath,
            isActive: this.configuration.profiles.length === 0, // First profile is active by default
            createdAt: new Date(),
            lastModified: new Date()
        };

        // If this is the first profile or no active profile exists, make it active
        if (newProfile.isActive) {
            this.configuration.profiles.forEach(p => p.isActive = false);
        }

        this.configuration.profiles.push(newProfile);
        this.saveConfiguration();

        return newProfile;
    }

    public updateProfile(options: UpdateProfileOptions): AppSageProfile {
        const profile = this.getProfileById(options.id);
        if (!profile) {
            throw new Error('Profile not found');
        }

        if (options.name !== undefined) {
            // Check for duplicate names (excluding current profile)
            if (this.configuration.profiles.some(p => p.id !== options.id && p.name === options.name)) {
                throw new Error('Profile name already exists');
            }
            profile.name = options.name;
        }

        if (options.workspacePath !== undefined) {
            if (!this.isValidPath(options.workspacePath)) {
                throw new Error('Invalid workspace path');
            }
            profile.workspacePath = options.workspacePath;
        }

        if (options.outputPath !== undefined) {
            if (!this.isValidPath(options.outputPath)) {
                throw new Error('Invalid output path');
            }
            profile.outputPath = options.outputPath;
        }

        profile.lastModified = new Date();
        this.saveConfiguration();

        return profile;
    }

    public deleteProfile(id: string): boolean {
        const index = this.configuration.profiles.findIndex(p => p.id === id);
        if (index === -1) {
            return false;
        }

        const profile = this.configuration.profiles[index];
        this.configuration.profiles.splice(index, 1);

        // If we deleted the active profile, make the first remaining profile active
        if (profile.isActive && this.configuration.profiles.length > 0) {
            this.configuration.profiles[0].isActive = true;
        }

        this.saveConfiguration();
        return true;
    }

    public setActiveProfile(id: string): boolean {
        const profile = this.getProfileById(id);
        if (!profile) {
            return false;
        }

        // Deactivate all profiles
        this.configuration.profiles.forEach(p => p.isActive = false);
        
        // Activate the selected profile
        profile.isActive = true;
        this.saveConfiguration();

        return true;
    }

    private generateId(): string {
        return `profile_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
    }

    private isValidPath(path: string): boolean {
        try {
            // Basic path validation - check if it's a valid absolute path
            return path !== null && path !== undefined && path.trim().length > 0 && (path.includes('/') || path.includes('\\'));
        } catch {
            return false;
        }
    }

    public exportProfiles(): string {
        return JSON.stringify(this.configuration, null, 2);
    }

    public importProfiles(jsonData: string): boolean {
        try {
            const importedConfig: ProfileConfiguration = JSON.parse(jsonData);
            
            // Validate the imported data
            if (!importedConfig.profiles || !Array.isArray(importedConfig.profiles)) {
                throw new Error('Invalid profile configuration format');
            }

            // Convert date strings to Date objects and validate profiles
            importedConfig.profiles = importedConfig.profiles.map((profile: any) => {
                if (!profile.id || !profile.name || !profile.workspacePath || !profile.outputPath) {
                    throw new Error('Invalid profile data');
                }
                return {
                    ...profile,
                    createdAt: new Date(profile.createdAt),
                    lastModified: new Date(profile.lastModified)
                };
            });

            this.configuration = importedConfig;
            this.saveConfiguration();
            return true;
        } catch (error) {
            console.error('Error importing profiles:', error);
            return false;
        }
    }
}
