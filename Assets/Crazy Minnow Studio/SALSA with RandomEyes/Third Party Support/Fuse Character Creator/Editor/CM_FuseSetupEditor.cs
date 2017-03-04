using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CrazyMinnow.SALSA.Fuse
{
    [CustomEditor(typeof(CM_FuseSetup))]
    public class CM_FuseSetupEditor : Editor
    {
        private CM_FuseSetup fuseSetup; // CM_FuseSetup reference

        public void OnEnable()
        {
            // Get reference
            fuseSetup = target as CM_FuseSetup;

            // Run Setup
            fuseSetup.Setup();

            // Remove setup component
            DestroyImmediate(fuseSetup);
        }
    }
}