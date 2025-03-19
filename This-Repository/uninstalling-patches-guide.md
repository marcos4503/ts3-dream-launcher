# Patch Uninstall Guide

This guide is written to help you uninstall most Patches that can be installed via Dream Launcher, if you are willing to take the risk of having your game affected after uninstalling a Patch. Generally, most issues resulting from uninstalling Patches occur in Save Games created while the Patch was installed, or when the Patch was a bug fix or performance issue, resulting in the problem returning after the Patch is removed.

Although Patches installed by Dream Launcher can be extremely useful, sometimes for different types of needs, you may want to uninstall them. To avoid data inconsistency issues or game crashes, Dream Launcher does not allow uninstallation of Patches once they are installed, so there is no option to uninstall Patches through the Launcher.

Below you will find a way to uninstall each Patch present in the Dream Launcher, separated by topic.

> [!TIP]
> Remember, if you like a Patch, but find it to be problematic, you can always RE-INSTALL that Patch, instead of uninstalling it. RE-INSTALLING is perfectly safe and can always be done through Dream Launcher.

> [!CAUTION]
> Keep in mind that when following the instructions below, always make sure that The Sims 3 and the Dream Launcher are closed. It is your responsibility to follow the steps below correctly, so that uninstalling the Patch does not break your game. After uninstalling a Patch, it is also your responsibility to check the Save Games that were created while the uninstalled Patch was still installed, and check if the Save Game still works as expected.

## Alder Laker+ Support

Uninstallation steps...

1. Go to the Root Folder of your The Sims 3 installation.
2. Go to folder `Game` and then `Bin`.
3. Delete files `TS3W.exe` and `IntelFix.dll`.
4. Copy file `TS3W-backup.exe` and paste it in the same directory, after that, rename it to `TS3W.exe`.
5. Now go to the `Content` folder.
6. In file `prefs.json`, change the value of variable `alreadyIntelFixed` to `false`.
7. Save the changes to the file and open Dream Launcher to check if the Patch appears as "Not Installed".

## PT-PT to PT-BR Better Support

Uninstallation steps...

1. Go to the Root Folder of your The Sims 3 installation.
2. Delete file `47890_install.vdf` (if exists).
3. If exists, copy file `47890_install-backup.vdf` and paste it in the same directory, after that, rename it to `47890_install.vdf`.
4. Go to folder `Game`, then `Bin` and then `Content`.
5. In file `prefs.json`, change the value of variable `alreadyTranslated` to `false`.
6. Save the changes to the file and open Dream Launcher to check if the Patch appears as "Not Installed".

## Mods Support

Uninstallation steps...

1. Go to the "Documents" folder on your computer.
2. Go to folder `Electronic Arts` and then `The Sims 3`.
3. Delete folder `Mods`.
4. Go to the Root Folder of your The Sims 3 installation.
5. Go to folder `Game`, then `Bin` and then `Content`.
6. In file `prefs.json`, change the value of variable `patchModsSupport` to `false`.
7. Save the changes to the file and open Dream Launcher to check if the Patch appears as "Not Installed".

## Basic Optimization Patch

Uninstallation steps...

1. Go to the "Documents" folder on your computer.
2. Go to folder `Electronic Arts`, then `The Sims 3`, then `Mods`, then `Packages` and then `DL3-Patches`.
3. Delete the files of this list...
    - ErrorTrap.package
    - HideExpansionPacksGameIcons.package
    - MasterController.package
    - MasterController_Cheats.package
    - MasterController_Integration.package
    - MemoriesDisabled.package
    - Overwatch.package
    - Overwatch_Tuning.package
    - Register.package
    - Register_Tuning.package
    - SmoothPatch.package
    - SmoothPatch_MasterController.package
    - Traffic.package
    - Traffic_Tuning.package
    - Traveler.package
    - Traveler_Tuning.package
    - GoHere.package
    - GoHere_Tuning.package
4. Go to the Root Folder of your The Sims 3 installation.
5. Go to folder `Game` and then `Bin`.
6. Delete files `TS3Patch.asi` and `TS3Patch.txt`.
7. Go to folder `Content`.
8. In file `prefs.json`, change the value of variable `patchBasicOptimization` to `false`.
9. Save the changes to the file and open Dream Launcher to check if the Patch appears as "Not Installed".

## FPS Limiter Patch

Uninstallation steps...

1. Go to the Root Folder of your The Sims 3 installation.
2. Go to folder `Game` and then `Bin`.
3. Delete the file `antilag.cfg`.
4. Go to folder `Content`.
5. In file `prefs.json`, change the value of variable `patchFpsLimiter` to `false`.
6. Save the changes to the file and open Dream Launcher to check if the Patch appears as "Not Installed".

## GPU and CPU Update Patch

Uninstallation steps...

1. Go to the Root Folder of your The Sims 3 installation.
2. Go to folder `Game` and then `Bin`.
3. Delete files `GraphicsCards.sgr` and `GraphicsRules.sgr`.
4. Copy file `GraphicsCards-backup.sgr` and paste it in the same directory, after that, rename it to `GraphicsCards.sgr`.
5. Copy file `GraphicsRules-backup.sgr` and paste it in the same directory, after that, rename it to `GraphicsRules.sgr`.
6. Go to folder `Content`.
7. In file `prefs.json`, change the value of variable `patchCpuGpuUpdate` to `false`.
8. Save the changes to the file and open Dream Launcher to check if the Patch appears as "Not Installed".

