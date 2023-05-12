using System.Collections.Generic;
using Entities;
using ProcGen;
using UnityEngine;

namespace VFX
{
    public class ParallaxManager : MonoBehaviour
    {
        [SerializeField] private float[] layerParallaxSpeeds = { 0f, 0f, 0f, 0f };
        
        private Transform[] _layerParents;
        private List<KeyValuePair<GameObject, PlanetDecorator.DecorOptions>> _updatingDecorObjects;
        private Planet _currentPlanet;
        private PlayerController _player;
        private float _oldZ;

        private void Start()
        {
            _player = PlayerController.instance;
            _player.OnEnteredPlanet += OnPlanetEntered;
        }

        // Update is called once per frame
        private void Update()
        {
#region Rotate Layers
            var z = _player.transform.eulerAngles.z;
            var diff = z - _oldZ;
            _oldZ = z;
            
            switch (diff)
            {
                case > 350:
                    diff -= 360;
                    break;
                case < -350:
                    diff += 360;
                    break;
            }

            // Ignore first parent since it's the planet itself
            for (var i = 1; i < _layerParents.Length; i++)
            {
                var pTr = _layerParents[i];
                pTr.Rotate(Vector3.forward, diff * layerParallaxSpeeds[i - 1]);
            }
#endregion

#region Move Updating Decor

            for (var i = 0; i < _updatingDecorObjects.Count; i++)
            {
                var options = _updatingDecorObjects[i].Value;
                var decor = _updatingDecorObjects[i].Key;

                if (options.move)
                {
                    var decPos = decor.transform.position;
                    var dirToPlanet = (_currentPlanet.transform.position - decPos).normalized;
                    decor.transform.LookAt(decPos + Vector3.forward, -dirToPlanet);
                    
                    // TODO: Random speed? Would be cool for birds but could fuck up other shit
                    decor.transform.Translate(Vector3.right * (Time.deltaTime * 0.7f));
                }

                if (options.animate)
                {
                    // TODO: optimize to not use GetComponent and to not get array item every frame
                    var sr = decor.GetComponent<SpriteRenderer>();
                    var num = (int)(Time.time * 2f % 2);
                    sr.sprite = options.spritePool[num];
                    sr.flipX = true;
                }
            }

#endregion    
            
        }

        private void OnPlanetEntered(Planet planet)
        {
            _currentPlanet = planet;
            (_layerParents, _updatingDecorObjects) = planet.GetComponent<PlanetDecorator>().GetDecorData();
        }
    }
}