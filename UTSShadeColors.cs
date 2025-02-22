using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UTS
{
    [ExecuteInEditMode]
    public class UTSShadeColors : MonoBehaviour
    {
        [SerializeField] Color _baseColor;
        [SerializeField] bool _preview = false; 
        [SerializeField, Range(0, 360)] int _hueShift = 5; 
        [SerializeField, Range(0, 100)] int _saturationDiff = 20; 
        [SerializeField, Range(0, 100)] int _valueDiff = 20;

        public static readonly float NormalizedBlueHue = 240f / 360f;
        public static readonly float NormalizedYellowHue = 120f / 360f;
        public static readonly string BaseColor = "_BaseColor";
        public static readonly string FirstShadeColor = "_1st_ShadeColor";
        public static readonly string SecondShadeColor = "_2nd_ShadeColor";

        private MeshRenderer _renderer;
        private Material _toonMat;

        public float normalizedHueShift => _hueShift / 360f;
        public float normalizedSaturationDiff => _saturationDiff / 100f;
        public float normalizedValueDiff => _valueDiff / 100f;

        public void SetColors()
        {
            if (!TryGetComponent(out _renderer))
            {
                Debug.LogError($"There is no MeshRenderer component attached to {gameObject}", gameObject);
            }
            else
            {
                _toonMat = Application.isEditor ? _renderer.sharedMaterial : _renderer.material;
            }
            if (_toonMat == null || _renderer == null)
            {
                Debug.LogError($"There is no MeshRenderer component attached to {gameObject}", gameObject);
                return;
            }
            float hue, saturation, value;

            Color.RGBToHSV(_baseColor, out hue, out saturation, out value);

            float firstHue = hue >= NormalizedBlueHue ? hue - normalizedHueShift : hue + normalizedHueShift;
            float firstSaturation = saturation - normalizedSaturationDiff;
            float firstValue = value - normalizedValueDiff;
            firstHue = Mathf.Clamp01(firstHue);
            firstSaturation = Mathf.Clamp01(firstSaturation);
            firstValue = Mathf.Clamp01(firstValue);

            Color firstColor = Color.HSVToRGB(firstHue, firstSaturation, firstValue);

            float secondHue = firstHue >= NormalizedBlueHue ? firstHue - normalizedHueShift : firstHue + normalizedHueShift;
            float secondSaturation = firstSaturation - normalizedSaturationDiff;
            float secondValue = firstValue - normalizedValueDiff;
            secondHue = Mathf.Clamp01(secondHue);
            secondSaturation = Mathf.Clamp01(secondSaturation);
            secondValue = Mathf.Clamp01(secondValue);

            Color secondColor = Color.HSVToRGB(secondHue, secondSaturation, secondValue);

            _toonMat.SetColor(BaseColor, _baseColor);
            _toonMat.SetColor(FirstShadeColor, firstColor);
            _toonMat.SetColor(SecondShadeColor, secondColor);
        }
#if UNITY_EDITOR
        private void Update()
        {
            if (_preview)
            {
                SetColors();
            }
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(UTSShadeColors))]
    public class CustomUTSShadeColorsInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            UTSShadeColors uts = (UTSShadeColors)target;

            if (GUILayout.Button("Set Colors"))
            {
                uts.SetColors();
            }
        }
    }
#endif
}