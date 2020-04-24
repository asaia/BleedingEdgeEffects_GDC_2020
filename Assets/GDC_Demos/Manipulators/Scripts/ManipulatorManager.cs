using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]
public class ManipulatorManager : MonoBehaviour
{
    public const int MAX_MANIPULATORS = 30;
    List<Manipulator> _manipulators = new List<Manipulator>(MAX_MANIPULATORS);

    Vector4[] _anchorpositions = new Vector4[MAX_MANIPULATORS];
    float[] _hardnesses = new float[MAX_MANIPULATORS];
    float[] _radii = new float[MAX_MANIPULATORS];

    public class ShaderParamaters
    {
        public static int numManipulatorsID = Shader.PropertyToID("_NumManipulators");
        public static int anchorPositionsID = Shader.PropertyToID("_AnchorPositions");
        public static int hardnessesID = Shader.PropertyToID("_Hardnesses");
        public static int radiiID = Shader.PropertyToID("_Radii");
        public static int RotationsID = Shader.PropertyToID("_Rotations");
        public static int TranslationsID = Shader.PropertyToID("_Translations");
        public static int ScaleFactorsID = Shader.PropertyToID("_ScaleFactors");
        public static string VISUALIZE_FALLOFF = "_VISUALIZE_FALLOFF";
        public static string DEBUG_NORMALS = "_DEBUG_NORMALS";
        public static string CORRECT_NORMALS = "_CORRECT_NORMALS";
    }

    [Header("Debug Visualize")]
    [SerializeField] bool _visualizeFalloff = false;
    [SerializeField] bool _debugNormals = false;
    [SerializeField] bool _correctNormals = true;

    Vector4[] _rotations = new Vector4[MAX_MANIPULATORS];
    Vector4[] _translations = new Vector4[MAX_MANIPULATORS];
    float[] _scaleFactors = new float[MAX_MANIPULATORS];

    void Update()
    {
        int numManipulators = Mathf.Min(_manipulators.Count, MAX_MANIPULATORS);
        for (int i = 0; i < numManipulators; i++)
        {
            Manipulator manipulator = _manipulators[i];
            _anchorpositions[i] = manipulator.transform.position;
            _hardnesses[i] = manipulator.hardness;
            _radii[i] = manipulator.transform.lossyScale.magnitude;

            Quaternion rotation = manipulator.handle.rotation * Quaternion.Inverse(manipulator.transform.rotation);
            float angle;
            Vector3 axis;
            rotation.ToAngleAxis(out angle, out axis);
            if (angle == 0.0f)
            {
                axis = new Vector3(1, 0, 0);
                angle = 0.0001f;
            }

            _rotations[i] = axis * (Mathf.Deg2Rad * angle);
            _scaleFactors[i] = Mathf.Max(Mathf.Max(manipulator.handle.transform.lossyScale.x / manipulator.transform.lossyScale.x, manipulator.handle.transform.lossyScale.y / manipulator.transform.lossyScale.y), manipulator.handle.transform.lossyScale.z / manipulator.transform.lossyScale.z) - 1;
            _translations[i] = manipulator.handle.transform.position - manipulator.transform.position;
        }

        Shader.SetGlobalInt(ShaderParamaters.numManipulatorsID, numManipulators);
        Shader.SetGlobalVectorArray(ShaderParamaters.anchorPositionsID, _anchorpositions);
        Shader.SetGlobalFloatArray(ShaderParamaters.hardnessesID, _hardnesses);
        Shader.SetGlobalFloatArray(ShaderParamaters.radiiID, _radii);
        Shader.SetGlobalVectorArray(ShaderParamaters.RotationsID, _rotations);
        Shader.SetGlobalVectorArray(ShaderParamaters.TranslationsID, _translations);
        Shader.SetGlobalFloatArray(ShaderParamaters.ScaleFactorsID, _scaleFactors);

        ManipulatorManager.SetKeyword(ShaderParamaters.VISUALIZE_FALLOFF, _visualizeFalloff);
        ManipulatorManager.SetKeyword(ShaderParamaters.DEBUG_NORMALS, _debugNormals);
        ManipulatorManager.SetKeyword(ShaderParamaters.CORRECT_NORMALS, _correctNormals);
    }

    public static void SetKeyword(string keyword, bool value)
    {
        if (value)
		{
        	Shader.EnableKeyword(keyword);
		}
		else
		{
			Shader.DisableKeyword(keyword);
		}
    }

    public void Add(Manipulator manipulator)
    {
        _manipulators.Add(manipulator);
    }

    public void Remove(Manipulator manipulator)
    {
        _manipulators.Remove(manipulator);
    }
}
