using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectTouch : MonoBehaviour
{
    void Update()
    {
        if (Input.touchCount > 0)
        {
            Debug.Log("Touch detected!");
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                Debug.Log("Touch Began at: " + touch.position);
            }
        }
    }

}
