// Copyright © 2018 Procedural Worlds Pty Limited.  All Rights Reserved.
using UnityEditor;
using PWCommon5;
namespace GeNa.Core
{
    [InitializeOnLoad]
    public static class PWApp
    {
        #region Variables
        public const string CONF_NAME = "GeNa";
        private static AppConfig m_conf;
        #endregion
        #region Properties
        public static AppConfig CONF
        {
            get
            {
                if (m_conf != null)
                    return m_conf;
                m_conf = AssetUtils.GetConfig(CONF_NAME);
                if (m_conf != null)
                    Prod.Checkin(m_conf);
                return m_conf;
            }
        }
        #endregion
        #region Constructors
        static PWApp()
        {
            // On Import Package Completed
            GeNaEditorEvents.onImportPackageCompleted -= OnImportPackageCompleted;
            GeNaEditorEvents.onImportPackageCompleted += OnImportPackageCompleted;
            // On Import Package Cancelled
            GeNaEditorEvents.onImportPackageCancelled -= OnImportPackageCancelled;
            GeNaEditorEvents.onImportPackageCancelled += OnImportPackageCancelled;
            // On Import Package Failed
            GeNaEditorEvents.onImportPackageFailed -= OnImportPackageFailed;
            GeNaEditorEvents.onImportPackageFailed += OnImportPackageFailed;
            m_conf = AssetUtils.GetConfig(CONF_NAME, true);
            // In case it was a script only import: let's check-in.
            if (m_conf != null)
                Prod.Checkin(m_conf);
        }
        #endregion
        #region Methods
        /// <summary>
        /// Called when a package import is Completed.
        /// </summary>
        private static void OnImportPackageCompleted(string packageName)
        {
#if PW_DEBUG
            Debug.LogFormat("[PWApp]: '{0}' Import Completed", packageName);
#endif
            OnPackageImport();
        }
        /// <summary>
        /// Called when a package import is Cancelled.
        /// </summary>
        private static void OnImportPackageCancelled(string packageName) => OnPackageImport();
        /// <summary>
        /// Called when a package import fails.
        /// </summary>
        private static void OnImportPackageFailed(string packageName, string error) => OnPackageImport();
        /// <summary>
        /// Used to run things after a package was imported.
        /// </summary>
        private static void OnPackageImport()
        {
            if (m_conf == null)
                m_conf = AssetUtils.GetConfig(CONF_NAME);
            Prod.Checkin(m_conf);
            // No need for these anymore
            GeNaEditorEvents.onImportPackageCompleted -= OnImportPackageCompleted;
            GeNaEditorEvents.onImportPackageCancelled -= OnImportPackageCancelled;
            GeNaEditorEvents.onImportPackageFailed -= OnImportPackageFailed;
        }
        /// <summary>
        /// Get an editor utils object that can be used for common Editor stuff - DO make sure to Dispose() the instance.
        /// </summary>
        /// <param name="editorObj">The class that uses the utils. Just pass in "this".</param>
        /// <param name="classNameOverride"></param>
        /// <param name="customUpdateMethod">(Optional) The method to be called when the GUI needs to be updated. (Repaint will always be called.)</param>
        /// <returns>Editor Utils</returns>
        public static EditorUtils GetEditorUtils(IPWEditor editorObj, string classNameOverride = null, System.Action customUpdateMethod = null) => new EditorUtils(CONF, editorObj, classNameOverride, customUpdateMethod);
        /// <summary>
        /// Get an editor utils object that can be used for common Editor stuff - DO make sure to Dispose() the instance.
        /// </summary>
        /// <param name="editorObj">The class that uses the utils. Just pass in "this".</param>
        /// <param name="customUpdateMethod">(Optional) The method to be called when the GUI needs to be updated. (Repaint will always be called.)</param>
        /// <param name="customNewsURL">(Optional) Custom News URL to fetch the news messages from (will default to the News URL in app config if none provided)</param>
        /// <param name="overrideParameters">A custom set of URL Parameters to use when fetching news data. If left empty, the default set of parameters will be used</param>
        /// <returns>Editor Utils</returns>
        public static EditorUtils GetEditorUtils(IPWEditor editorObj, System.Action customUpdateMethod = null, string customNewsURL = null, URLParameters overrideParameters = null) => new EditorUtils(CONF, editorObj, null, customUpdateMethod, customNewsURL, overrideParameters);
        #endregion
    }
}