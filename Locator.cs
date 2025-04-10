using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;
using UnityEngine.ProBuilder.Shapes;

namespace Minicop.Game.GravityRave
{
    public class Locator : MonoBehaviour
    {
        [SerializeField]
        private Collider _trigger;
        [SerializeField]
        private Transform _locatePoint;
        public float TargebleRadius;
        public float TargebleDistance;
        public bool IsUseZone;
        public float CheckZoneTime;
        public LayerMask TargetLocateLayer;
        public LayerMask TargebleLayer;

        public UnityEvent<Vector3> OnTargetFound = new UnityEvent<Vector3>();

        private List<Transform> _targets = new List<Transform>();

        private void Start()
        {
            StartCoroutine(CheckZone());
        }
        private IEnumerator CheckZone()
        {
            yield return new WaitForSeconds(CheckZoneTime);
            yield return new WaitForEndOfFrame();


            if (Physics.SphereCast(_locatePoint.position + Vector3.forward, TargebleRadius, _locatePoint.forward,
                out RaycastHit sphereCastHit, TargebleDistance, TargetLocateLayer))
            {
                if (Physics.Raycast(_locatePoint.position, (sphereCastHit.transform.position - transform.position),
                out RaycastHit raycastHit, TargebleDistance, TargebleLayer))
                {
                    if ((~TargetLocateLayer & (1 << raycastHit.transform.gameObject.layer)) == 0)
                        OnTargetFound.Invoke(raycastHit.transform.position);
                }
            }

            yield return StartCoroutine(CheckZone());
        }
        private void OnTriggerEnter(Collider col)
        {
            if ((~TargetLocateLayer & (1 << col.gameObject.layer)) == 0)
            {
                _targets.Add(col.transform);
            }
        }
        private void OnTriggerExit(Collider col)
        {
            if ((~TargetLocateLayer & (1 << col.gameObject.layer)) == 0)
            {
                _targets.Remove(col.transform);
            }
        }
    }
}