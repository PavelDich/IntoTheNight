using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Minicop.Game.GravityRave
{
    public class Fog : MonoBehaviour
    {
        [SerializeField]
        private MeshRenderer _meshRenderer;
        void Start()
        {
            DayCycle.OnWorldSunState.AddListener(SetStrengthFog);
        }

        private void SetStrengthFog(float value)
        {
            _meshRenderer.material.color = new Color(
                _meshRenderer.material.color.r,
                _meshRenderer.material.color.g,
                _meshRenderer.material.color.b,
                math.abs(value - 1));
        }
    }
}