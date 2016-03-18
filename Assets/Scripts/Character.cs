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
            public float minimumY = -60F;
            public float maximumY = 60F;
            public float sensitivityX = 15F;
            public float sensitivityY = 15F;
        }
        public CameraConfiguration cameraConfiguration;

        // Displacement
        [System.Serializable]
        public class DisplacementConfiguration {
            public float walkingSpeed = 1f;
            public float runingSpeed = 1f;
            public AnimationCurve walkRunTransitionCurve;
            public float walkRunTransitionDelay;
            public float walkRunStopDelay = 1f;
            public float jumpStrength = 5f;
        }
        public DisplacementConfiguration displacementConfiguration;

        /** CAMERA **/
        public class PlayerCamera {
            public float rotationY = 0F;
        }
        private PlayerCamera playerCamera = new PlayerCamera();

        /** PHYSIC **/
        // Displacement
        //[System.Serializable]
        public class Physic {
            // i = Input
            // a = Acceleration
            // v = Velocity
            // p = positon
            // s = slider
            public CharacterController characterController;
            public Vector3 r_rotation;  // 
            public Vector2 i_WR;        // Walk/Run input
            public bool i_WRState;      // Walk/Run state | false = walking | true = running
            public float s_WRSlider;    // Walk/Run slider | 0 = walking | 1 = running
            public Vector2 v_WR;        // Walk/Run velocity
            public Vector3 a_gravity = new Vector3(0, -9.81f, 0);
            public Vector3 v_gravity = Vector3.zero;
            public Vector3 a_jump = new Vector3(0, 9f, 0);
            public Vector3 v_jump = Vector3.zero;
        }
        public Physic physic = new Physic();

        /** UNITY **/   
        // Constructor
        public void Awake() {
            // Debug
            ToogleCursor();

            // Retriece character controller
            physic.characterController = gameObject.GetComponent<CharacterController>();
        }

        // Input Update
        private void Update() { InputUpdate(); }
        private void InputUpdate() {
            /** DEBUG **/
            if(Input.GetKeyDown(KeyCode.Escape)) {
                ToogleCursor();
            }

            /** Retrieve player walk/run inputs **/
            // Direction
            physic.i_WR = new Vector2(
                Input.GetAxis("Horizontal"),
                Input.GetAxis("Vertical")
                );
            physic.i_WR = Vector2.ClampMagnitude(physic.i_WR, 1f);
            // State
            physic.i_WRState = Input.GetKey(KeyCode.LeftShift);

            /** Retrieve jump input **/
            if (Input.GetKeyDown(KeyCode.Space)) {
                physic.v_jump = transform.rotation * physic.a_jump;
                // Reset gravity
                physic.v_gravity = Vector3.MoveTowards(physic.v_gravity, Vector3.zero, Vector3.Project(physic.v_gravity, physic.v_jump).magnitude);
            }
            if (Input.GetKey(KeyCode.Space)) {
                physic.v_jump += transform.rotation * physic.a_jump * Time.deltaTime;
            }

            /** Retrieve player camera inputs **/
            GameObject camera = cameraConfiguration.target;
            
            float rotationX = transform.localEulerAngles.y - physic.r_rotation.y + Input.GetAxis("Mouse X") * cameraConfiguration.sensitivityX;

            playerCamera.rotationY += Input.GetAxis("Mouse Y") * cameraConfiguration.sensitivityY;
            playerCamera.rotationY = Mathf.Clamp(playerCamera.rotationY, cameraConfiguration.minimumY, cameraConfiguration.maximumY);

            transform.localEulerAngles = physic.r_rotation + new Vector3(0, rotationX, 0);
        }

        // Physic Update
        private void FixedUpdate() {
            /** STATES **/
            // Calculate walk/run related slider
            if (displacementConfiguration.walkRunTransitionDelay > 0) {
                physic.s_WRSlider = physic.i_WRState ?
                    Mathf.MoveTowards(physic.s_WRSlider, 1f, Time.fixedDeltaTime * (1f / displacementConfiguration.walkRunTransitionDelay)) :
                    Mathf.MoveTowards(physic.s_WRSlider, 0f, Time.fixedDeltaTime * (1f / displacementConfiguration.walkRunTransitionDelay));
            } else {
                physic.s_WRSlider = physic.i_WRState ?
                    1f:
                    0f;
            }



            /** ACCELERATIONS **/
            // Calculate walk/run acceleration
            float s_currentWRSpeed = Mathf.Lerp(
                    displacementConfiguration.walkingSpeed,
                    displacementConfiguration.runingSpeed,
                    displacementConfiguration.walkRunTransitionCurve.Evaluate(Mathf.Clamp01(physic.s_WRSlider))
                    );
            Vector2 a_WR = physic.i_WR * s_currentWRSpeed;



            /** VELOCITIES **/
            // Update walk/run velocity
            //physic.v_WR += a_WR * Time.fixedDeltaTime;
            physic.v_WR = a_WR;
            // Clamp walk/run velocity
            physic.v_WR = Vector2.ClampMagnitude(physic.v_WR, s_currentWRSpeed);
            // Correct walk/run velocity
            Vector2 v_correctedWR = Vector2.ClampMagnitude(physic.v_WR, s_currentWRSpeed * Mathf.Clamp01(physic.i_WR.magnitude));
            physic.v_WR = displacementConfiguration.walkRunStopDelay > 0 ?
                // If delayed
                Vector2.MoveTowards(
                    physic.v_WR,
                    v_correctedWR,
                    Time.fixedDeltaTime * (1f / displacementConfiguration.walkRunStopDelay) * s_currentWRSpeed) :
                // If instant
                v_correctedWR;

            // Update jump
            physic.v_jump = Vector3.MoveTowards(physic.v_jump, Vector3.zero, Vector3.Project(physic.v_gravity, physic.v_jump).magnitude * Time.fixedDeltaTime);

            // Update gravity
            physic.v_gravity += physic.a_gravity * Time.fixedDeltaTime;



            /** APPLYING **/
            Vector3 v_globalVelocity = Vector3.zero;
            // Apply walk/run velocity
            v_globalVelocity += transform.localRotation * new Vector3(physic.v_WR.x, 0, physic.v_WR.y);

            // Apply jump
            v_globalVelocity += physic.v_jump;

            // Apply gravity
            v_globalVelocity += physic.v_gravity;


            /** POSITION **/
            // Apply velocity on character controller
            CollisionFlags cf = physic.characterController.Move(v_globalVelocity * Time.fixedDeltaTime);



            /** COLLISIONS **/
            if ((cf & CollisionFlags.CollidedBelow) != 0) {
                physic.v_gravity = Vector3.zero;
                physic.v_jump = Vector3.zero;
            }
            if ((cf & CollisionFlags.CollidedAbove) != 0) {
                physic.v_jump = Vector3.zero;
            }

        }

        // Rendering Update
        private void LateUpdate() {
            GameObject camera = cameraConfiguration.target;
            if (camera == null) {
                Debug.LogWarningFormat("NoCameraAsociatedWithCharacter");
                return;
            }
            camera.transform.position = transform.position + transform.rotation * Quaternion.Euler(-playerCamera.rotationY, 0,0) * cameraConfiguration.offset;
            //camera.transform.rotation = transform.localRotation;
            //transform.localEulerAngles = new Vector3(0, rotationX, 0);
            camera.transform.localEulerAngles = new Vector3(-playerCamera.rotationY, 0, 0);
        }

        /** DEBUG **/
        void ToogleCursor() {
            if (Cursor.visible) {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Confined;
            } else {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

}