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
    }

    [Serializable]
    public class UnitCameraVisuals
    {
        public BaseUnit.UnitTypes UnitTypes;
        public GameObject unitGameObject;
    }
}