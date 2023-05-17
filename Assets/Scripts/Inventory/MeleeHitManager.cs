// the point of this script is to register collisions between a melee weapon and an enemy,
// and then deal damage to the enemy. This should get weapon statistics from the player controller
// when the player equips one.

using System;
using Entities;
using Inventory.Item_Types;
using UnityEngine;

namespace Inventory
{
    public class MeleeHitManager : MonoBehaviour
    {
        [SerializeField] private GameObject equippedItemObject;
        
        private MeleeSo _meleeSo;
        private EdgeCollider2D _edgeCollider;

        private void Start()
        {
            var itemAnimationManager = transform.parent.GetComponent<ItemAnimationManager>();
            itemAnimationManager.SwingStarted += () => SetCollision(true);
            itemAnimationManager.SwingCompleted += () => SetCollision(false);

            // TODO: Subscribe to item equip event in InventoryManager
        }

        private void OnItemEquipped(Item item)
        {
            if (item.itemSo is MeleeSo meleeSo)
            {
                SetWeaponStats(meleeSo.colliderPoints);
                SetCollision(false);
            }
            else
            {
                SetWeaponStats(null);
            }
        }

        private void SetWeaponStats(Vector2[] colliderPoints)
        {
            _edgeCollider ??= equippedItemObject.GetComponent<EdgeCollider2D>();
            _edgeCollider.points = colliderPoints;
        }

        private void SetCollision(bool state)
        {
            _edgeCollider.enabled = state;
        }
        
        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.gameObject.CompareTag("Enemy"))
            {
                var enemy = col.gameObject.GetComponent<EnemyEntity>();
                enemy.TakeDamage(_meleeSo.damage);
                enemy.Knockback(transform.position, _meleeSo.knockback);
            }
        }
    }
}
