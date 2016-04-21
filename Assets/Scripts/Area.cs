using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Area : MonoBehaviour {
    /** SHORTCUTS **/
    // References
    private References r { get { return references; } }
    // Configuration
    private Configuration c { get { return configuration; } }

    /** REFERENCES **/
    [System.Serializable]
    public class References {
        public Controller.Character character;
        [HideInInspector]
        public Rigidbody rigidbody;
    }
    public References references;

    /** CONFIGURATION **/
    [System.Serializable]
    public class Configuration {

        [System.Serializable]
        public class Animation {
            public float duration = 0.25f;
        }

        [Header("Physic")]
        public float areaRange = 2.5f;
        public float speedDecayScale = 5f;

        [Header("Animations")]
        public Animation openingAnimation = new Animation();
        public Animation closingAnimation = new Animation();

        // Ease in function
        public float easeIn(float x) {
            return -((x - 1) * (x - 1)) + 1;
        }
        // Ease out function
        public float easeOut(float x) {
            return x * x;
        }
    }
    public Configuration configuration;

    public enum State {
        NULL,
        OPENING,
        OPEN,
        CLOSING,
        CLOSED
    }
    [Header("DEBUG")]
    public State state = State.CLOSED;


    /** UNITY **/

    public void Awake() {
        // Initialise object
        Init();
    }

    private void Update() {
        // Input update
    }

    private void FixedUpdate() {
        // Update physic
        PhysicUpdate();
    }

    private void LateUpdate() {
        // Rendering update
    }

    /** SYSTEM **/

    public void Init() {
        r.rigidbody = GetComponent<Rigidbody>();
        gameObject.SetActive(false);
    }   
    
    public void Open(Vector3 origin, Vector3 impulse) {
        state = State.OPENING;
        // Repositionate
        transform.position = origin;
        r.rigidbody.velocity = impulse;

        // Activate game object
        gameObject.SetActive(true);

        // Play opening animation
        StartCoroutine(OpeningAnimation());
    }

    public void Close() {
        state = State.CLOSING;
        // Eject player

        // Calculate the strengh of the area on the character
        float str = Vector3.Distance(r.character.transform.position, transform.position) / c.areaRange;
        if (str <= 1) {
            r.character.OnStasisExit();
        }

        // Play closing animation
        StartCoroutine(ClosingAnimation());
    }

    IEnumerator OpeningAnimation() {
        float
            lastTime = Time.time,
            deltaTime = 0,
            progress = 0;
        while (state == State.OPENING) {
            /** Apply animation to this frame **/
            // Update scale
            transform.localScale = Vector3.one * c.easeIn(progress) * (c.areaRange * 2);

            /** End animation if compleated **/
            if (progress >= 1) {
                state = State.OPEN;
                break;
            }

            /** Prepare next frame **/
            lastTime = Time.time;
            yield return new WaitForEndOfFrame();
            // Update delta time
            deltaTime = Time.time - lastTime;
            // Update progress
            progress =
                c.openingAnimation.duration > 0 ?
                Mathf.MoveTowards(progress, 1, deltaTime / c.openingAnimation.duration) :
                1;
        }
    }
    
    IEnumerator ClosingAnimation() {
        float
            lastTime = Time.time,
            deltaTime = 0,
            progress = 0;
        while (state == State.CLOSING) {
            /** Apply animation to this frame **/
            // Update scale
            transform.localScale = Vector3.one * Mathf.Clamp01(1 - c.easeOut(progress)) * (c.areaRange * 2);

            /** End animation if compleated **/
            if (progress >= 1) {
                state = State.CLOSED;
                break;
            }

            /** Prepare next frame **/
            lastTime = Time.time;
            yield return new WaitForEndOfFrame();
            // Update delta time
            deltaTime = Time.time - lastTime;
            // Update progress
            progress =
                c.openingAnimation.duration > 0 ?
                Mathf.MoveTowards(progress, 1, deltaTime / c.closingAnimation.duration) :
                1;
        }
    }

    private void PhysicUpdate() {
        // Velocity
        Vector3 velocity = r.rigidbody.velocity;
        velocity = Vector3.MoveTowards(velocity, Vector3.zero, Time.fixedDeltaTime * c.speedDecayScale);
        r.rigidbody.velocity = velocity;

        // Character detection
        if (state == State.OPEN || state == State.OPENING) {
            // Calculate the strengh of the area on the character
            float str = Vector3.Distance(r.character.transform.position, transform.position) / c.areaRange;
            if (str <= 1) {
                if (r.character.physic.isInStasis) {
                    r.character.OnStasisStay(transform.position, str * 20f);
                } else {
                    r.character.OnStasisEnter();
                    r.character.OnStasisStay(transform.position, str * 20f);
                }
            } else if (r.character.physic.isInStasis) {
                r.character.OnStasisExit();
            }
        }
    }

    public bool IsClosed() {
        return state == State.CLOSED;
    }
}
