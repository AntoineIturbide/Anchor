using UnityEngine;
using System.Collections;

namespace Controller {
    public class CoreAbility : MonoBehaviour {
        /** REFERENCES **/
        // Ability
        [System.Serializable]
        public class References {
            public Character character;
            public Camera camera;
            public Artefact artefact;
        }
        public References references;
        private References r { get { return references; } }



        /** CONFIGURATION **/
        // Ability
        [System.Serializable]
        public class AbilityConfiguration {
            public float preChargeDuration = 0.25f;
            public float timeToFullCharge = 1f;
            public float preCastDuration = 0.25f;
            public float cooldown = 1f;
        }
        public AbilityConfiguration abilityConfiguration;
        private AbilityConfiguration ac { get { return abilityConfiguration; } }


        /** MEMORY **/

        public enum State {
            NULL,
            IDLE,
            CHARGING,
            PRE_CAST,
            PRE_ACTIVATION,
            POST_ACTIVATION,
            RETURNING
        }
        public State state = State.IDLE;



        public void InputUpdate() {
            switch (r.artefact.state) {
                case Artefact.State.IDLE:
                    if (Input.GetMouseButtonDown(0)) {
                        // Start charging
                        state = State.CHARGING;
                        r.artefact.state = Artefact.State.CHARGING;
                        StartCoroutine(Charge());
                    }
                    break;
            }

            switch (state) {
                /*
                case State.IDLE:
                    // Ablility cast
                    if (Input.GetMouseButtonDown(0)) {
                        // Start charging
                        state = State.CHARGING;
                        StartCoroutine(Charge());
                    }
                    break;
                    */
                case State.CHARGING:
                    // Ablility cast
                    if (Input.GetMouseButtonUp(0)) {
                        // Stop charging

                        // Start casting
                        state = State.PRE_ACTIVATION;
                        StartCoroutine(Cast());
                    }
                    if (Input.GetMouseButtonDown(1)) {
                        state = State.IDLE;
                        r.artefact.state = Artefact.State.IDLE;
                        r.artefact.slider = 0;
                    }
                    break;
                case State.PRE_ACTIVATION:
                    // Ablility stop cast
                    if (Input.GetMouseButtonDown(0)) {
                        // Retrieve
                        state = State.IDLE;
                        r.artefact.Retrieve();
                    }
                    // Ablility activate
                    else if (Input.GetMouseButtonDown(1)) {
                        // Set in stasis
                        state = State.PRE_ACTIVATION;
                        //r.artefact.SetInStasis();
                        r.artefact.Activate();
                    }
                    break;
                case State.POST_ACTIVATION:
                    // Ablility stop cast or desactivate
                    if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || (r.character.physic.isInStasis && Input.GetKeyDown(KeyCode.Space))) {
                        // Retrieve
                        state = State.IDLE;
                        r.artefact.Retrieve();
                    }
                    break;
            }

            if(!(state == State.PRE_ACTIVATION) && Input.GetMouseButtonDown(1)) {
                r.artefact.CloseAllAreas();
            }
        }

        IEnumerator Charge() {
            // Begin charge
            yield return new WaitForSeconds(ac.preCastDuration);

            // Charging

            // End charging

        }

        IEnumerator Cast() {
            // Begin casting
            yield return new WaitForSeconds(ac.preCastDuration);

            // Cast
            r.artefact.Project(r.character.transform.position, r.camera.transform.rotation);

            // End casting
        }

        IEnumerator Activate() {
            // Begin casting
            yield return new WaitForSeconds(ac.preCastDuration);

            // Cast

            // End casting
        }

        IEnumerator Desactivate() {
            // Begin casting
            yield return new WaitForSeconds(ac.preCastDuration);

            // Cast

            // End casting
        }

        IEnumerator Retrieve() {
            // Begin casting
            yield return new WaitForSeconds(ac.preCastDuration);

            // Cast

            // End casting
        }

        /** UNITY **/
    }
}