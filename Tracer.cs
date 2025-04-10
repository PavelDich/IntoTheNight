using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Mathematics;

namespace Minicop.Game.GravityRave
{
    //[RequireComponent(typeof(LineRenderer))]
    public class Tracer : MonoBehaviour
    {
        [field: SerializeField]
        public LineRenderer _lineRenderer { get; private set; }
        [field: SerializeField]
        private Vector3 StartPosition;
        [field: SerializeField]
        private Vector3 EndPosition;
        [field: SerializeField]
        private float DiffTimeLife = 0f;
        [field: SerializeField]
        private float TimeLife = 0f;
        [field: SerializeField]
        private float StartAlpha = 1f;
        [field: SerializeField]
        public UnityEvent OnInit { get; private set; }
        private void Start()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            StartAlpha = _lineRenderer.material.color.a;
        }

        public void Init(Vector3 startPosition, Vector3 endPosition)
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
            DiffTimeLife = TimeLife;

            if (_lineRenderer != null)
            {
                _lineRenderer.SetPosition(0, StartPosition);
                _lineRenderer.SetPosition(1, EndPosition);
            }
            OnInit.Invoke();
            StartCoroutine(Alpha());
        }
        private IEnumerator Alpha()
        {
            while (DiffTimeLife >= 0)
            {
                DiffTimeLife -= Time.deltaTime;
                _lineRenderer.material.color = new Color(_lineRenderer.material.color.r,
                 _lineRenderer.material.color.g,
                  _lineRenderer.material.color.b,
                  math.lerp(0f, StartAlpha, DiffTimeLife / TimeLife));
                yield return new WaitForEndOfFrame();
            }
            Destroy(gameObject);
            yield return null;
        }
    }
}