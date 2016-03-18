using UnityEngine;
using System.Collections;

namespace Character {

    public class Camera : MonoBehaviour {

        Vector2 lastMousePos = Vector3.zero;

        void LateUpdate() {

            Input.GetAxis("MouseX");

        }

    }

}