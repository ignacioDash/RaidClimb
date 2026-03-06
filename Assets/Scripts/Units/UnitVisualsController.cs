using System;
using UnityEngine;

namespace Units
{
    public class UnitVisualsController : MonoBehaviour
    {
        [SerializeField] private MeshRenderer unitMesh;
        
        [Header("Settings")]
        [SerializeField] private Color invalidColor = Color.red;

        private Color _validColor;
        private MaterialPropertyBlock _mpb;
        private static readonly int ColorProperty = Shader.PropertyToID("_BaseColor");

        private void OnEnable()
        {
            _validColor = unitMesh.material.color;
        }

        public void SetUnitVisuals(bool canDrop)
        {
            _mpb ??= new MaterialPropertyBlock();

            unitMesh.GetPropertyBlock(_mpb);
            _mpb.SetColor(ColorProperty, canDrop ? _validColor : invalidColor);
            unitMesh.SetPropertyBlock(_mpb);
        }
    }
}