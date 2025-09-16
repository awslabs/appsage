export interface AppSageProfile {
    id: string;
    name: string;
    workspacePath: string;
    outputPath: string;
    isActive: boolean;
    createdAt: Date;
    lastModified: Date;
}

export interface ProfileConfiguration {
    profiles: AppSageProfile[];
    activeProfileId?: string;
}

export interface CreateProfileOptions {
    name: string;
    workspacePath: string;
    outputPath: string;
}

export interface UpdateProfileOptions {
    id: string;
    name?: string;
    workspacePath?: string;
    outputPath?: string;
}
