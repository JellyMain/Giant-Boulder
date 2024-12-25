// Copyright © 2018 Procedural Worlds Pty Limited.  All Rights Reserved.
using UnityEngine;
using UnityEditor;
using PWCommon5;
using GeNa.Core;
namespace GeNa.Core
{
    public class GeNaStdMenu : Editor
    {
        /// <summary>
        /// Show tutorials
        /// </summary>
        [MenuItem("Window/" + PWConst.COMMON_MENU + "/GeNa/Show GeNa Tutorials...", false, 60)]
        public static void ShowTutorial() => Application.OpenURL(PWApp.CONF.TutorialsLink);
        /// <summary>
        /// Show support page
        /// </summary>
        [MenuItem("Window/" + PWConst.COMMON_MENU + "/GeNa/Show GeNa Support, Lodge a Ticket...", false, 61)]
        public static void ShowSupport() => Application.OpenURL(PWApp.CONF.SupportLink);
        /// <summary>
        /// Show review option
        /// </summary>
        [MenuItem("Window/" + PWConst.COMMON_MENU + "/GeNa/Please Review GeNa...", false, 62)]
        public static void ShowProductAssetStore() => Application.OpenURL(PWApp.CONF.ASLink);
    }
}