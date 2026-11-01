using System;
using UnityEngine;

namespace Code.UI.Components
{
    public class ProgressBarUI : MonoBehaviour
    {
        [SerializeField] private RectTransform maskTrm;
        
        [SerializeField] private Vector2 padding = new Vector2(12f, 4f);
        private RectTransform _maskParentRectTrm;

        private void Awake()
        {
            _maskParentRectTrm = maskTrm.parent.GetComponent<RectTransform>();
        }

        public void SetProgress(float value)
        {
            Vector2 parentSize = _maskParentRectTrm.sizeDelta;
            Vector2 targetSize = parentSize - padding * 2;
            targetSize.x *= Mathf.Clamp01(value);
            
            maskTrm.offsetMin = padding;
            maskTrm.offsetMax = new Vector2(padding.x + targetSize.x - parentSize.x, -padding.y);
        }
    }
}