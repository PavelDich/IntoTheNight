using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minicop.Game.GravityRave
{
    public class AimVisual : MonoBehaviour
    {
        [field: SerializeField]
        public Transform _transform { get; private set; }
        private void Start()
        {
            _transform = transform;
        }
        public GameObject AimPoint;
        public void Aim(Vector3 gun, Vector3 target)
        {
            AimPoint.SetActive(true);
            _transform.position = target;
            _transform.LookAt(gun);
        }
        public void StopAiming()
        {
            AimPoint.SetActive(false);
        }
    }
}