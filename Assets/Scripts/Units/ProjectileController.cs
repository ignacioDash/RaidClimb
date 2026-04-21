using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Units
{
    public class ProjectileController : MonoBehaviour
    {
        [SerializeField] private bool randomRotation;
        [SerializeField] private float rotationSpeed = 360f;

        private Vector3 _target;
        private float _damage;
        private float _speed;
        private Vector3 _randomRotationAxis;
        private Action<float> _onHit;
        private Action<ProjectileController> _returnToPool;

        public void Launch(Vector3 target, float damage, float speed, Action<float> onHit, Action<ProjectileController> returnToPool)
        {
            _target = target;
            _damage = damage;
            _speed = speed;
            _onHit = onHit;
            _returnToPool = returnToPool;
            _randomRotationAxis = Random.onUnitSphere;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, _target, _speed * Time.deltaTime);

            if (randomRotation)
                transform.Rotate(_randomRotationAxis, rotationSpeed * Time.deltaTime, Space.World);
            else
                transform.LookAt(_target);

            if (Vector3.Distance(transform.position, _target) < 0.1f)
                Arrive();
        }

        private void Arrive()
        {
            _onHit?.Invoke(_damage);
            _onHit = null;
            gameObject.SetActive(false);
            _returnToPool?.Invoke(this);
        }
    }
}
