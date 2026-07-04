// System
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Globalization;

// UnityEngine
using UnityEngine;
using UnityEngine.SceneManagement;

// BepInEx
using BepInEx;
using BepInEx.Configuration;

// Jotunn
using Jotunn;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.GUI;
using Jotunn.Managers;
using Jotunn.Utils;
using Logger = Jotunn.Logger;

using Plugin;

namespace Common
{
    public class FileOperations
    {

        #region[CheckRequiredPluginDependencies]
        public static Boolean CheckRequiredPluginDependencies(string[] plugins)
        {

            bool allPluginsFound = true;

            foreach (string plugin in plugins)
            {

#if (DEBUG)
                Logger.LogInfo("CheckRequiredPluginDependencies = " + plugin);
#endif

                string[] entries = Directory.GetFiles(@BepInEx.Paths.PluginPath, plugin, SearchOption.AllDirectories);

                if (entries.Length == 0)
                {

                    Logger.LogInfo("Needed plugin " + plugin + " not installed.");

                    allPluginsFound = false;

                }

            }

            return allPluginsFound;

        }
        #endregion

        #region[checkFileDependencies]
        public static Boolean CheckFileDependencies(string[] files)
        {

            bool allFilesFound = true;

            foreach (string file in files)
            {

                var fileName = Path.Combine(BepInEx.Paths.PluginPath, Data.Namespace, file);

#if (DEBUG)
                Logger.LogInfo("CheckFileDependencies = " + fileName);
#endif

                if (File.Exists(fileName) == false)
                {

                    Logger.LogError("File " + fileName + " not exists.");

                    allFilesFound = false;

                }

            }

            return allFilesFound;

        }
        #endregion

        #region[CheckRecommendedPluginDependencies]
        public static int CheckRecommendedPluginDependencies(string[,] plugins)
        {
            int missingCount = 0;

            for (int i = 0; i < plugins.GetLength(0); i++)
            {
                string fileName = plugins[i, 0];
                string fileComment = plugins[i, 1];

#if (DEBUG)
                Logger.LogInfo("CheckRecommendedPluginDependencies = " + fileName);
#endif

                string[] found = Directory.GetFiles(BepInEx.Paths.PluginPath, fileName, SearchOption.AllDirectories);

                if (found.Length == 0)
                {
                    missingCount++;

                    Logger.LogInfo("Recommended plugin " + fileName + " (" + fileComment + ") not installed.");

                }

            }

            return missingCount;

        }
        #endregion

    }

}
