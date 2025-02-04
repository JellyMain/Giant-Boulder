using PWCommon5;
namespace GeNa.Core
{
    public class GeNaEditor : PWEditor
    {
        #region Variables
        private GeNaStyles m_styles;
        private GeNaSpawner m_geNaSpawner = null;
        protected EditorUtils m_editorUtils;
        protected bool m_inited = false;
        #endregion
        #region Properties
        protected GeNaStyles Styles => m_styles;
        protected GeNaSpawner GeNaSpawner => m_geNaSpawner;
        #endregion
        #region Methods
        public void SetSpawner(GeNaSpawner spawner) => m_geNaSpawner = spawner;
        protected void Initialize()
        {
            // Initialize GUI
            if (m_styles == null || m_inited == false)
            {
                m_styles?.Dispose();
                m_styles = new GeNaStyles();
                m_inited = true;
            }
            // Initialize Editor Utils (if it exists)
            m_editorUtils?.Initialize();
        }
        protected virtual void OnDestroy() => m_styles?.Dispose();
        public override void OnInspectorGUI() => Initialize();
        public virtual void OnSceneGUI()
        {
        }
        #endregion
    }
}