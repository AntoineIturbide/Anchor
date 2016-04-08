using UnityEngine;
using System.Collections;

namespace Controller {
    [RequireComponent(typeof(CharacterController))]
    public class Character : MonoBehaviour {
        /** REFERENCES **/
        // Ability
        [System.Serializable]
        public class References {
            public CoreAbility coreAbility;
        }
        public References references;
        private References r { get { return references; } }

        /** CONFIGURATION **/
        // Camera
        [System.Serializable]
        public class CameraConfiguration {
            public GameObject target;
            public Vector3 offsetRight;
            public Vector3 offsetLeft;
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
            public int jumpCount = 1;
            public Vector3 gravity = new Vector3(0, -9.81f, 0);
        }
        public DisplacementConfiguration displacementConfiguration;

        /** CAMERA **/
        public class PlayerCamera {
            public float rotationY = 0F;
            public float slider = 0f;
            public bool isOnRightSide = false;
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
            public float a_jump = 9f;
            public Vector3 v_jump = Vector3.zero;
            public int l_jumpCount = 0;
            public bool isInStasis = false;
        }
        public Physic physic = new Physic();

        /*
        public void CatchedByStasis() {
            physic.isInStasis = true;
            physic.l_jumpCount = 1;
            physic.v_gravity = Vector3.zero;
            physic.v_jump = Vector3.zero;
        }

        public void EjectFromStasis() {

        }*/


        public void OnStasisEnter() {
            physic.isInStasis = true;
            physic.l_jumpCount = 1;
            physic.v_gravity = Vector3.zero;
            physic.v_jump = Vector3.zero;
        }
        
        public void OnStasisStay(Vector3 origin, float str = 1) {
            Vector3 newPos = transform.position;
            newPos = Vector3.MoveTowards(newPos, origin, str * Time.fixedDeltaTime);
            transform.position = newPos;
            physic.v_gravity = Vector3.zero;
            physic.v_jump = Vector3.zero;
        }

        public void OnStasisExit() {
            physic.isInStasis = false;
            if (Input.GetKey(KeyCode.Space)) {
                // Try jump
                if (physic.l_jumpCount > 0) {
                    // Adjust jump count
                    physic.l_jumpCount -= 1;
                    // Jump impulse
                    physic.v_jump = transform.rotation * (physic.a_gravity.normalized * -physic.a_jump);
                    // Reset gravity
                    physic.v_gravity = Vector3.MoveTowards(physic.v_gravity, Vector3.zero, Vector3.Project(physic.v_gravity, physic.v_jump).magnitude);
                }
            }
        }

        /** UNITY **/   
        // Constructor
        public void Awake() {
            // Setup
            physic.l_jumpCount = displacementConfiguration.jumpCount;
            physic.a_gravity = displacementConfiguration.gravity;
            physic.a_jump = displacementConfiguration.jumpStrength;

            // Debug
            ToogleCursor();

            // Retriece character controller
            physic.characterController = gameObject.GetComponent<CharacterController>();
        }

        // Input Update
        private void Update() {
            InputUpdate();
            //r.coreAbility.InputUpdate();
        }
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
            physic.i_WRState = !    Input.GetKey(KeyCode.LeftShift);

            /** Retrieve jump input **/
            if (!physic.isInStasis && Input.GetKeyDown(KeyCode.Space)) {
                // Try jump
                if (physic.l_jumpCount > 0) {
                    // Adjust jump count
                    physic.l_jumpCount -= 1;
                    // Jump impulse
                    physic.v_jump = transform.rotation * (physic.a_gravity.normalized * -physic.a_jump);
                    // Reset gravity
                    physic.v_gravity = Vector3.MoveTowards(physic.v_gravity, Vector3.zero, Vector3.Project(physic.v_gravity, physic.v_jump).magnitude);
                }
            }/*
            if (Input.GetKey(KeyCode.Space)) {
                physic.v_jump += transform.rotation * (physic.a_gravity.normalized * -physic.a_jump) * Time.deltaTime;
            }*/

            /** Retrieve player camera inputs **/
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
                // Reset gravity
                physic.v_gravity = Vector3.zero;
                // Reset jumpt
                physic.v_jump = Vector3.zero;
                physic.l_jumpCount = displacementConfiguration.jumpCount;
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
            
            
            RaycastHit hit;
            int mask = ~((1 << 8) | (1 << 9));
            // Left
            Vector3 leftTarget = transform.position + transform.rotation * Quaternion.Euler(-playerCamera.rotationY, 0, 0) * cameraConfiguration.offsetLeft;
            Ray leftRay = new Ray(transform.position, leftTarget - transform.position);
                Debug.DrawRay(leftRay.origin, leftTarget - transform.position, Color.red);
            float leftLength = (leftTarget - transform.position).magnitude;
            bool leftCollide;
            if(leftCollide = Physics.Raycast(leftRay, out hit, leftLength, mask)) {
                leftTarget = Vector3.MoveTowards(hit.point, transform.position, 0.5f);
                leftLength = (leftTarget - transform.position).magnitude;
            }
            // Right
            Vector3 rightTarget = transform.position + transform.rotation * Quaternion.Euler(-playerCamera.rotationY, 0, 0) * cameraConfiguration.offsetRight;
            Ray rightRay = new Ray(transform.position, rightTarget - transform.position);
                Debug.DrawRay(rightRay.origin, rightTarget - transform.position, Color.red);
            float rightLength = (rightTarget - transform.position).magnitude;
            bool rightCollide;
            if (rightCollide = Physics.Raycast(rightRay, out hit, rightLength, mask)) {
                rightTarget = Vector3.MoveTowards(hit.point, transform.position, 0.5f);
                rightLength = (rightTarget - transform.position).magnitude;
            }

            // Compare
            if(leftCollide || rightCollide) {
                playerCamera.isOnRightSide = playerCamera.isOnRightSide ?
                    rightLength * 1.5f > leftLength :
                    rightLength * 0.5f > leftLength;
            }

            playerCamera.slider = playerCamera.isOnRightSide ?
                Mathf.MoveTowards(playerCamera.slider, 1, Time.deltaTime * 3f) :
                Mathf.MoveTowards(playerCamera.slider, 0, Time.deltaTime * 3f);

            Vector3 target = Vector3.Lerp(
                leftTarget,
                rightTarget,
                Mathf.SmoothStep(0, 1, playerCamera.slider)
                );

            camera.transform.position = target;
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