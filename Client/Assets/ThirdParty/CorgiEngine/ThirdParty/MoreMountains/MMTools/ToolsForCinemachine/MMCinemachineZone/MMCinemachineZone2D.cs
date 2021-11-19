using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 2D Implementation of the CinemachineZone abstract class
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class MMCinemachineZone2D : MMCinemachineZone
    {
        protected Collider2D _collider2D;
        protected Collider2D _confinerCollider2D;
        protected Rigidbody2D _confinerRigidbody2D;
        protected CompositeCollider2D _confinerCompositeCollider2D;
        protected CinemachineConfiner _cinemachineConfiner;
        protected BoxCollider2D _boxCollider2D;
        protected CircleCollider2D _circleCollider2D;
        
        /// <summary>
        /// Gets and sets up the colliders
        /// </summary>
        protected override void InitializeCollider()
        {
            _collider2D = GetComponent<Collider2D>();
            _boxCollider2D = GetComponent<BoxCollider2D>();
            _circleCollider2D = GetComponent<CircleCollider2D>();
            _collider2D.isTrigger = true;
        }

        /// <summary>
        /// Creates and sets up the camera's confiner
        /// </summary>
        protected override void SetupConfiner()
        {
            // we add a rigidbody2D to it and set it up
            _confinerRigidbody2D = _confinerGameObject.AddComponent<Rigidbody2D>();
            _confinerRigidbody2D.bodyType = RigidbodyType2D.Static;
            _confinerRigidbody2D.simulated = false;
            _confinerRigidbody2D.useAutoMass = true;
            _confinerRigidbody2D.bodyType = RigidbodyType2D.Dynamic;
            
            // we copy the collider and set it up
            
            _confinerCollider2D = MMHelpers.CopyComponent(_collider2D, _confinerGameObject);
            _confinerCollider2D.usedByComposite = true;
            _confinerGameObject.transform.localPosition = Vector3.zero;
            
            // we reset these settings, set differently initially to avoid a weird Unity warning
            _confinerRigidbody2D.bodyType = RigidbodyType2D.Static;
            _confinerRigidbody2D.useAutoMass = false;
            
            // we add a composite collider 2D and set it up
            _confinerCompositeCollider2D = _confinerGameObject.AddComponent<CompositeCollider2D>();
            _confinerCompositeCollider2D.geometryType = CompositeCollider2D.GeometryType.Polygons;
            
            // we set the composite collider as the virtual camera's confiner
            _cinemachineConfiner = VirtualCamera.gameObject.MMGetComponentAroundOrAdd<CinemachineConfiner>();
            _cinemachineConfiner.m_ConfineMode = CinemachineConfiner.Mode.Confine2D;
            _cinemachineConfiner.m_ConfineScreenEdges = true;
            _cinemachineConfiner.m_BoundingShape2D = _confinerCompositeCollider2D;
        }

        /// <summary>
        /// On enter, enables the camera and triggers the enter event
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void OnTriggerEnter2D(Collider2D collider)
        {
            if (TriggerMask.MMContains (collider.gameObject))
            {
                StartCoroutine(EnableCamera(true, 0));
                OnEnterZoneEvent.Invoke();
            }
        }

        /// <summary>
        /// On exit, disables the camera and invokes the exit event
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void OnTriggerExit2D(Collider2D collider)
        {
            if (TriggerMask.MMContains (collider.gameObject))
            {
                StartCoroutine(EnableCamera(false, 0));
                OnExitZoneEvent.Invoke();
            }
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// Draws gizmos to show the shape of the zone
        /// </summary>
        protected virtual void OnDrawGizmos()
        {
            if (!DrawGizmos)
            {
                return;
            }
            
            Gizmos.color = GizmosColor;

            if ((_boxCollider2D != null) && _boxCollider2D.enabled)
            {
                _gizmoSize.x =  _boxCollider2D.bounds.size.x ;
                _gizmoSize.y =  _boxCollider2D.bounds.size.y ;
                _gizmoSize.z = 1f;
                Gizmos.DrawCube(_boxCollider2D.bounds.center, _gizmoSize);
            }
            if (_circleCollider2D != null && _circleCollider2D.enabled)
            {
                Gizmos.DrawSphere((Vector2)this.transform.position + _circleCollider2D.offset, _circleCollider2D.radius);                
            }
        }
        #endif
    }    
}

