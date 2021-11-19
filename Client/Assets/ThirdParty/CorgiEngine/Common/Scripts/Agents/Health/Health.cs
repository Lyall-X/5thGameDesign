using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.CorgiEngine
{
    /// <summary>
    /// This class manages the health of an object, pilots its potential health bar, handles what happens when it takes damage,
    /// and what happens when it dies.
    /// </summary>
    [AddComponentMenu("Corgi Engine/Character/Core/Health")]
    public class Health : MonoBehaviour
    {
        /// the current health of the character
        [MMReadOnly] [Tooltip("the current health of the character")]
        public int CurrentHealth;

        /// If this is true, this object can't take damage at the moment
        [MMReadOnly] [Tooltip("If this is true, this object can't take damage at the moment")]
        public bool TemporaryInvulnerable = false;

        [Header("Health")]
        [MMInformation(
            "Add this component to an object and it'll have health, will be able to get damaged and potentially die.",
            MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        /// the initial amount of health of the object
        [Tooltip("the initial amount of health of the object")]
        public int InitialHealth = 10;

        /// the maximum amount of health of the object
        [Tooltip("the maximum amount of health of the object")]
        public int MaximumHealth = 10;

        /// if this is true, this object can't take damage
        [Tooltip("if this is true, this object can't take damage")]
        public bool Invulnerable = false;

        [Header("Damage")]
        [MMInformation(
            "Here you can specify an effect and a sound FX to instantiate when the object gets damaged, and also how long the object should flicker when hit (only works for sprites).",
            MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        
        /// whether or not this Health object can be damaged, you can play with this on top of Invulnerable, which will be turned on/off temporarily for temporary invulnerability. ImmuneToDamage is more of a permanent solution. 
        [Tooltip("whether or not this Health object can be damaged, you can play with this on top of Invulnerable, which will be turned on/off temporarily for temporary invulnerability. ImmuneToDamage is more of a permanent solution.")]
        public bool ImmuneToDamage = false;
        
        /// the MMFeedbacks to play when the character gets hit
        [Tooltip("the MMFeedbacks to play when the character gets hit")]
        public MMFeedbacks DamageFeedbacks;
        
        /// if this is true, the damage value will be passed to the MMFeedbacks as its Intensity parameter, letting you trigger more intense feedbacks as damage increases
        [Tooltip("if this is true, the damage value will be passed to the MMFeedbacks as its Intensity parameter, letting you trigger more intense feedbacks as damage increases")]
        public bool FeedbackIsProportionalToDamage = false;

        /// should the sprite (if there's one) flicker when getting damage ?
        [Tooltip("should the sprite (if there's one) flicker when getting damage ?")]
        public bool FlickerSpriteOnHit = true;

        /// the color the sprite should flicker to
        [Tooltip("the color the sprite should flicker to")] [MMCondition("FlickerSpriteOnHit", true)]
        public Color FlickerColor = new Color32(255, 20, 20, 255);

        /// whether or not this object can get knockback
        [Tooltip("whether or not this object can get knockback")]
        public bool ImmuneToKnockback = false;

        [Header("Death")]
        [MMInformation(
            "Here you can set an effect to instantiate when the object dies, a force to apply to it (corgi controller required), how many points to add to the game score, and where the character should respawn (for non-player characters only).",
            MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        /// the MMFeedbacks to play when the character dies
        [Tooltip("the MMFeedbacks to play when the character dies")]
        public MMFeedbacks DeathFeedbacks;

        /// if this is not true, the object will remain there after its death
        [Tooltip("if this is not true, the object will remain there after its death")]
        public bool DestroyOnDeath = true;

        /// the time (in seconds) before the character is destroyed or disabled
        [Tooltip("the time (in seconds) before the character is destroyed or disabled")]
        public float DelayBeforeDestruction = 0f;

        /// if this is true, collisions will be turned off when the character dies
        [Tooltip("if this is true, collisions will be turned off when the character dies")]
        public bool CollisionsOffOnDeath = true;

        /// if this is true, gravity will be turned off on death
        [Tooltip("if this is true, gravity will be turned off on death")]
        public bool GravityOffOnDeath = false;

        /// the points the player gets when the object's health reaches zero
        [Tooltip("the points the player gets when the object's health reaches zero")]
        public int PointsWhenDestroyed;

        /// if this is set to false, the character will respawn at the location of its death, otherwise it'll be moved to its initial position (when the scene started)
        [Tooltip(
            "if this is set to false, the character will respawn at the location of its death, otherwise it'll be moved to its initial position (when the scene started)")]
        public bool RespawnAtInitialLocation = false;

        [Header("Death Forces")]
        /// whether or not to apply a force on death
        [Tooltip("whether or not to apply a force on death")]
        public bool ApplyDeathForce = true;

        /// the force applied when the character dies
        [Tooltip("the force applied when the character dies")]
        public Vector2 DeathForce = new Vector2(0, 10);

        /// whether or not the controller's forces should be set to 0 on death
        [Tooltip("whether or not the controller's forces should be set to 0 on death")]
        public bool ResetForcesOnDeath = false;
        
        /// if this is true, color will be reset on revive
        [Tooltip("if this is true, color will be reset on revive")]
        public bool ResetColorOnRevive = true;
        /// the name of the property on your renderer's shader that defines its color 
        [Tooltip("the name of the property on your renderer's shader that defines its color")]
        [MMCondition("ResetColorOnRevive", true)]
        public string ColorMaterialPropertyName = "_Color";
        /// if this is true, this component will use material property blocks instead of working on an instance of the material.
        [Tooltip("if this is true, this component will use material property blocks instead of working on an instance of the material.")] 
        public bool UseMaterialPropertyBlocks = false;

        public int LastDamage { get; set; }
        public Vector3 LastDamageDirection { get; set; }

        // respawn
        public delegate void OnHitDelegate();

        public OnHitDelegate OnHit;

        public delegate void OnHitZeroDelegate();

        public OnHitZeroDelegate OnHitZero;

        public delegate void OnReviveDelegate();

        public OnReviveDelegate OnRevive;

        public delegate void OnDeathDelegate();

        public OnDeathDelegate OnDeath;

        protected Vector3 _initialPosition;
        protected Color _initialColor;
        protected Renderer _renderer;
        protected Character _character;
        protected CorgiController _controller;
        protected MMHealthBar _healthBar;
        protected Collider2D _collider2D;
        protected bool _initialized = false;
        protected AutoRespawn _autoRespawn;
        protected Animator _animator;
        protected CharacterPersistence _characterPersistence = null;
        protected MaterialPropertyBlock _propertyBlock;
        protected bool _hasColorProperty = false;

        /// <summary>
        /// On Start, we initialize our health
        /// </summary>
        protected virtual void Start()
        {
            Initialization();
            InitializeSpriteColor();
        }

        /// <summary>
        /// Grabs useful components, enables damage and gets the inital color
        /// </summary>
        protected virtual void Initialization()
        {
            _character = this.gameObject.GetComponent<Character>();
            _characterPersistence = this.gameObject.GetComponent<CharacterPersistence>();

            if (this.gameObject.MMGetComponentNoAlloc<SpriteRenderer>() != null)
            {
                _renderer = this.gameObject.GetComponent<SpriteRenderer>();
            }

            if (_character != null)
            {
                if (_character.CharacterModel != null)
                {
                    if (_character.CharacterModel.GetComponentInChildren<Renderer>() != null)
                    {
                        _renderer = _character.CharacterModel.GetComponentInChildren<Renderer>();
                    }
                }
            }

            // we grab our animator
            if (_character != null)
            {
                if (_character.CharacterAnimator != null)
                {
                    _animator = _character.CharacterAnimator;
                }
                else
                {
                    _animator = this.gameObject.GetComponent<Animator>();
                }
            }
            else
            {
                _animator = this.gameObject.GetComponent<Animator>();
            }

            if (_animator != null)
            {
                _animator.logWarnings = false;
            }

            _autoRespawn = this.gameObject.GetComponent<AutoRespawn>();
            _controller = this.gameObject.GetComponent<CorgiController>();
            _healthBar = this.gameObject.GetComponent<MMHealthBar>();
            _collider2D = this.gameObject.GetComponent<Collider2D>();
            
            StoreInitialPosition();    
            _initialized = true;
            CurrentHealth = InitialHealth;
            DamageEnabled();
            UpdateHealthBar(false);
        }

        public virtual void StoreInitialPosition()
        {
            _initialPosition = transform.position;
        }

        /// <summary>
        /// Stores the inital color of the Character's sprite.
        /// </summary>
        protected virtual void InitializeSpriteColor()
        {
            if (!FlickerSpriteOnHit)
            {
                return;
            }

            if (_renderer != null)
            {
                if (UseMaterialPropertyBlocks)
                {
                    if (_renderer.sharedMaterial.HasProperty(ColorMaterialPropertyName))
                    {
                        _hasColorProperty = true; 
                        _initialColor = _renderer.sharedMaterial.GetColor(ColorMaterialPropertyName);
                    }
                }
                else
                {
                    if (_renderer.material.HasProperty(ColorMaterialPropertyName))
                    {
                        _hasColorProperty = true;
                        _initialColor = _renderer.material.GetColor(ColorMaterialPropertyName);
                    } 
                }
            }
        }

        /// <summary>
        /// Restores the original sprite color
        /// </summary>
        protected virtual void ResetSpriteColor()
        {
            if (_renderer != null)
            {
                if (UseMaterialPropertyBlocks)
                {
                    _renderer.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetColor(ColorMaterialPropertyName, _initialColor);
                    _renderer.SetPropertyBlock(_propertyBlock);    
                }
                else
                {
                    _renderer.material.SetColor(ColorMaterialPropertyName, _initialColor);
                }
            }
        }

        /// <summary>
        /// Called when the object takes damage
        /// </summary>
        /// <param name="damage">The amount of health points that will get lost.</param>
        /// <param name="instigator">The object that caused the damage.</param>
        /// <param name="flickerDuration">The time (in seconds) the object should flicker after taking the damage.</param>
        /// <param name="invincibilityDuration">The duration of the short invincibility following the hit.</param>
        public virtual void Damage(int damage, GameObject instigator, float flickerDuration,
            float invincibilityDuration, Vector3 damageDirection)
        {
            if (damage <= 0)
            {
                OnHitZero?.Invoke();
                return;
            }

            // if the object is invulnerable, we do nothing and exit
            if (TemporaryInvulnerable || Invulnerable || ImmuneToDamage)
            {
                OnHitZero?.Invoke();
                return;
            }
            
            if (!this.enabled)
            {
                return;
            }

            // if we're already below zero, we do nothing and exit
            if ((CurrentHealth <= 0) && (InitialHealth != 0))
            {
                return;
            }

            // we decrease the character's health by the damage
            float previousHealth = CurrentHealth;
            CurrentHealth -= damage;

            LastDamage = damage;
            LastDamageDirection = damageDirection;
            OnHit?.Invoke();

            if (CurrentHealth < 0)
            {
                CurrentHealth = 0;
            }

            // we prevent the character from colliding with Projectiles, Player and Enemies
            if (invincibilityDuration > 0)
            {
                DamageDisabled();
                StartCoroutine(DamageEnabled(invincibilityDuration));
            }

            // we trigger a damage taken event
            MMDamageTakenEvent.Trigger(_character, instigator, CurrentHealth, damage, previousHealth);

            if (_animator != null)
            {
                _animator.SetTrigger("Damage");
            }

            // we play the damage feedback
            if (FeedbackIsProportionalToDamage)
            {
                DamageFeedbacks?.PlayFeedbacks(this.transform.position, damage);    
            }
            else
            {
                DamageFeedbacks?.PlayFeedbacks(this.transform.position);
            }

            if (FlickerSpriteOnHit)
            {
                // We make the character's sprite flicker
                if (_renderer != null)
                {
                    StartCoroutine(MMImage.Flicker(_renderer, _initialColor, FlickerColor, 0.05f, flickerDuration));
                }
            }

            // we update the health bar
            UpdateHealthBar(true);

            // if health has reached zero
            if (CurrentHealth <= 0)
            {
                // we set its health to zero (useful for the healthbar)
                CurrentHealth = 0;
                if (_character != null)
                {
                    if (_character.CharacterType == Character.CharacterTypes.Player)
                    {
                        LevelManager.Instance.KillPlayer(_character);
                        return;
                    }
                }

                Kill();
            }
        }

        /// <summary>
        /// Kills the character, instantiates death effects, handles points, etc
        /// </summary>
        public virtual void Kill()
        {
            if (ImmuneToDamage)
            {
                return;
            }
            
            // we prevent further damage
            DamageDisabled();

            // instantiates the destroy effect
            DeathFeedbacks?.PlayFeedbacks();

            // Adds points if needed.
            if (PointsWhenDestroyed != 0)
            {
                // we send a new points event for the GameManager to catch (and other classes that may listen to it too)
                CorgiEnginePointsEvent.Trigger(PointsMethods.Add, PointsWhenDestroyed);
            }

            if (_animator != null)
            {
                _animator.SetTrigger("Death");
            }

            if (OnDeath != null)
            {
                OnDeath();
            }

            // if we have a controller, removes collisions, restores parameters for a potential respawn, and applies a death force
            if (_controller != null)
            {
                // we make it ignore the collisions from now on
                if (CollisionsOffOnDeath)
                {
                    _controller.CollisionsOff();
                    if (_collider2D != null)
                    {
                        _collider2D.enabled = false;
                    }
                }

                // we reset our parameters
                _controller.ResetParameters();

                if (GravityOffOnDeath)
                {
                    _controller.GravityActive(false);
                }

                // we reset our controller's forces on death if needed
                if (ResetForcesOnDeath)
                {
                    _controller.SetForce(Vector2.zero);
                }

                // we apply our death force
                if (ApplyDeathForce)
                {
                    _controller.GravityActive(true);
                    _controller.SetForce(DeathForce);
                }
            }


            // if we have a character, we want to change its state
            if (_character != null)
            {
                // we set its dead state to true
                _character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Dead);
                _character.Reset();

                // if this is a player, we quit here
                if (_character.CharacterType == Character.CharacterTypes.Player)
                {
                    return;
                }
            }

            if (DelayBeforeDestruction > 0f)
            {
                Invoke("DestroyObject", DelayBeforeDestruction);
            }
            else
            {
                // finally we destroy the object
                DestroyObject();
            }
        }

        /// <summary>
        /// Revive this object.
        /// </summary>
        public virtual void Revive()
        {
            if (!_initialized)
            {
                return;
            }

            if (_characterPersistence != null)
            {
                if (_characterPersistence.Initialized)
                {
                    return;
                }
            }

            if (_collider2D != null)
            {
                _collider2D.enabled = true;
            }

            if (_controller != null)
            {
                _controller.CollisionsOn();
                _controller.GravityActive(true);
                _controller.SetForce(Vector2.zero);
                _controller.ResetParameters();
            }

            if (_character != null)
            {
                _character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
            }

            if (RespawnAtInitialLocation)
            {
                transform.position = _initialPosition;
            }

            Initialization();
            if (FlickerSpriteOnHit && ResetColorOnRevive)
            {
                ResetSpriteColor();
            }

            UpdateHealthBar(false);
            if (OnRevive != null)
            {
                OnRevive.Invoke();
            }
        }

        /// <summary>
        /// Destroys the object, or tries to, depending on the character's settings
        /// </summary>
        protected virtual void DestroyObject()
        {
            if (!DestroyOnDeath)
            {
                return;
            }

            if (_autoRespawn == null)
            {
                // object is turned inactive to be able to reinstate it at respawn
                gameObject.SetActive(false);
            }
            else
            {
                _autoRespawn.Kill();
            }
        }

        /// <summary>
        /// Called when the character gets health (from a stimpack for example)
        /// </summary>
        /// <param name="health">The health the character gets.</param>
        /// <param name="instigator">The thing that gives the character health.</param>
        public virtual void GetHealth(int health, GameObject instigator)
        {
            // this function adds health to the character's Health and prevents it to go above MaxHealth.
            CurrentHealth = Mathf.Min(CurrentHealth + health, MaximumHealth);
            UpdateHealthBar(true);
        }

        /// <summary>
        /// Sets the health of the character to the one specified in parameters
        /// </summary>
        /// <param name="newHealth"></param>
        /// <param name="instigator"></param>
        public virtual void SetHealth(int newHealth, GameObject instigator)
        {
            CurrentHealth = Mathf.Min(newHealth, MaximumHealth);
            UpdateHealthBar(false);
        }

        /// <summary>
        /// Resets the character's health to its max value
        /// </summary>
        public virtual void ResetHealthToMaxHealth()
        {
            CurrentHealth = MaximumHealth;
            UpdateHealthBar(false);
        }

        /// <summary>
        /// Updates the character's health bar progress.
        /// </summary>
        public virtual void UpdateHealthBar(bool show)
        {
            if (_healthBar != null)
            {
                _healthBar.UpdateBar(CurrentHealth, 0f, MaximumHealth, show);
            }

            if (_character != null)
            {
                if (_character.CharacterType == Character.CharacterTypes.Player)
                {
                    // We update the health bar
                    if (GUIManager.Instance != null)
                    {
                        GUIManager.Instance.UpdateHealthBar(CurrentHealth, 0f, MaximumHealth, _character.PlayerID);
                    }
                }
            }
        }

        /// <summary>
        /// Prevents the character from taking any damage
        /// </summary>
        public virtual void DamageDisabled()
        {
            TemporaryInvulnerable = true;
        }

        /// <summary>
        /// Allows the character to take damage
        /// </summary>
        public virtual void DamageEnabled()
        {
            TemporaryInvulnerable = false;
        }

        /// <summary>
        /// makes the character able to take damage again after the specified delay
        /// </summary>
        /// <returns>The layer collision.</returns>
        public virtual IEnumerator DamageEnabled(float delay)
        {
            yield return new WaitForSeconds(delay);
            TemporaryInvulnerable = false;
        }

        /// <summary>
        /// When the object is enabled (on respawn for example), we restore its initial health levels
        /// </summary>
        protected virtual void OnEnable()
        {
            if ((_characterPersistence != null) && (_characterPersistence.Initialized))
            {
                UpdateHealthBar(false);
                return;
            }

            CurrentHealth = InitialHealth;
            DamageEnabled();
            UpdateHealthBar(false);
        }

        /// <summary>
        /// Cancels all running invokes on disable
        /// </summary>
        protected virtual void OnDisable()
        {
            CancelInvoke();
        }
    }
}