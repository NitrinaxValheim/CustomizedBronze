# Changelog

* Version 0.0.20

    + updated for Valheim 0.217.25

    + removed function GetGameVersionExtended in VersionOperations because the constants m_major, m_minor, m_patch have been removed from assembly_valheim
    + changed GetCompatiblePlayerVersions to use m_oldestForwardCompatiblePlayerVersion instead of m_compatiblePlayerVersions because the constant has been removed from assembly_valheim
    + changed GetCompatibleWorldVersion to use m_oldestForwardCompatibleWorldVersion instead of m_compatibleWorldVersions because the constant has been removed from assembly_valheim
    + renamed category Recipe Bronze changed to Alloy Type    
    + adjusted the names of the configuration variables
      - renamed Recipe Custom Copper to Required Copper
      - renamed Recipe Custom Tin to Required Tin
      - renamed Recipe Custom Bronze to Produced Bronze
    + fixed the "The given key was not present in the dictionary" error
    + added a note to log out and log in for the changes to be applied

* Version 0.0.19
    + fixing typo

* Version 0.0.18
    + reworked configuration to adapt to Jotunn 2.7.0 changes and to use BepInExConfigurationManager
    (please delete your current Nitrinax.CustomizedBronze.cfg to create a new config with the current strings)

* Version 0.0.17
    + changed Guid to ModGuid
    + changed ModGuid from Company.Namespace to Company.ModName
    + changed CompatibilityLevel from EveryoneMustHaveMod to ClientMustHaveMod
    + adjusted Version string from pattern 0.0.0.0 to 0.0.0
    + removed unused code parts
    + removed unused event handling

* Version 0.0.1.6
    + updated for Valheim 0.209.8 (Unity 2020.3.33f1)
    + updated for Jotunn 2.6.10
    + renamed class PluginDependencies to Dependencies and moved it into folder Plugin
    + renamed class PluginSettings to Data and moved it into folder Plugin

* Version 0.0.1.5
    + The code has been optimized so that debug messages are not included in the release code.
    + Added file PluginDependencies.cs

* Version 0.0.1.4
    + added switch for *active* message
    + added switch for *disabled* message
    + splitted switch *showPluginInfo* in *showPluginActiveInfo* and *showPluginDisabledInfo*
    + regroup all switches for debug and release path
    + updated script for release to reflect the config (debug or release)
    + updated postbuild event in visual studio to reflect the config

* Version 0.0.1.3
    + added bool isModded
    + split README in single documents for README, INSTALLATION and CHANGELOG

* Version 0.0.1.2
    + internal version
         - changed readonly state of some vars
         - naming of variables changed to be more in line with naming conventions

* Version 0.0.1.1
    + added presets for composition of the alloy
    + added new config options

* Version 0.0.1.0
    + code used from new template
    + plugin renamed from BronzeRevamp to CustomizedBronze

* Version 0.0.0.5
    + Config category General added
    + Config option ShowChangesAtStartup added
    + changed names of some variables
    + variables set to private
    + config category custom renamed to Bronze
    + description text corrected and changed

* Version 0.0.0.4
    + code optimation

* Version 0.0.0.3
    + Configuration added to make all quantities customizable by the user.
    + Added notes for different alloys.

* Version 0.0.0.2
    + added change for the bronze recipe to make it create 2 bronze from one tin and one copper.
    + added change for the bronze5 recipe to make it create 10 bronze from 5 tin and 5 copper.

* Version 0.0.0.1
    + Initial release.