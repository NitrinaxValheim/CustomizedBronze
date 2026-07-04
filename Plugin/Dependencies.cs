using System;

namespace Plugin
{ // namespace start

    class Dependencies
    { // class start

        #region[PluginDependencies]

        // define needed Valheim name
        public const string GameName = "Valheim";
        // define needed Valheim version
        public const string GameVersion = "0.221.12";

        // define needed BepInEx name
        public const string BepInExName = "BepInExPack Valheim";
        // define needed BepInEx version
        public const string BepInExVersion = "5.4.23.3";

        #endregion

        #region[JotunnDependencies]

        // set to true if Jotunn required
        public const bool IsJotunnRequired = true;

        // define needed Jotunn name
        public const string JotunnName = "Jotunn, the Valheim Library";
        // define needed Jotunn version
        public const string JotunnVersion = "2.29.1";

        #endregion

        #region[RequiredPlugins]

        // define needed plugins list
        public static string[] RequiredPlugins = Array.Empty<string>();

        #endregion

        #region[RequiredFiles]

        // define needed files list
        public static string[] RequiredFiles = Array.Empty<string>();

        #endregion

        #region[RecommendedPlugins]

        public static string[,] RecommendedPlugins =
        {
            { "ConfigurationManager.dll", "This is required to change the mod’s settings within the game. Without this plugin, settings can only be adjusted by editing the configuration file." },
        };

        #endregion

    } // class end

} // namespace end