// System
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// BepInEx
using BepInEx;

using Logger = Jotunn.Logger;

using Plugin;

namespace Common
{
    public class DependencyOperations
    {

        public static bool CheckForDependencyErrors(string pluginName) {

            bool dependenciesError = false;

            //Plugin.Dependencies.GameName
            //Plugin.Dependencies.GameVersion
            if (VersionOperations.CheckVersionWithOutput(
                pluginName,
                Dependencies.GameName,
                VersionOperations.GetGameVersion(),
                Dependencies.GameVersion
                ) == false) { dependenciesError = true; }

            //Plugin.Dependencies.BepInExName
            //Plugin.Dependencies.BepInExVersion
            if (VersionOperations.CheckVersionWithOutput(
                pluginName,
                Dependencies.BepInExName,
                VersionOperations.GetBepinExVersion(),
                Dependencies.BepInExVersion
                ) == false) { dependenciesError = true; }

            //Plugin.Dependencies.IsJotunnRequired
            //Plugin.Dependencies.JotunnName
            //Plugin.Dependencies.JotunnVersion
            if (Dependencies.IsJotunnRequired == true)
            {
                if (VersionOperations.CheckVersionWithOutput(
                    pluginName,
                    Dependencies.JotunnName,
                    VersionOperations.GetJotunnVersion(),
                    Dependencies.JotunnVersion
                    ) == false) { dependenciesError = true; }
            }

            //Plugin.Dependencies.RequiredPlugins
            if (Dependencies.RequiredPlugins.Length > 0)
            {
                if (!FileOperations.CheckRequiredPluginDependencies(Dependencies.RequiredPlugins))
                {  
                    dependenciesError = true;
                }
            }

            //Plugin.Dependencies.RequiredFiles
            if (Dependencies.RequiredFiles.Length > 0)
            {
                if (!FileOperations.CheckFileDependencies(Dependencies.RequiredFiles))
                { 
                    dependenciesError = true;
                }
            }

            //Plugin.Dependencies.RequiredFiles
            if (Dependencies.RecommendedPlugins.Length > 0 && FileOperations.CheckRecommendedPluginDependencies(Dependencies.RecommendedPlugins) > 0)
            {
                Logger.LogInfo("Some recommended plugins could not be found. The mod's functionality may be limited.");
            }

            return dependenciesError;
        
        }

    }

}
