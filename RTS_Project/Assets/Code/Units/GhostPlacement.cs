using UnityEngine;

namespace Code.Units
{
    public class GhostPlacement : MonoBehaviour
    {
        private static readonly int TINT = Shader.PropertyToID("_Tint");
        private static readonly int FRESNEL = Shader.PropertyToID("_FresnelColor");

        [SerializeField, ColorUsage(true, true)] private Color errorTintColor = Color.red;
        [SerializeField, ColorUsage(true, true)] private Color errorFresnelColor = new Color(4f, 1.7f, 0f, 1);
        [SerializeField, ColorUsage(true, true)] private Color availableToPlaceTintColor = new Color(0.2f, 0.65f, 1, 1);
        [SerializeField, ColorUsage(true, true)] private Color availableToPlaceFresnelColor = new Color(0.8f, 1.5f, 2.2f, 1);
        [SerializeField] private MeshRenderer ghostRenderer;
        private bool _isPassRestriction = false;

        public void SetPassRestriction(bool isPass)
        {
            if (isPass == _isPassRestriction) return;

            _isPassRestriction = isPass;

            ghostRenderer.material.SetColor(TINT, _isPassRestriction ? availableToPlaceTintColor : errorTintColor);
            ghostRenderer.material.SetColor(FRESNEL, _isPassRestriction ? availableToPlaceFresnelColor : errorFresnelColor);
        }
    }
}