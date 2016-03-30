using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class Artefact : MonoBehaviour {
    
    [System.Serializable]
    public class References {
        public Controller.Character character;
        [HideInInspector]
        public Rigidbody rigidbody;
    }
    public References references;
    private References r { get { return references; } }


    /** CONFIGURATION **/
    [System.Serializable]
    public class TravelConfiguration {
        public Vector3 direction;
    }
    public TravelConfiguration travelConfiguration = new TravelConfiguration();
    private TravelConfiguration tc { get { return travelConfiguration; } }


    [System.Serializable]
    public class StasisConfiguration {
        public float range;
    }
    public StasisConfiguration stasisConfiguration = new StasisConfiguration();
    private StasisConfiguration sc { get { return stasisConfiguration; } }

    /** MEMORY **/
    public enum State {
        NULL,
        IDLE,
        TRAVELING,
        IN_STASIS,
        RETURNING
    }
    public State state = State.IDLE;

    public void Project(Vector3 origin, Quaternion direcition) {
        transform.position = origin;
        r.rigidbody.velocity = direcition * tc.direction;
        gameObject.SetActive(true);
        state = State.TRAVELING;
    }
    
    public void SetInStasis() {
        transform.localScale = Vector3.one * (sc.range * 2f);
        r.rigidbody.velocity = Vector3.zero;
        r.rigidbody.isKinematic = true;
        state = State.IN_STASIS;
    }

    public void Retrieve() {
        transform.localScale = Vector3.one;
        r.rigidbody.isKinematic = false;
        gameObject.SetActive(false);
        state = State.IDLE;
    }

    /** UPDATES **/
    private void FixedUpdate() {
        switch (state) {
            case State.TRAVELING:
                TravelUpdate();
                break;
            case State.IN_STASIS:
                StasisUpdate();
                break;
            case State.RETURNING:
                ReturnUpdate();
                break;
        }
    }

    private void TravelUpdate() {

    }
    
    private void StasisUpdate() {
        float dist = Vector3.Distance(r.character.transform.position, transform.position) / sc.range;
        if(dist <= 1) {
            Debug.Log(dist);
            Vector3 newPos = r.character.transform.position;
            newPos = Vector3.MoveTowards(newPos, transform.position, dist * 20f * Time.deltaTime);
            r.character.transform.position = newPos;
            r.character.CatchedByStasis();
        }

    }

    private void ReturnUpdate() {

    }


    /** UNITY **/
    void Awake() {
        r.rigidbody = GetComponent<Rigidbody>();
    }
}
