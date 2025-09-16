# AppSage Profile Management

The AppSage VS Code Extension now includes a comprehensive profile management system that allows you to organize and switch between different AppSage configurations.

## Features

### Activity Bar Integration
- **AppSage Activity Bar**: A dedicated activity bar on the left side with an AppSage icon
- **Profile Explorer**: Shows all your profiles in a tree view
- **Quick Actions**: Create, edit, delete, and switch profiles with a few clicks

### Profile Configuration
Each profile contains two essential settings:
1. **Workspace Path**: The folder that contains your AppSage workspace files
2. **Output Path**: The folder where AppSage generates its output files

### Status Bar Integration
- Shows the currently active profile in the status bar
- Click the status bar item to quickly switch between profiles
- Visual indicators show when no profile is active

## How to Use

### Creating Your First Profile

1. **Open the AppSage Activity Bar**: Click the AppSage icon in the left activity bar
2. **Create a Profile**: Click the "+" button in the Profile Explorer or use `Ctrl+Shift+P` and run "AppSage: Create Profile"
3. **Enter Profile Details**:
   - Provide a name for your profile (e.g., "My Project")
   - Select your AppSage workspace folder
   - Select your AppSage output folder
4. **Automatic Activation**: Your first profile becomes active automatically

### Managing Profiles

#### Switching Profiles
- **From Tree View**: Right-click any profile and select "Set Active Profile"
- **From Status Bar**: Click the profile name in the status bar and select from the list
- **From Command Palette**: Use "AppSage: Select Profile"

#### Editing Profiles
- Right-click a profile in the tree view and select "Edit Profile"
- Choose what to edit: name, workspace path, output path, or all
- Changes are saved automatically

#### Deleting Profiles
- Right-click a profile and select "Delete Profile"
- Confirm the deletion (this action cannot be undone)
- If you delete the active profile, the first remaining profile becomes active

### Visual Indicators

- **Active Profile**: Shows a filled circle icon (●) and "(Active)" label
- **Inactive Profiles**: Show an outline circle icon (○)
- **Status Bar**: Displays "AppSage: [Profile Name]" for active profile or "AppSage: No Profile" when none is active

### Profile Structure

Profiles are stored in your VS Code global storage and contain:
- Unique ID
- Profile name
- Workspace path
- Output path
- Active status
- Creation and modification timestamps

### Commands Available

| Command | Description |
|---------|-------------|
| `appsage.createProfile` | Create a new profile |
| `appsage.deleteProfile` | Delete the selected profile |
| `appsage.editProfile` | Edit the selected profile |
| `appsage.setActiveProfile` | Set the selected profile as active |
| `appsage.refreshProfiles` | Refresh the profile tree view |
| `appsage.selectProfileFromStatusBar` | Quick profile selection from status bar |

### For Extension Developers

Other parts of the AppSage extension can access the current profile configuration using:

```typescript
import { getAppSageConfiguration } from './extension';

const config = getAppSageConfiguration();

// Get current settings
const workspacePath = config.getWorkspacePath();
const outputPath = config.getOutputPath();
const profileName = config.getActiveProfileName();

// Check if configuration is ready
const validation = config.validateConfiguration();
if (validation.isValid) {
    // Use the configuration
} else {
    // Handle missing configuration
    console.log('Configuration issues:', validation.errors);
}
```

## Future Enhancements

The profile system is designed to be extensible. Future versions may include:
- Additional configuration settings per profile
- Profile import/export functionality
- Integration with AppSage CLI tools
- Workspace-specific profile recommendations
- Profile templates for common setups

## Troubleshooting

**Profile not showing in tree view**: Click the refresh button in the Profile Explorer

**Can't create profile**: Ensure you have selected valid folder paths for both workspace and output

**Status bar not updating**: The status bar should update automatically, but you can manually refresh the profiles if needed

**Lost profiles**: Profiles are stored in VS Code's global storage. If you're missing profiles, check that VS Code has proper permissions to write to its storage directory.
