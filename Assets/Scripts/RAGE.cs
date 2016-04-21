using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RAGE : MonoBehaviour {
    
    [System.Serializable]
    public class References {
        public Rigidbody artefact;
    }
    public References references;
    private References r { get { return references; } }


    /* PARAMETERS */

    public float cleanValue = 1.5f;

    public enum KeyboardMode {
        NULL,
        QWERTY,
        AZERTY
    }
    public KeyboardMode keyboardMode = KeyboardMode.AZERTY;

    /* FEEDBACK */

    [Range(0, 1)]
    public float recentFeedback = float.NaN;

    [Range(0, 1)]
    public float oldFeedback = float.NaN;

    [Range(0, 1)]
    public float strFeedback = float.NaN;

    [Range(-1, 1)]
    public float strFeedback2 = float.NaN;
    
    public bool valid = false;
    
    public float rageStrengh = 20f;
    public float frontRageStrengh = 10f;
    public float upStrengh = 5f;
    public float randomStrengh = 2f;
    public float distance = 10f;
    public AnimationCurve distanceStr = new AnimationCurve();

    /* DATA */

    [System.Serializable]
    public class Key {
        public float time;
        public int level;
        public float value;
    }

    public List<Key> keys = new List<Key>();

    List<Rigidbody> corpses = new List<Rigidbody>();
    bool thrown = false;

    /* METHODS */

    void Start() {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.AddComponent<Rigidbody>();
        GameObject instance = null;
        for(int i = 0; i < 20; i++) {
            instance = Instantiate(go);
            instance.transform.position = transform.position + transform.forward * 10f + transform.up * 20f +  Random.onUnitSphere * 10f;
            instance.transform.localRotation = Random.rotation;
            instance.transform.localScale = Vector3.one * Random.Range(0.5f, 2f);
            corpses.Add(instance.GetComponent<Rigidbody>());
        }
        corpses.Add(r.artefact);

    }

    float strFeedbackOld = 0;

    public void Update() {

        if (Input.GetMouseButtonDown(2)) {
            DEBUG_TeleportTestCorpses();
        }

        if (Input.anyKeyDown) {
            RetrieveKeys();
        }
        CleanKeys();
        FeedbackKeys();

        if (valid && strFeedback > 0.25f && keys.Count > 5) {
            if (!thrown) {
                foreach (Rigidbody body in corpses) {
                    float dist = Mathf.Clamp01(Vector3.Distance(body.transform.position, transform.position) / distance);
                    Vector3 impulse = (transform.up * Random.Range(upStrengh/2,upStrengh) * strFeedback + (transform.forward * frontRageStrengh * Mathf.Clamp01(1 - Mathf.Abs(strFeedback2 * 10f))) + (transform.right * strFeedback2).normalized * rageStrengh * strFeedback + Random.insideUnitSphere * randomStrengh) * distanceStr.Evaluate(dist);
                    Vector3 vel = body.velocity;
                    vel -= Vector3.Project(vel, impulse);
                    vel += impulse;
                    body.velocity = vel;
                }
                thrown = true;
            } else {
                if (strFeedback > strFeedbackOld) {
                    foreach (Rigidbody body in corpses) {
                        float dist = Mathf.Clamp01(Vector3.Distance(body.transform.position, transform.position) / distance);
                        Vector3 impulse = (transform.up * upStrengh * (strFeedbackOld - strFeedback) + (transform.right * strFeedback2).normalized * rageStrengh * (strFeedbackOld - strFeedback)) * Time.deltaTime * distanceStr.Evaluate(dist);
                        Vector3 vel = body.velocity;
                        vel += impulse;
                        body.velocity = vel;
                    }
                } else {
                    thrown = false;
                }
            }
        } else {
            thrown = false;
        }
        strFeedbackOld = strFeedback;
    }

    public void DEBUG_TeleportTestCorpses() {
        for (int i = 0; i < corpses.Count; i++) {
            corpses[i].velocity = Vector3.zero;
            corpses[i].angularVelocity = Vector3.zero;
            corpses[i].transform.position = transform.position + transform.forward * 10f + transform.up * 20f + Random.onUnitSphere * 10f;
            corpses[i].transform.localRotation = Random.rotation;
            corpses[i].transform.localScale = Vector3.one * Random.Range(0.5f, 2f);
        }
    }

    public void RetrieveKeys() {
        Key key = new Key();
        if (Input.GetKeyDown(KeyCode.A)) {
            key.time = Time.time;
            switch (keyboardMode) {
                case KeyboardMode.QWERTY:
                    key.level = 3;
                    key.value = 0.00f;
                    break;
                case KeyboardMode.AZERTY:
                    key.level = 1;
                    key.value = 0.05f;
                    break;
            }
            keys.Add(key);
            key = new Key();
        }
        if (Input.GetKeyDown(KeyCode.B)) {
            key.time = Time.time;
            key.level = 3;
            key.value = 0.50f;
            keys.Add(key);
            key = new Key();
        }
        if (Input.GetKeyDown(KeyCode.C)) {
            key.time = Time.time;
            key.level = 3;
            key.value = 0.30f;
            keys.Add(key);
            key = new Key();
        }
        if (Input.GetKeyDown(KeyCode.D)) {
            key.time = Time.time;
            key.level = 2;
            key.value = 0.25f;
            keys.Add(key);
            key = new Key();
        }
        if (Input.GetKeyDown(KeyCode.E)) {
            key.time = Time.time;
            key.level = 1;
            key.value = 0.20f;
            keys.Add(key);
            key = new Key();
        }
        if (Input.GetKeyDown(KeyCode.F)) {
            key.time = Time.time;
            key.level = 2;
            key.value = 0.35f;
            keys.Add(key);
            key = new Key();
        }
        if (Input.GetKeyDown(KeyCode.G)) {
            key.time = Time.time;
            key.level = 2;
            key.value = 0.45f;
            keys.Add(key);
            key = new Key();
        }
        if (Input.GetKeyDown(KeyCode.H)) {
            key.time = Time.time;
            key.level = 2;
            key.value = 0.55f;
            keys.Add(key);
            key = new Key();
        }
        if (Input.GetKeyDown(KeyCode.I)) {
            key.time = Time.time;
            key.level = 1;
            key.value = 0.75f;
            keys.Add(key);
            key = new Key();
        }
        if (Input.GetKeyDown(KeyCode.J)) {
            key.time = Time.time;
            key.level = 2;
            key.value = 0.70f;
            keys.Add(key);
            key = new Key();
        }
        if (Input.GetKeyDown(KeyCode.K)) {
            key.time = Time.time;
            key.level = 2;
            key.value = 0.80f;
            keys.Add(key);
            key = new Key();
        }
        if (Input.GetKeyDown(KeyCode.L)) {
            key.time = Time.time;
            key.level = 2;
            key.value = 0.90f;
            keys.Add(key);
            key = new Key();
        }
        if (Input.GetKeyDown(KeyCode.M)) {
            key.time = Time.time;
            switch (keyboardMode) {
                case KeyboardMode.QWERTY:
                    key.level = 3;
                    key.value = 0.75f;
                    break;
                case KeyboardMode.AZERTY:
                    key.level = 2;
                    key.value = 1.00f;
                    break;
            }
            keys.Add(key);
            key = new Key();
        }
        if (Input.GetKeyDown(KeyCode.N)) {
            key.time = Time.time;
            key.level = 3;
            key.value = 0.65f;
            keys.Add(key);
            key = new Key();
        }
        if (Input.GetKeyDown(KeyCode.O)) {
            key.time = Time.time;
            key.level = 1;
            key.value = 0.85f;
            keys.Add(key);
            key = new Key();
        }
        if (Input.GetKeyDown(KeyCode.P)) {
            key.time = Time.time;
            key.level = 1;
            key.value = 0.95f;
            keys.Add(key);
            key = new Key();
        }
        if (Input.GetKeyDown(KeyCode.Q)) {
            key.time = Time.time;
            switch (keyboardMode) {
                case KeyboardMode.QWERTY:
                    key.level = 1;
                    key.value = 0.00f;
                    break;
                case KeyboardMode.AZERTY:
                    key.level = 2;
                    key.value = 0.05f;
                    break;
            }
            keys.Add(key);
            key = new Key();
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            key.time = Time.time;
            key.level = 1;
            key.value = 0.30f;
            keys.Add(key);
            key = new Key();
        }
        if (Input.GetKeyDown(KeyCode.S)) {
            key.time = Time.time;
            key.level = 2;
            key.value = 0.15f;
            keys.Add(key);
            key = new Key();
        }
        if (Input.GetKeyDown(KeyCode.T)) {
            key.time = Time.time;
            key.level = 1;
            key.value = 0.45f;
            keys.Add(key);
            key = new Key();
        }
        if (Input.GetKeyDown(KeyCode.U)) {
            key.time = Time.time;
            key.level = 1;
            key.value = 0.65f;
            keys.Add(key);
            key = new Key();
        }
        if (Input.GetKeyDown(KeyCode.V)) {
            key.time = Time.time;
            key.level = 3;
            key.value = 0.45f;
            keys.Add(key);
            key = new Key();
        }
        if (Input.GetKeyDown(KeyCode.W)) {
            key.time = Time.time;
            switch (keyboardMode) {
                case KeyboardMode.QWERTY:
                    key.level = 1;
                    key.value = 0.10f;
                    break;
                case KeyboardMode.AZERTY:
                    key.level = 3;
                    key.value = 0.10f;
                    break;
            }
            keys.Add(key);
            key = new Key();
        }
        if (Input.GetKeyDown(KeyCode.X)) {
            key.time = Time.time;
            key.level = 3;
            key.value = 0.20f;
            keys.Add(key);
            key = new Key();
        }
        if (Input.GetKeyDown(KeyCode.Y)) {
            key.time = Time.time;
            key.level = 1;
            key.value = 0.55f;
            keys.Add(key);
            key = new Key();
        }
        if (Input.GetKeyDown(KeyCode.Z)) {
            key.time = Time.time;
            switch (keyboardMode) {
                case KeyboardMode.QWERTY:
                    key.level = 3;
                    key.value = 0.10f;
                    break;
                case KeyboardMode.AZERTY:
                    key.level = 1;
                    key.value = 0.10f;
                    break;
            }
            keys.Add(key);
            key = new Key();
        }
    }

    private void ClearKeys(float dirction) {
        float value = float.NaN;
        float time = float.NaN;
        for (int i = keys.Count - 1; i >= 0; --i) {
            if(!(float.IsNaN(value) || float.IsNaN(time))) {
                if(
                    time > keys[i].time
                    &&
                    (dirction > 0) ?
                    (value - keys[i].value) > 0 :
                    (value - keys[i].value) < 0
                ){
                    keys.RemoveAt(i);
                }
            }
            value = keys[i].value;
            time = keys[i].time;
        }
    }

    private void CleanKeys() {
        while (keys.Count > 0 && keys[0].time + cleanValue < Time.time) {
            //Debug.LogFormat("Clean - delta = {0}", keys[0].time);
            keys.RemoveAt(0);
        }
    }

    private void FeedbackKeys() {
        /* GRADIENT */

        /* RECENT */
        float deltaTime = 0;
        float count = 0;
        float value = 0;
        for(int i = 0; i < keys.Count; i++) {
            deltaTime = (Time.time - keys[i].time)/cleanValue;
            deltaTime = Mathf.Clamp01(1 - deltaTime);
            count += 1 * deltaTime;
            value += keys[i].value * deltaTime;
        }
        value /= count;
        recentFeedback = value;
        

        count = 0;
        value = 0;
        for (int i = 0; i < keys.Count; i++) {
            deltaTime = (Time.time - keys[i].time) / cleanValue;
            count += 1 * deltaTime;
            value += keys[i].value * deltaTime;
        }
        value /= count;
        oldFeedback = value;

        float v = float.NaN;
        float time = float.NaN;
        value = keys.Count > 1 ? 0 : float.NaN;
        for (int i = 0; i < keys.Count; i++) {
            if (!float.IsNaN(time)) {
                float dt = keys[i].time - time;
                dt = Mathf.Clamp01(1 - dt);
                value += dt * (keys[i].value - v);
                if (dt * Mathf.Abs(keys[i].value - v) > 0.25f) {
                    value = 0;
                    break;
                }
            }
            time = keys[i].time;
            v = keys[i].value;
        }
        strFeedback2 = value;


        //strFeedback = Mathf.Clamp01(keys.Count / 12f);

        if(keys.Count > 0) {
            bool valid1 = false;
            bool valid2 = false;
            bool valid3 = false;
            float minValue = 1; 
            float maxValue = 0;
            for (int i = 0; i < keys.Count; i++) {
                minValue = Mathf.Min(keys[i].value, minValue);
                maxValue = Mathf.Max(keys[i].value, maxValue);
                if (!valid1 && keys[i].level == 1)
                    valid1 = true;
                if (!valid2 && keys[i].level == 2)
                    valid2 = true;
                if (!valid3 && keys[i].level == 3)
                    valid3 = true;
            }
            strFeedback = Mathf.Abs(maxValue - minValue);
            valid = (valid1 && valid2) || (valid2 && valid3) || (valid1 && valid3);
        } else {
            strFeedback = float.NaN;
            valid = false;
        }

    }

}
