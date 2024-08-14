using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace HiveVolumeRenderer.XR.Tools
{
    public class XRToolIcon : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private XRToolRadial _prefabRadial;
        [SerializeField] private Transform _parentRadial;
        [SerializeField] private Transform _idleRadial;

        [SerializeField] private float _radius;
        [SerializeField] private float _depth;
        [SerializeField] private float _idleTimeout;
        [SerializeField] private float _idleTransition;

        private List<XRToolRadial> _radialIcons = new();

        private bool _isActive = false;
        private float _secondsSinceLastInteraction;

        private void Awake()
        {
            _idleRadial.localPosition = Vector3.forward * _depth;
            _parentRadial.localPosition = Vector3.forward * _depth * 2;

            _idleRadial.localScale = Vector3.one;
            _parentRadial.localScale = Vector3.zero;
        }

        public void RegisterTool(XRTool tool)
        {
            XRToolRadial radialIcon = Instantiate(_prefabRadial, _parentRadial);
            radialIcon.Inject(tool);
            _radialIcons.Add(radialIcon);

            float arcDegreesPerSegment = 360f / _radialIcons.Count;

            for (int i = 0; i < _radialIcons.Count; i++)
            {
                _radialIcons[i].SetPosition(_radius, _depth, i * arcDegreesPerSegment, arcDegreesPerSegment);
            }
        }

        public void SetActiveTool(XRTool tool, bool setActive = true)
        {
            if (setActive)
            {
                if (!_isActive)
                {
                    _idleRadial.DOScale(0f, _idleTransition);
                    _parentRadial.DOScale(1f, _idleTransition);
                }

                _isActive = true;
                _secondsSinceLastInteraction = 0f;
            }

            _renderer.material.mainTexture = tool.Icon;

            foreach (var icon in _radialIcons)
                icon.SetActive(icon.Tool == tool);
        }

        private void Update()
        {
            if (!_isActive) return;

            _secondsSinceLastInteraction += Time.deltaTime;

            if (_secondsSinceLastInteraction > _idleTimeout)
            {
                _idleRadial.DOScale(1f, _idleTransition);
                _parentRadial.DOScale(0f, _idleTransition);
                _isActive = false;
            }
        }
    }
}
