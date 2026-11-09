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
        private MaterialPropertyBlock _propertyBlock;
        private bool _isPassRestriction = false;

        private void Awake()
        {
            // Some ghost prefabs reference a renderer from the source building prefab.
            // Resolve the renderer from this instantiated ghost instead.
            if (ghostRenderer == null || (ghostRenderer.transform != transform
                && !ghostRenderer.transform.IsChildOf(transform)))
                ghostRenderer = GetComponentInChildren<MeshRenderer>(true);

            _propertyBlock = new MaterialPropertyBlock();
        }

        public void SetPassRestriction(bool isPass)
        {
            if (isPass == _isPassRestriction) return;

            _isPassRestriction = isPass;

            if (ghostRenderer == null)
            {
                Debug.LogWarning($"{name}: GhostPlacement에 사용할 MeshRenderer가 없습니다.", this);
                return;
            }

            ghostRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(TINT, _isPassRestriction ? availableToPlaceTintColor : errorTintColor);
            _propertyBlock.SetColor(FRESNEL, _isPassRestriction ? availableToPlaceFresnelColor : errorFresnelColor);
            ghostRenderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
