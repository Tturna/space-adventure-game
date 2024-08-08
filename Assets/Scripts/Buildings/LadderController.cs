using System;
using Entities;
using UnityEngine;

namespace Buildings
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class LadderController : MonoBehaviour
    {
        [SerializeField] private float climbSpeed;
        [SerializeField, Tooltip("Colliders to disable as the player is climbing.")] private Collider2D[] hatchColliders;
        
        private PlayerController playerController;
        private Transform playerTransform;
        private BoxCollider2D ladderTrigger;
        private bool isPlayerInRange;
        private bool isPlayerOnLadder;
        private float failSafeTimer;
        private const float FailSafeTime = 0.5f;
        
        private void Start()
        {
            playerController = PlayerController.instance;
            playerTransform = playerController.transform;
            ladderTrigger = GetComponent<BoxCollider2D>();
            ladderTrigger.isTrigger = true;
        }

        private void TogglePlayerOnLadder(bool state)
        {
            playerController.ToggleControl(!state);
            playerController.TogglePhysics(!state);

            if (state)
            {
                playerController.ResetVelocity(true, true, false);
            }

            foreach (var hatchCollider in hatchColliders)
            {
                hatchCollider.enabled = !state;
            }
        }

        private void Update()
        {
            if (!isPlayerInRange) return;
            
            failSafeTimer += Time.deltaTime;

            if (failSafeTimer >= FailSafeTime)
            {
                failSafeTimer = 0f;
                isPlayerInRange = false;
                isPlayerOnLadder = false;
                TogglePlayerOnLadder(false);
            }

            var verticalDirection = Input.GetAxis("Vertical");
            var horizontalDirection = Input.GetAxis("Horizontal");
            
            if (!isPlayerOnLadder)
            {
                if (verticalDirection != 0f)
                {
                    TogglePlayerOnLadder(true);
                    isPlayerOnLadder = true;
                }
                else
                {
                    return;
                }
            }
            
            if (horizontalDirection != 0f)
            {
                playerTransform.Translate(Vector3.right * (horizontalDirection * climbSpeed * Time.deltaTime), Space.Self);
            }

            if (verticalDirection != 0f)
            {
                playerTransform.Translate(Vector3.up * (verticalDirection * climbSpeed * Time.deltaTime), Space.Self);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                isPlayerInRange = false;
                isPlayerOnLadder = false;
                TogglePlayerOnLadder(false);
            }
        }

        // repeatedly make sure the player is on the ladder and use a
        // fail-safe timer to prevent the player from getting stuck if
        // OnTriggerExit2D fails for some reason.
        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.transform.root.CompareTag("Player"))
            {
                isPlayerInRange = true;
                failSafeTimer = 0f;
            }
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.transform.root.CompareTag("Player"))
            {
                isPlayerInRange = false;
                isPlayerOnLadder = false;
                TogglePlayerOnLadder(false);
            }
        }
    }
}
