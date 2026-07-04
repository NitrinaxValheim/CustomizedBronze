using System;
using System.Reflection;
using UnityEngine;

using BepInEx;

using Jotunn;
using Jotunn.Utils;
using Logger = Jotunn.Logger;

namespace Common
{
    /// <summary>
    /// contains methods for testing versions
    /// </summary>
    public class VersionOperations
    {

        internal static string BepInExVersion => typeof(BaseUnityPlugin).Assembly.GetName().Version.ToString();

        #region[CheckVersionWithOutput]
        /// <summary>
        /// compares a dependency version string dependencyInstalledVersion with a dependency version string dependencyMinVersion
        /// </summary>
        /// <remarks>
        /// displays the result of the check in the Unity log
        /// </remarks>
        /// <returns>
        /// returns true if dependencyInstalledVersion is greater than or equal to dependencyMinVersion and false if dependencyInstalledVersion is less than dependencyMinVersion
        /// </returns>
        public static bool CheckVersionWithOutput(string pluginName, string dependencyName, string dependencyInstalledVersion, string dependencyNeededMinVersion)
        {

            // use versionCompare to compare the version strings
            if (CheckVersion(dependencyInstalledVersion, dependencyNeededMinVersion) == false)
            {

                Logger.LogError(pluginName + " need " + dependencyName + " version " + dependencyNeededMinVersion + " or higher, installed version is " + dependencyInstalledVersion + ", pls update");

                return false;

            }

            else
            {

#if (DEBUG)
                Logger.LogInfo(pluginName + " need " + dependencyName + " version " + dependencyNeededMinVersion + ", installed version is " + dependencyInstalledVersion);
#endif

                return true;

            }

        }
        #endregion

        #region[CheckVersion]
        /// <summary>
        /// compares a version string v1 with a version string v2
        /// </summary>
        /// <returns>
        /// returns true if v1 is greater than or equal to v2 and false if v1 is less than v2
        /// </returns>
        private static bool CheckVersion(string v1, string v2)
        {

            // vnum stores each numeric
            // part of version
            int i;
            int j;
            int vnum1 = 0;
            int vnum2 = 0;

            // loop untill both string are
            // processed
            for (i = 0, j = 0; i < v1.Length || j < v2.Length;)
            {

                // storing numeric part of
                // version 1 in vnum1
                while (i < v1.Length && v1[i] != '.')
                {

                    vnum1 = vnum1 * 10 + (v1[i] - '0');
                    i++;

                }

                // storing numeric part of
                // version 2 in vnum2
                while (j < v2.Length && v2[j] != '.')
                {

                    vnum2 = vnum2 * 10 + (v2[j] - '0');
                    j++;

                }

                if (vnum1 > vnum2)
                    return true;

                if (vnum2 > vnum1)
                    return false;

                // if equal, reset variables and
                // go for next numeric part
                vnum1 = vnum2 = 0;
                i++;
                j++;

            }

            return true;

        }
        #endregion

        #region[GetGameVersion]
        /// <summary>
        /// directly referencing of the class Version from assembly_valheim.dll
        /// </summary>
        /// <returns>
        /// returns m_major, m_minor, m_patch from class Version
        /// </returns>
        public static string GetGameVersion()
        {

            return GameVersions.ValheimVersion.ToString();

        }
        #endregion

        #region[GetCompatiblePlayerVersions]
        /// <summary>
        /// get all entries from m_compatiblePlayerVersions and join them to a string
        /// </summary>
        /// <returns>
        /// returns m_compatiblePlayerVersions from class Version
        /// </returns>
        private static string GetCompatiblePlayerVersions()
        {

            return string.Join(", ", Version.m_oldestForwardCompatiblePlayerVersion);

        }
        #endregion

        #region[GetCompatibleWorldVersion]
        /// <summary>
        /// get all entries from m_compatibleWorldVersions and join them to a string
        /// </summary>
        /// <returns>
        /// returns m_compatibleWorldVersions from class Version
        /// </returns>
        private static string GetCompatibleWorldVersion()
        {

            return string.Join(", ", Version.m_oldestForwardCompatibleWorldVersion);

        }
        #endregion

        #region[GetBepinExVersion]
        /// <summary>
        /// directly referencing of the class Version from assembly_valheim.dll
        /// </summary>
        /// <returns>
        /// returns m_major, m_minor, m_patch from class Version
        /// </returns>
        public static string GetBepinExVersion()
        {

            return BepInExVersion;

        }
        #endregion

        #region[GetJotunnVersion]
        /// <summary>
        /// directly referencing of the class Version from assembly_valheim.dll
        /// </summary>
        /// <returns>
        /// returns m_major, m_minor, m_patch from class Version
        /// </returns>
        public static string GetJotunnVersion()
        {

            return Jotunn.Main.Version;

        }
        #endregion

    }

}
