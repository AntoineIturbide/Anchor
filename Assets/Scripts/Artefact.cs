using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class Artefact : MonoBehaviour {
    
    [System.Serializable]
    public class References {
        public Controller.Character character;
        public Camera camera;
        public List<Area> areasPool;

        //[HideInInspector]
        public List<Area> activeAreasPool;
        [HideInInspector]
        public Rigidbody rigidbody;
    }
    public References references;
    private References r { get { return references; } }


    /** CONFIGURATION **/
    [System.Serializable]
    public class TravelConfiguration {
        public Vector3 directionMin;
        public Vector3 directionMax;
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
        CHARGING,
        TRAVELING,
        IN_STASIS,
        RETURNING
    }
    public State state = State.IDLE;

    public void Project(Vector3 origin, Quaternion direcition) {
        transform.position = origin;
        r.rigidbody.velocity = direcition * Vector3.Lerp(tc.directionMin, tc.directionMax, slider);
        slider = 0;
        gameObject.SetActive(true);
        r.rigidbody.isKinematic = false;
        state = State.TRAVELING;
    }

    public void Activate() {
        // Close previously created areas
        CloseAllAreas();
        // Open a new area
        OpenArea();
    }

    private bool OpenArea() {
        if (r.areasPool.Count <= 0)
            return false;

        Area area = r.areasPool[0];
        r.areasPool.Remove(area);
        r.activeAreasPool.Add(area);

        area.Open(transform.position, r.rigidbody.velocity);

        return true;
    }

    public void CloseAllAreas() {
        // Close previously created areas
        for (int i = r.activeAreasPool.Count - 1; i >= 0; --i) {
            StartCoroutine(CloseArea(r.activeAreasPool[i]));
            r.activeAreasPool.RemoveAt(i);
        }
    }

    IEnumerator CloseArea(Area area) {
        area.Close();
        yield return new WaitUntil(() => area.IsClosed());
        r.areasPool.Add(area);
        area.gameObject.SetActive(false);
    }
    
    public void SetInStasis() {
        //transform.localScale = Vector3.one * (sc.range * 2f);
        //r.rigidbody.velocity = Vector3.zero;
        //r.rigidbody.isKinematic = true;
        r.rigidbody.useGravity = false;

        state = State.IN_STASIS;
    }

    public void Retrieve() {
        transform.localScale = Vector3.one;
        r.rigidbody.isKinematic = true;
        r.rigidbody.useGravity = true;
        //gameObject.SetActive(false);
        state = State.RETURNING;
    }

    public void Retrieved() {
        state = State.IDLE;
    }

    /** Inputs **/
    public void Update() {
        InputUpdate();
    }

    public void InputUpdate() {
        switch (state) {
            case State.IDLE:
                if (Input.GetMouseButtonDown(0)) {
                    // Start charging
                    state = State.CHARGING;
                }
                break;

            case State.CHARGING:
                // Ablility cast
                if (Input.GetMouseButtonUp(0)) {
                    Project(r.character.transform.position, r.camera.transform.rotation);
                }
                if (Input.GetMouseButtonDown(1)) {
                    state = State.IDLE;
                    state = State.IDLE;
                    slider = 0;
                }
                break;

            case State.TRAVELING:
                // Ablility stop cast
                if (Input.GetMouseButtonDown(0)) {
                    // Retrieve
                    Retrieve();
                }
                // Ablility activate
                else if (Input.GetMouseButtonDown(1)) {
                    // Set in stasis
                    Activate();
                }
                break;
        }
        
        if (state != State.TRAVELING && Input.GetMouseButtonDown(1)) {
            CloseAllAreas();
        }
    }



    /** UPDATES **/
    private void FixedUpdate() {
        switch (state) {
            case State.CHARGING:
                ChargingUpdate();
                break;
            case State.TRAVELING:
                TravelUpdate();
                break;
            case State.IN_STASIS:
                //StasisUpdate();
                break;
            case State.RETURNING:
                ReturnUpdate();
                break;
            case State.IDLE:
                ReturnUpdate();
                break;
        }
    }

    private void TravelUpdate() {

    }
    

    public float slider = 0;
    public float rotation = 0;
    private void ChargingUpdate() {
        slider = Mathf.MoveTowards(slider, 1f, Time.fixedDeltaTime/1f);
        rotation = Mathf.Repeat(rotation + (Time.fixedDeltaTime * (1 + slider * 3f) * 360f), 360f);
        Vector3 position = transform.position;
        //Vector3 target = r.character.transform.position + r.character.transform.up * 2 + (Quaternion.Euler(360f * Mathf.Repeat(Time.fixedTime * (1 + slider), 1), 0, 0) * r.character.transform.right);
        Vector3 target = r.character.transform.position + r.character.transform.up * 2 + (r.character.transform.rotation * Quaternion.Euler(0,0, rotation) * Vector3.right);    
        position = Vector3.MoveTowards(
            position,
            target,
            ((Vector3.Distance(position, target) * 2f) + (4f)) * Time.fixedDeltaTime
            );
        transform.position = target;
    }


    private void ReturnUpdate() {
        Vector3 position = transform.position;
        Vector3 target = r.character.physic.isInStasis ?
            r.character.transform.position + r.character.transform.up * (Mathf.Sin(Time.time) * 2) + (Quaternion.Euler(0, 180 * Time.time, 0) * Vector3.right * (Mathf.Cos(Time.time) * 2)) :
            r.character.transform.position + r.character.transform.up * 2 + (Quaternion.Euler(0, 180 * Time.time, 0) * Vector3.right);
        position = Vector3.MoveTowards(
            position,
            target,
            ((Vector3.Distance(position, target) * 2f) + ( 4f )) * Time.fixedDeltaTime
            );
        transform.position = position;

        if(Vector3.Distance(position, target) <= 3f) {
            Retrieved();
        }
    }


    /** UNITY **/
    void Awake() {
        r.rigidbody = GetComponent<Rigidbody>();
    }
}
