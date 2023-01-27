using System.Collections.Generic;
using UnityEngine;
using Inventory;
using Inventory.Entities;
using Unity.VisualScripting;

namespace Entities
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : EntityController, IDamageable
    {
        #region Serialized Fields
        
            [Header("Components")]
            [SerializeField] private SpriteRenderer handsSr;
            [SerializeField] private Animator handsAnimator;
            [SerializeField] private GameObject equippedItemObject;
        
            [Header("Movement Settings")]
            [SerializeField] private float accelerationSpeed;
            [SerializeField] private float maxMoveSpeed;
            [SerializeField] private float jumpForce;
            [SerializeField, Tooltip("How long can the jump key be held to increase jump force")] private float maxJumpForceTime;

            [Header("Stats Settings")]
            [SerializeField] private float maxHealth;
            [SerializeField] private float health;
            [SerializeField] private float maxEnergy;
            [SerializeField] private float energy;
            [SerializeField] private float energyRegenDelay;
            [SerializeField] private float energyRegenRate;
            [SerializeField] private float healthRegenDelay;
            [SerializeField] private float healthRegenRate;
            
        #endregion

        #region Unserialized Components
        
            private SpriteRenderer _sr;
            private Animator _animator;
            private Transform _itemAnchor;
            private Transform _transform;
            private SpriteRenderer _equippedSr;
            
        #endregion

        #region Interaction Variables
        
            private Interactable _closestInteractable;
            private Interactable _newClosestInteractable;
            private readonly List<Interactable> _interactablesInRange = new();
            
        #endregion

        #region Jumping Variables
        
            private bool _jumping;
            private const float JumpSafetyCooldown = 0.2f; // Used to prevent another jump as the player is jumping up
            private float _jumpCooldownTimer; // Same ^
            private float _jumpForceTimer; // Used to calculate how long the jump key can be held down to jump higher
            
        #endregion
    
        private Vector2 _inputVector;
        private Vector2 _oldLocalVelocity; // Used to fix landing momentum
        private Item _equippedItem;
        private float _energyRegenTimer, _healthRegenTimer;

        protected override void Start()
        {
            base.Start();

            Physics2D.queriesHitTriggers = false;
        
            _animator = GetComponent<Animator>();
            _sr = GetComponent<SpriteRenderer>();
            _itemAnchor = equippedItemObject.transform.parent;
            _equippedSr = equippedItemObject.GetComponent<SpriteRenderer>();
            _transform = transform;

            InventoryManager.SlotSelected += EquipItem;
            Physics2D.queriesHitTriggers = false;
        }

        private void Update()
        {
            HandleInteraction();
            HandleGroundCheck();

            if (!CanControl) return;
            
            HandleControls();
            HandleItemAiming();
            HandleStatsRegeneration();

            if (_inputVector.x == 0) return;
            _sr.flipX = _inputVector.x > 0;
            handsSr.flipX = _inputVector.x > 0;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (CanControl)
            {
                //transform.Translate(Vector3.right * (_inputVector.x * moveSpeed));
                
                // Checking for velocity.x or y doesn't work because the player can face any direction and still be moving "right" in relation to themselves
                // That's why we use a local velocity
                var localVelocity = Rigidbody.GetVector(Rigidbody.velocity);
                if ((_inputVector.x > 0 && localVelocity.x < maxMoveSpeed) ||
                    (_inputVector.x < 0 && localVelocity.x > -maxMoveSpeed))
                {
                    Rigidbody.AddForce(_transform.right * (_inputVector.x * accelerationSpeed));
                    _oldLocalVelocity = Rigidbody.GetVector(Rigidbody.velocity);
                }

                _animator.SetBool("running", _inputVector.x != 0);
                handsAnimator.SetBool("running", _inputVector.x != 0);
            }
        }

        private void LateUpdate()
        {
            // Used to fix landing issue in HandleGroundCheck function
            // _oldVelocity = Rigidbody.velocity;
        }

        private void HandleControls()
        {
            _inputVector.x = Input.GetAxis("Horizontal");

            // Jumping
            if (Input.GetKey(KeyCode.Space))
            {
                // OLD -----------
                
                // if (_jumping) return;
                //
                // Rigidbody.AddForce(_transform.up * jumpForce, ForceMode2D.Impulse);
                // _jumping = true;
                // _jumpCooldownTimer = JumpSafetyCooldown;
                
                // ---------------

                // Check if the jump key can be held to increase jump force
                if (_jumpForceTimer < maxJumpForceTime)
                {
                    _jumpForceTimer += Time.deltaTime;

                    if (!_jumping) _jumpCooldownTimer = JumpSafetyCooldown;
                    _jumping = true;
                    
                    Rigidbody.AddForce(_transform.up * (jumpForce * Time.deltaTime), ForceMode2D.Impulse);
                }
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                // Prevent adding jump force in the air if the jump key is released while jumping
                _jumpForceTimer = maxJumpForceTime;
            }
            
            // Attacking
            else if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Attack();
            }
        }

        private void HandleItemAiming()
        {        
            var mousePosition = Camera.main!.ScreenToWorldPoint(Input.mousePosition);
            var directionToMouse = (mousePosition - _transform.position).normalized;
            var cross = Vector3.Cross(-_transform.forward, directionToMouse);
            _itemAnchor.LookAt( transform.position - _transform.forward, cross);

            // Flip sprite when aiming right
            var angle = Vector3.Angle(transform.right, directionToMouse);
            _equippedSr.flipY = angle < 90;
        }

        private void HandleGroundCheck()
        {
            // This is to make sure the raycast doesn't allow another jump as the player is moving up
            if (_jumpCooldownTimer > 0)
            {
                _jumping = true;
                _jumpCooldownTimer -= Time.deltaTime;
                return;
            }
        
            // var hit = Physics2D.Raycast(_transform.position, -_transform.up, 0.6f, 1 << LayerMask.NameToLayer("World"));

            var hit = Physics2D.CircleCast(_transform.position, 0.2f, -_transform.up, 0.4f, 1 << LayerMask.NameToLayer("World"));
            
            if (!hit) return;
            
            _jumpForceTimer = 0;
            
            if (_jumping)
            {
                _jumping = false;
                
                // Set velocity when landing to keep horizontal momentum
                var tempVel = Rigidbody.GetVector(Rigidbody.velocity);
                tempVel.x = _oldLocalVelocity.x;
                Rigidbody.velocity = Rigidbody.GetRelativeVector(tempVel);
            }
        }

        private void HandleInteraction()
        {
            // Interaction
            if (_interactablesInRange.Count <= 0) return;
        
            // Check for closest interactable when moving
            if (_inputVector.magnitude > 0)
            {
                // Find closest interactable
                _newClosestInteractable = GetClosestInteractable();

                // Check if the closest interactable is the same as the previous closest one
                if (_newClosestInteractable != _closestInteractable)
                {
                    // Disable the prompt on the old one if there is one
                    if (_closestInteractable) _closestInteractable.DisablePrompt();
                    
                    _closestInteractable = _newClosestInteractable;
                    _closestInteractable.PromptInteraction();
                }
            }

            if (!Input.GetKeyDown(KeyCode.F)) return;
            
            _closestInteractable.Interact(this);
            _closestInteractable.DisablePrompt();
        }

        private void HandleStatsRegeneration()
        {
            if (_energyRegenTimer > 0)
            {
                _energyRegenTimer -= Time.deltaTime;
            }
            else
            {
                energy = Mathf.Clamp(energy + energyRegenRate * Time.deltaTime, 0, maxEnergy);
                StatsUIManager.Instance.UpdateEnergyUI(energy, maxEnergy);
            }

            if (_healthRegenTimer > 0)
            {
                _healthRegenTimer -= Time.deltaTime;
            }
            else
            {
                health = Mathf.Clamp(health + healthRegenRate * Time.deltaTime, 0, maxEnergy);
                StatsUIManager.Instance.UpdateHealthUI(health, maxHealth);
            }
        }

        private Interactable GetClosestInteractable()
        {
            var closest = _interactablesInRange[0];

            foreach (var interactable in _interactablesInRange)
            {
                var distToCurrent = (closest.transform.position - _transform.position).magnitude;
                var distToNew = (interactable.transform.position - _transform.position).magnitude;

                if (distToNew < distToCurrent)
                {
                    closest = interactable;
                }
            }

            return closest;
        }

        public override void ToggleSpriteRenderer(bool state)
        {
            _sr.enabled = state;
            handsSr.enabled = state;
        }

        private void EquipItem(Item item)
        {
            _equippedItem = item;
            equippedItemObject.SetActive(item != null);

            if (item != null)
            {
                equippedItemObject.GetComponent<SpriteRenderer>().sprite = item.itemSo.sprite;
            }
        
            //equippedItem.transform.localPosition = item.itemSo.defaultHandPosition;
            //equippedItem.transform.localEulerAngles = item.itemSo.defaultHandRotation;
        }

        private void Attack()
        {
            if (_equippedItem?.itemSo is not WeaponSo weaponSo) return;

            if (energy > weaponSo.energyCost)
            {
                // Attack
                _equippedItem.LogicScript.Attack(equippedItemObject, _equippedItem, _equippedSr.flipY);

                // Update energy
                energy = Mathf.Clamp(energy - weaponSo.energyCost, 0, maxEnergy);
                _energyRegenTimer = energyRegenDelay;
                StatsUIManager.Instance.UpdateEnergyUI(energy, maxEnergy);
                
                //TODO: Recoil
            }
            else
            {
                Debug.Log("No energy!");
            }
        }

        public void TakeDamage(float amount)
        {
            health = Mathf.Clamp(health - amount, 0, maxHealth);
            _healthRegenTimer = healthRegenDelay;
            StatsUIManager.Instance.UpdateHealthUI(health, maxHealth);
            
            if (health <= 0) Death();
            
            //TODO: damage numbers
            
            Debug.Log($"Took {amount} damage!");
        }

        public void Knockback(Vector3 damageSourcePosition, float amount)
        {
            if (amount == 0) return;
            
            Rigidbody.velocity = Vector2.zero;
            var knockbackDirection = (transform.position - damageSourcePosition).normalized;
            Rigidbody.AddForce(knockbackDirection * amount, ForceMode2D.Impulse);
        }

        public void Death()
        {
            Debug.Log("Death.");
        }
    
        protected override void OnTriggerEnter2D(Collider2D col)
        {
            base.OnTriggerEnter2D(col);
        
            if (col.transform.root.TryGetComponent<Interactable>(out var interactable))
            {
                _interactablesInRange.Add(interactable);
            }
            else if (col.transform.root.TryGetComponent<ItemEntity>(out var item))
            {
                if (!InventoryManager.AddItem(item.item)) return;
            
                Destroy(col.transform.parent.gameObject);
            }
        }

        protected override void OnTriggerExit2D(Collider2D other)
        {
            base.OnTriggerEnter2D(other);
        
            if (other.transform.root.TryGetComponent<Interactable>(out var interactable))
            {
                interactable.DisablePrompt();

                if (interactable == _closestInteractable) _closestInteractable = null;
            
                _interactablesInRange.Remove(interactable);
            }
        }
    }
}