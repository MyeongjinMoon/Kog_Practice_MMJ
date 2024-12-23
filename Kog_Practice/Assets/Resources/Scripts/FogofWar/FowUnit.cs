using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Myeongjin
{
    public class FowUnit : MonoBehaviour
    {
        public float sightRange = 5;

        public float sightHeight = 0.5f;

        void OnEnable() => FowManager.AddUnit(this);

        private void OnDisable() => FowManager.RemoveUnit(this);
        private void OnDestroy() => FowManager.RemoveUnit(this);
    }
}