using UnityEngine;
using TMPro;

namespace HiveVolumeRenderer.XR.Tools.Window
{
    public class WindowCustomIndicator : MonoBehaviour
    {
        [SerializeField] private WindowCustomController _controller;

        [Header("Regions")]
        [SerializeField] private Transform _activeRegion;
        [SerializeField] private Transform _lowerRegion;
        [SerializeField] private Transform _upperRegion;

        [Header("Level")]
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private Transform _levelIndicator;

        [Header("Width")]
        [SerializeField] private TMP_Text _widthText;
        [SerializeField] private Transform _widthLowerIndicator;
        [SerializeField] private Transform _widthLowerConnector;
        [SerializeField] private Transform _widthUpperConnector;
        [SerializeField] private Transform _widthUpperIndicator;

        private void LateUpdate()
        {
            UpdateRegions();
            UpdateLevelIndicator();
            UpdateWidthIndicator();
        }


        private void UpdateRegions()
        {
            SetRegion(_activeRegion, _controller.WindowLevel, _controller.WindowWidth);

            float lowerLevel = Mathf.Lerp(_controller.WindowMin, _controller.lowerBound, 0.5f);
            float lowerWidth = _controller.lowerBound - _controller.WindowMin;
            SetRegion(_lowerRegion, lowerLevel, lowerWidth);

            float upperLevel = Mathf.Lerp(_controller.upperBound, _controller.WindowMax, 0.5f);
            float upperWidth = _controller.WindowMax - _controller.upperBound;
            SetRegion(_upperRegion, upperLevel, upperWidth);
        }

        private void SetRegion(Transform region, float level, float width)
        {
            float normalisedLevel = (level - _controller.WindowMin) / (_controller.WindowMax - _controller.WindowMin);
            float rescaledLevel = normalisedLevel - 0.5f; // Rescale to (-0.5f, 0.5f)
            region.localPosition = new Vector3(rescaledLevel, 0f, 0f);

            float normalisedWidth = width / (_controller.WindowMax - _controller.WindowMin);
            region.localScale = new Vector3(normalisedWidth, 1f, 1f);
        }

        private void UpdateLevelIndicator()
        {
            _levelText.text = _controller.WindowLevel.ToString("0");

            _levelText.transform.localPosition = new Vector3(_activeRegion.localPosition.x, _levelText.transform.localPosition.y, 0f);
            _levelIndicator.localPosition = new Vector3(_activeRegion.localPosition.x, _levelIndicator.localPosition.y, 0f);
        }

        private void UpdateWidthIndicator()
        {
            _widthText.text = _controller.WindowWidth.ToString("0");

            _widthText.transform.localPosition = new Vector3(_activeRegion.localPosition.x, _widthText.transform.localPosition.y, 0f);

            float normalisedLowerBound = (_controller.lowerBound - _controller.WindowMin) / (_controller.WindowMax - _controller.WindowMin);
            float rescaledLowerBound = (normalisedLowerBound - 0.5f); // Rescale to (-0.5f, 0.5f)
            _widthLowerIndicator.localPosition = new Vector3(rescaledLowerBound, _widthLowerIndicator.localPosition.y, 0f);
            _widthLowerConnector.localPosition = new Vector3(rescaledLowerBound, _widthLowerConnector.localPosition.y, 0f);

            float normalisedUpperBound = (_controller.upperBound - _controller.WindowMin) / (_controller.WindowMax - _controller.WindowMin);
            float rescaledUpperBound = (normalisedUpperBound - 0.5f); // Rescale to (-0.5f, 0.5f)
            _widthUpperIndicator.localPosition = new Vector3(rescaledUpperBound, _widthUpperIndicator.localPosition.y, 0f);
            _widthUpperConnector.localPosition = new Vector3(rescaledUpperBound, _widthUpperConnector.localPosition.y, 0f);

            float normalisedWidth = (normalisedUpperBound - normalisedLowerBound);
            float connectorGap = _widthText.rectTransform.rect.width;
            _widthLowerConnector.localScale = new Vector3((normalisedWidth - connectorGap) / 2f, _widthLowerConnector.localScale.y, _widthLowerConnector.localScale.z);
            _widthUpperConnector.localScale = new Vector3((normalisedWidth - connectorGap) / 2f, _widthLowerConnector.localScale.y, _widthLowerConnector.localScale.z);
        }
    }
}
