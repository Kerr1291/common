using UnityEngine;

namespace nv
{
    public class SceneAssetPathField : PropertyAttribute
    {
        //Taken from: https://gist.github.com/joltmode/794b113b1d9fc2df54e890f123f63312
        // While there are various options to tweak the code above
        // to suit other needs (store only name, loadable path, maybe even index - requires editor script rewrite, though)
        // this helper comes in handy for the version found here.
        // Useful because in editor, the scene is saved with Assets/ prefix an *.unity suffix, but
        // in build the prefix is removed and SceneManager works without extension.
        // Not a fully optimized one, but hey - this is just a tip! ;)
        //  Example:
        //  public class SceneReferencingObject : MonoBehaviour
        // {
        //    [SerializeField, SceneAssetPathField]
        //    private string referencedScenePath;
        // }
        public static string LoadableName(string path)
        {
            string start = "Assets/";
            string end = ".unity";

            if(path.EndsWith(end))
            {
                path = path.Substring(0, path.LastIndexOf(end));
            }

            if(start.StartsWith(start))
            {
                path = path.Substring(start.Length);
            }

            return path;
        }
    }
}