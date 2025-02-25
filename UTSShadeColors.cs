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
        public bool Preview { get; set; }

        [SerializeField,
            Tooltip("The primary color that the toon shading will be based on.")]
        Color _baseColor;

        [SerializeField, Range(0, 360),
            Tooltip("The amount to shift the hue of the base color, in degrees (0 to 360). This controls the color variation for the shades.")]
        int _hueShift = 5;

        [SerializeField, Range(0, 100),
            Tooltip("The difference in saturation for the first and second shaded colors, where 0 is no change and 100 is the maximum decrease in saturation.")]
        int _saturationDiff = 20;

        [SerializeField, Range(0, 100),
            Tooltip("The difference in brightness (value) for the first and second shaded colors, where 0 is no change and 100 is the maximum decrease in brightness.")]
        int _valueDiff = 20;

        public static readonly float BlueHue = 240f;
        public static readonly float YellowHue = 60f;
        public static readonly float NormalizedBlueHue = 240f / 360f;
        public static readonly float NormalizedYellowHue = 60f / 360f;

        public static readonly string BaseColor = "_BaseColor";
        public static readonly string FirstShadeColor = "_1st_ShadeColor";
        public static readonly string SecondShadeColor = "_2nd_ShadeColor";

        private MeshRenderer _renderer;
        private Material _toonMat;

        public float normalizedHueShift => _hueShift / 360f;
        public float normalizedSaturationDiff => _saturationDiff / 100f;
        public float normalizedValueDiff => _valueDiff / 100f;
        private void Start()
        {
            Init();
            SetColors();
        }

        public void SetColors()
        {
            Color firstColor = GetNextShadeColor(_baseColor);
            Color secondColor = GetNextShadeColor(firstColor);

            _toonMat.SetColor(BaseColor, _baseColor);
            _toonMat.SetColor(FirstShadeColor, firstColor);
            _toonMat.SetColor(SecondShadeColor, secondColor);
        }
#if UNITY_EDITOR
        private void Update()
        {
            if (Preview)
            {
                SetColors();
                Preview = Selection.activeObject == gameObject;
            }
        }
#endif
        public void Init()
        {
            if (!TryGetComponent(out _renderer))
            {
                Debug.LogError($"There is no MeshRenderer component attached to {gameObject}", gameObject);
            }
            else
            {
                if (Application.isEditor)
                {
                    _toonMat = new Material(_renderer.sharedMaterial);
                    _renderer.material = _toonMat;
                }
                else
                {
                    _toonMat = _renderer.material;
                }

            }
            if (_toonMat == null || _renderer == null)
            {
                Debug.LogError($"There is no MeshRenderer component attached to {gameObject}", gameObject);
                return;
            }
        }
        private float ShadowHueShift(float hue)
        {
            float hue360 = Mathf.Round(hue * 360);
            float result = 0;

            if (hue360 >= YellowHue && hue360 <= BlueHue)
            {
                result = hue360 += _hueShift;
            }
            else if (hue360 >= BlueHue)
            {
                result = hue360 - _hueShift;
            }
            else if (hue360 < YellowHue)
            {
                if (hue360 - _hueShift < 0)
                {
                    result = 360 - (_hueShift - hue360);
                }
            }
            return result / 360f;
        }
        private Color GetNextShadeColor(Color color)
        {
            float hue, saturation, value;

            Color.RGBToHSV(color, out hue, out saturation, out value);

            float newHue = ShadowHueShift(hue);
            float newSaturation = value - normalizedSaturationDiff;
            float newValue = value - normalizedValueDiff;
            newHue = Mathf.Clamp01(newHue);
            newSaturation = Mathf.Clamp01(newSaturation);
            newValue = Mathf.Clamp01(newValue);

            return Color.HSVToRGB(newHue, newSaturation, newValue);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(UTSShadeColors))]
    public class CustomUTSShadeColorsInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            UTSShadeColors uts = (UTSShadeColors)target;

            GUIStyle style = GUI.skin.button;
            string previewLabel = string.Empty;
            if (uts.Preview)
            {
                GUI.backgroundColor = Color.red;
                previewLabel = "Disable preview";
            }
            else
            {
                GUI.backgroundColor = GUI.color;
                previewLabel = "Enable preview";
            }
            GUI.backgroundColor = uts.Preview ? Color.red : GUI.color;
            if (GUILayout.Button(previewLabel, style))
            {
                uts.Preview = !uts.Preview;
                if (uts.Preview) uts.Init();
            }
            GUI.backgroundColor = GUI.color;

            DrawDefaultInspector();

            if (GUILayout.Button("Set Colors"))
            {
                uts.SetColors();
            }
        }
    }
#endif
}