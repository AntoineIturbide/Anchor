using UnityEngine;
using System.Collections;

namespace Controller {
    [RequireComponent(typeof(CharacterController))]
    public class Character : MonoBehaviour {
        /** CONFIGURATION **/
        // Camera
        [System.Serializable]
        public class CameraConfiguration {
            public GameObject target;
            public Vector3 offset;
        }
        public CameraConfiguration cameraConfiguration;

        // Displacement
        [System.Serializable]
        public class DisplacementConfiguration {
            public float walkingSpeed = 1f;
            public float runingSpeed = 1f;
            public AnimationCurve walkRunTransition;
            public float jumpStrength = 5f;
        }
        public DisplacementConfiguration displacementConfiguration;

        /** Camera **/
        public class PlayerCamera {
            public Quaternion rotation;
        }
        private PlayerCamera playerCamera;

        /** PHYSIC **/
        // Displacement
        public class Physic {
            public Vector2 displacement;
        }
        private Physic physic;

        /** UNITY **/
        // Input Update
        private void InputUpdate() {
            // Retrieve player displacement input
            physic.displacement = new Vector2(
                Input.GetAxis("Horizontal"),
                Input.GetAxis("Vertical")
                );
        }

        // Physic Update
        private void FixedUpdate() {
            
        }

        // Rendering Update
        private void LateUpdate() {
            GameObject camera = cameraConfiguration.target;
            camera.transform.position = transform.position + cameraConfiguration.offset;
        }


    }

}