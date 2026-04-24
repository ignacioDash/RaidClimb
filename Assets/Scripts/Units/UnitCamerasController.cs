using System;
using System.Collections.Generic;
using Units.UnitTypes;
using UnityEngine;

namespace Units
{
    public class UnitCamerasController : MonoBehaviour
    {
        [SerializeField] private List<UnitCameraVisuals> cameraVisuals;
        [SerializeField] private List<UnitCameraVisuals> fullBodyCameraVisuals;

        public void Init(List<BaseUnit.UnitTypes> equippedUnits)
        {
            foreach (var visual in cameraVisuals)
                visual.unitGameObject.SetActive(equippedUnits.Contains(visual.UnitTypes));
        }

        public void ShowAllUnits()
        {
            foreach (var visual in cameraVisuals)
            {
                visual.unitGameObject.SetActive(true);
                if (visual.unitCamera)
                {
                    visual.unitCamera.enabled = true;
                    visual.unitCamera.Render();
                }
            }
        }

        public RenderTexture GetRenderTexture(BaseUnit.UnitTypes unitType)
        {
            var visual = cameraVisuals.Find(v => v.UnitTypes == unitType);
            if (visual?.unitCamera == null) return null;
            visual.unitCamera.Render();
            return visual.unitCamera.targetTexture;
        }

        public void ShowFullBodyUnit(BaseUnit.UnitTypes unitType)
        {
            var visual = fullBodyCameraVisuals.Find(v => v.UnitTypes == unitType);
            foreach (var v in fullBodyCameraVisuals)
                v.unitGameObject.SetActive(v == visual);
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