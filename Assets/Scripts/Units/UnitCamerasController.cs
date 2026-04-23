using System;
using System.Collections.Generic;
using Units.UnitTypes;
using UnityEngine;

namespace Units
{
    public class UnitCamerasController : MonoBehaviour
    {
        [SerializeField] private List<UnitCameraVisuals> cameraVisuals;

        public void Init(List<BaseUnit.UnitTypes> equippedUnits)
        {
            foreach (var visual in cameraVisuals)
                visual.unitGameObject.SetActive(equippedUnits.Contains(visual.UnitTypes));
        }

        public RenderTexture EnableUnitCamera(BaseUnit.UnitTypes unitType)
        {
            var visual = cameraVisuals.Find(v => v.UnitTypes == unitType);
            if (visual == null) return null;

            foreach (var v in cameraVisuals)
                v.unitGameObject.SetActive(v == visual);

            if (visual.unitCamera == null) return null;

            visual.unitCamera.enabled = true;
            visual.unitCamera.Render();
            return visual.unitCamera.targetTexture;
        }
    }

    [Serializable]
    public class UnitCameraVisuals
    {
        public BaseUnit.UnitTypes UnitTypes;
        public GameObject unitGameObject;
        public Camera unitCamera;
    }
}