## Better Global Ilumination Patch

Uninstallation steps...

1. Go to the "Documents" folder on your computer.
2. Go to folder `Electronic Arts`, then `The Sims 3`, then `Mods`, then `Packages` and then `DL3-Patches`.
3. Delete the files of this list...
    - AutoLightsOverhaul.package
    - AutoLightsOverhaulCommon.package
    - BoringBonesFixedLightning.package
    - ImprovedEnvironmentalShadows.package
4. Go to the Root Folder of your The Sims 3 installation.
5. Go to folder `Game`, then `Bin` and then `Content`.
6. In file `prefs.json`, change the value of variable `patchBetterGlobalIllumination` to `false`.
7. Save the changes to the file and open Dream Launcher to check if the Patch appears as "Not Installed".

## Improved Shading Patch

Uninstallation steps...

1. Go to the Root Folder of your The Sims 3 installation.
2. Go to folder `Game` and then `Bin`.
3. Delete files `D3DShaderReplacer.asi` and `D3DShaderReplacer.cfg`.
4. Delete folders `shader_replace` and `shader_textures`.
5. Go to folder `Content`.
6. In file `prefs.json`, change the value of variable `patchImprovedShaders` to `false`.
7. Save the changes to the file and open Dream Launcher to check if the Patch appears as "Not Installed".

## Shadow Extender Patch

Uninstallation steps...

1. Go to the Root Folder of your The Sims 3 installation.
2. Go to folder `Game` and then `Bin`.
3. Delete files `ShadowExtender.cfg` and `TS3ShadowExtender.asi`.
4. Go to folder `Content`.
5. In file `prefs.json`, change the value of variable `patchShadowExtender` to `false`.
6. Save the changes to the file and open Dream Launcher to check if the Patch appears as "Not Installed".

## Routing Optimizations Patch

Uninstallation steps...

1. Go to the "Documents" folder on your computer.
2. Go to folder `Electronic Arts`, then `The Sims 3`, then `Mods`, then `Packages` and then `DL3-Patches`.
3. Delete the files of this list...
   - FasterElevatorMoving.package or FasterElevatorMoving.package.disabled
   - BetterRoutingForGameObjects.package
   - NoFootTapping.package
   - NoWhiningMotives.package
   - RouteFixF4V9.package
   - NoRouteFailAnimation.package
4. Go to the Root Folder of your The Sims 3 installation.
5. Go to folder `Game`, then `Bin` and then `Content`.
6. In file `prefs.json`, change the value of variable `patchRoutingOptimizations` to `false`.
7. Save the changes to the file and open Dream Launcher to check if the Patch appears as "Not Installed".

## Internet Removal Patch

Uninstallation steps...

1. Press the "WIN + R" shortcut.
2. Run the command "wf.msc".
3. In "Inbound Rules", search and delete the rule with the name of "The Sims 3 - DL3 Block Patch" (if exists).
4. In "Outbound Rules", search and delete the rule with the name of "The Sims 3 - DL3 Block Patch" (if exists).
5. Go to the Root Folder of your The Sims 3 installation.
6. Go to folder `Game`, then `Bin` and then `Content`.
7. In file `prefs.json`, change the value of variable `patchInternetRemoval` to `false`.
8. Save the changes to the file and open Dream Launcher to check if the Patch appears as "Not Installed".

## Better Story Progression Patch

Uninstallation steps...

1. Go to the "Documents" folder on your computer.
2. Go to folder `Electronic Arts`, then `The Sims 3`, then `Mods`, then `Packages` and then `DL3-Patches`.
3. Delete the files of this list...
   - SecondImage.package
   - StoryProgression.package
   - StoryProgression_Tuning.package
   - StoryProgression_Meanies.package
   - StoryProgression_Lovers.package
   - StoryProgression_Career.package
   - StoryProgression_FairiesAndWerewolves.package or StoryProgression_FairiesAndWerewolves.package.disabled
   - StoryProgression_Extra.package
   - StoryProgression_Money.package
   - StoryProgression_Population.package
   - StoryProgression_Relationship.package
   - StoryProgression_Skill.package
   - StoryProgression_CopsAndRobbers.package
   - StoryProgression_VampiresAndSlayers.package or StoryProgression_VampiresAndSlayers.package.disabled
4. Go to the Root Folder of your The Sims 3 installation.
5. Go to folder `Game`, then `Bin` and then `Content`.
6. In file `prefs.json`, change the value of variable `patchStoryProgression` to `false`.
7. Save the changes to the file and open Dream Launcher to check if the Patch appears as "Not Installed".

## Tuning Patch for Recommended Mods

Uninstallation steps...

1. Go to the "Documents" folder on your computer.
2. Go to folder `Electronic Arts`, then `The Sims 3` and then `Library`.
3. Delete the files of this list...
   - passion_recommended_tunning.package
   - tagger_recommended_tunning.package
4. Go to the Root Folder of your The Sims 3 installation.
5. Go to folder `Game`, then `Bin` and then `Content`.
6. In file `prefs.json`, change the value of variable `patchRecommendedMods` to `false`.
7. Save the changes to the file and open Dream Launcher to check if the Patch appears as "Not Installed".

# Doubts?

If you have any questions or want to report a problem, feel free to create a topic in the <a href="https://github.com/marcos4503/ts3-dream-launcher/issues">Issues</a> tab.