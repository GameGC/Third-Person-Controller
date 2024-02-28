using System;
using GameGC.SurfaceSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace MTPS.Shooter.WeaponsSystem.ShootableWeapon
{
    public class DefaultRaycastBullet : MonoBehaviour , IDamageSender
    {
        private new Transform transform;
        private Vector3 _flyDestination;

        public LayerMask impactMask;
        [field: SerializeField] public float damage { get; private set; } = 10;

        [field: SerializeField, FormerlySerializedAs("HitType")]
        public SurfaceHitType HitType { get; } = SurfaceHitType.Bullet;
    
        public int speed = 100;
        public int distance = 1000;
    
        public SurfaceEffect defaultImpactEffect;

        // Start is called before the first frame update

        private void Reset()
        {
            impactMask = LayerMask.GetMask("Default","Character","Char_Collision");
        }

        private void Awake() => transform = base.transform;

        private void Start()
        {
            var ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray,out var hit, distance, impactMask, QueryTriggerInteraction.Ignore))
            {
                var isCharacter = hit.collider.gameObject.layer == LayerMask.NameToLayer("Character") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Char_Collision");
            
                // Debug.DrawLine(transform.position,hit.point,Color.red,100);
                _flyDestination = hit.point;
                try
                {
                    SurfaceSystem.instance.OnSurfaceHit(hit,(int) HitType, defaultImpactEffect);
                }
                catch (Exception)
                {
                    // ignored
                }

                if (isCharacter)
                {
                    var gameObject = hit.collider.gameObject;
                    bool isCharPart = gameObject.layer == LayerMask.NameToLayer("Char_Collision");
                    if (!isCharPart)
                    {
                        ray.origin = hit.point;
                        if (Physics.Raycast(ray, out var newHit, distance,
                                LayerMask.GetMask("Char_Collision"), QueryTriggerInteraction.Ignore))
                        {
                            hit = newHit;
                        }
                    }

                    gameObject.GetComponentInChildren<HealthComponent>().OnHit(hit, this);
                }
            }
            else
            {
                _flyDestination = transform.position + transform.forward * distance;
            }
        }

        public void Init(Vector3 flyDestination)
        {
            this._flyDestination = flyDestination;
        }

        // Update is called once per frame
        private void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, _flyDestination, Time.deltaTime * speed);
            if(Vector3.Distance(transform.position,_flyDestination)<0.1f)
                Destroy(gameObject);
        }

    }

    public interface IDamageSender
    {
        public float damage { get; }
        public SurfaceHitType HitType { get; }
    }
}