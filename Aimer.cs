using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace Minicop.Game.GravityRave
{
    public class Aimer : NetworkBehaviour
    {
        #region System
        [field: SerializeField]
        private Transform _aimPoint;
        [field: SerializeField]
        private Transform _rotateTransform;
        [field: SerializeField]
        private Transform _shootPoint;
        [field: SerializeField]
        private NetworkIdentity _networkIdentity;

        protected void Start()
        {
            _aimPoint ??= GetComponent<Transform>();
            _rotateTransform ??= GetComponent<Transform>();
            _shootPoint ??= GetComponent<Transform>();
            _networkIdentity ??= GetComponentInParent<NetworkIdentity>();
            _startRotation = _rotateTransform.rotation;
        }
        private void Update()
        {
            Aim();
        }
        #endregion



        #region Aim
        public float Distance
        {
            get { return _data.Distance; }
            set { _data.Distance = value; }
        }
        public float AimRadius
        {
            get { return _data.AimRadius; }
            set { _data.AimRadius = value; }
        }
        public float Speed
        {
            get { return _data.Speed; }
            set { _data.Speed = value; }
        }
        public float LostTime
        {
            get { return _data.LostTime; }
            set { _data.LostTime = value; }
        }
        public float FoundTime
        {
            get { return _data.FoundTime; }
            set { _data.FoundTime = value; }
        }
        public LayerMask TargetAimLayer
        {
            get { return _data.TargetAimLayer; }
            set { _data.TargetAimLayer = value; }
        }
        public LayerMask TargebleLayer
        {
            get { return _data.TargebleLayer; }
            set { _data.TargebleLayer = value; }
        }
        [SerializeField, HideInInspector]
        private bool IsAiming;

        private Quaternion _startRotation;
        private Transform _target;
        private Coroutine _waiterLost;
        private Coroutine _waiterFound;



        [field: SerializeField]
        public UnityEvent<Vector3, Vector3> OnTargetFound { get; private set; } = new UnityEvent<Vector3, Vector3>();
        [field: SerializeField]
        public UnityEvent OnTargetLost { get; private set; } = new UnityEvent();


        public void SwitchIsAim()
        {
            if (!isOwned) return;
            IsAiming = !IsAiming;
        }
        public void SwitchIsAim(bool state)
        {
            if (!isOwned) return;
            IsAiming = state;
        }

        public void Aim()
        {
            if (!isOwned) return;


            if (!IsAiming)
            {
                if (_waiterLost != null) StopCoroutine(_waiterLost);
                if (_waiterFound != null) StopCoroutine(_waiterFound);
                _target = null;
                LookFree();
                return;
            }

            RaycastHit raycastHit;
            RaycastHit sphereCastHit;

            if (IsFound())
            {
                StartCoroutine(WaitToFound());
                IEnumerator WaitToFound()
                {
                    if (_target == null)
                        yield return new WaitForSeconds(FoundTime);

                    if (Physics.Raycast(_aimPoint.position, _aimPoint.forward,
                        out raycastHit, Distance, TargetAimLayer))
                    {
                        _target = raycastHit.transform;
                        LookAtTarget(raycastHit.transform.position);
                    }
                    else if (Physics.SphereCast(_aimPoint.position + Vector3.forward * AimRadius, AimRadius, _aimPoint.forward,
                        out sphereCastHit, Distance, TargetAimLayer))
                    {
                        _target = sphereCastHit.transform;
                        LookAtTarget(sphereCastHit.transform.position);
                    }

                    _waiterFound = null;
                    yield return null;
                }
            }
            else
            {
                StartCoroutine(WaitLost());
                IEnumerator WaitLost()
                {
                    if (_target != null)
                    {
                        LookAtTarget(_target.position);
                        yield return new WaitForSeconds(LostTime);
                    }

                    if (!IsFound())
                    {
                        _target = null;
                        LookFree();
                    }

                    _waiterLost = null;
                    yield return null;
                }
            }

            bool IsFound()
            {
                if (Physics.Raycast(_aimPoint.position, _aimPoint.forward,
                            out raycastHit, Distance, TargebleLayer))
                {
                    if ((~TargetAimLayer & (1 << raycastHit.collider.gameObject.layer)) == 0) return true;
                }
                else if (Physics.SphereCast(_aimPoint.position + Vector3.forward * AimRadius, AimRadius, _aimPoint.forward,
                        out sphereCastHit, Distance, TargebleLayer))
                {
                    if ((~TargetAimLayer & (1 << sphereCastHit.collider.gameObject.layer)) == 0) return true;
                }

                return false;
            }
            void LookAtTarget(Vector3 targetPosition)
            {
                Quaternion rotation = _rotateTransform.rotation;
                _rotateTransform.LookAt(targetPosition);
                Quaternion gunRotation = _rotateTransform.localRotation;
                _rotateTransform.rotation = rotation;
                _rotateTransform.localRotation = Quaternion.Lerp(_rotateTransform.localRotation, gunRotation, Speed * Time.deltaTime);

                Physics.Raycast(_rotateTransform.position, _rotateTransform.forward,
                                    out RaycastHit gunHit, Distance, TargetAimLayer);
                OnTargetFound.Invoke(_shootPoint.position, gunHit.point);
            }

            void LookFree()
            {
                _rotateTransform.localRotation = Quaternion.Lerp(_rotateTransform.localRotation, _startRotation, Speed * Time.deltaTime);
                OnTargetLost.Invoke();
            }
        }

        #endregion



        #region Network
        [SyncVar(hook = (nameof(DataSync)))]
        [SerializeField]
        private Data _data = new Data();
        private void DataSync(Data oldValue, Data newValue)
        {
            _data = newValue;
        }
        [Command(requiresAuthority = false)]
        public void CmdSyncData(Data value)
        {
            _data = value;
        }
        [System.Serializable]
        public struct Data
        {
            public float Distance;
            public float AimRadius;
            public float Speed;
            public float LostTime;
            public float FoundTime;
            public LayerMask TargetAimLayer;
            public LayerMask TargebleLayer;
        }
        #endregion
    }

    public static class AimerSerializer
    {
        public static void Write(this NetworkWriter writer, Aimer.Data item)
        {
            writer.WriteFloat(item.Distance);
            writer.WriteFloat(item.AimRadius);
            writer.WriteFloat(item.Speed);
            writer.WriteFloat(item.LostTime);
            writer.WriteFloat(item.FoundTime);
            writer.WriteLayerMask(item.TargetAimLayer);
            writer.WriteLayerMask(item.TargebleLayer);
        }

        public static Aimer.Data Read(this NetworkReader reader)
        {
            return new Aimer.Data
            {
                Distance = reader.ReadFloat(),
                AimRadius = reader.ReadFloat(),
                Speed = reader.ReadFloat(),
                LostTime = reader.ReadFloat(),
                FoundTime = reader.ReadFloat(),
                TargetAimLayer = reader.ReadLayerMask(),
                TargebleLayer = reader.ReadLayerMask(),
            };
        }
    }
}