using UnityEngine;

namespace HiveVolumeRenderer.UI
{
    public class QuitButton : MonoBehaviour
    {
        public void Quit()
        {
            #if UNITY_EDITOR
                 UnityEditor.EditorApplication.isPlaying = false;
            #else
                 Application.Quit();
            #endif
        }
    }
}
