using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
namespace Games.EditorOnly
{
    [CustomEditor(typeof(ShaderGlobals))]
    public class ShaderGlobals_Editor : Editor
    {
        ShaderGlobals _target;

        public override void OnInspectorGUI()
        {
            _target = (ShaderGlobals)target;
            
            base.OnInspectorGUI();
        }
    }
}
#endif

public class ShaderGlobals : MonoBehaviour
{
    [Tooltip("Value used to cycle time at a constant rate. Also controls the frame update rate.")]
    public float deltaTime = .01667f;

    [Tooltip("All materials here will have their shaders passed these globals each frame")]
    public List<Material> materialsToUpdate;

    [SerializeField, Tooltip("Use this in shaders instead of unity's Time to avoid decimal rounding issues")]
    string timeCycleName2PI = "_Safe2PITime";

    [SerializeField, Tooltip("Use this in shaders instead of unity's Time to avoid decimal rounding issues")]
    string timeCycleName10f = "_Safe10fTime";

    /// <summary>
    /// Cycles from approximately 0 to 2PI, may be slightly under 0f on the frame it cycles around
    /// </summary>
    float _2PITimeCycle = 0f;

    /// <summary>
    /// Cycles from approximately 0 to 10f, may be slightly under 0f on the frame it cycles around
    /// </summary>
    float _10fTimeCycle = 0f;

    IEnumerator _updateFunction;

	void OnEnable() 
    {
        if(_updateFunction == null)
            _updateFunction = ShaderUpdate();

        //start or resume the update function
        StartCoroutine(_updateFunction);
    }

    void OnDisable()
    {
        StopCoroutine( _updateFunction );
        _updateFunction = null;
    }

    IEnumerator ShaderUpdate()
    {
        float twoPI = Mathf.PI * 2f;
        for(;;)
        {
            float inc = deltaTime * Time.timeScale;

            _2PITimeCycle += inc;
            while(_2PITimeCycle > twoPI)
                _2PITimeCycle -= twoPI;

            _10fTimeCycle += inc;
            while(_10fTimeCycle > 10f)
                _10fTimeCycle -= 10f;

            for(int i = 0; i < materialsToUpdate.Count; ++i)
            {
                materialsToUpdate[i].SetFloat(timeCycleName2PI, _2PITimeCycle);
                materialsToUpdate[i].SetFloat(timeCycleName10f, _10fTimeCycle);
            }

            yield return new WaitForSeconds( inc );
        }
    }
}
