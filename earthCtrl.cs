using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class earthCtrl : MonoBehaviour
{
    UIManager um;

    Quaternion desiredRotQ;
    float desiredRot;
    float speed;

    float zoomSpeed = 32;
    float rotSpeed = 8;
    float damping = 4;

    Vector3 direction;
    Vector3 touchFirst;
    Vector3 touchStart;
    Vector3 touchEnd;
    float stopTime;

    float dragTime;
    bool isRoll;

    Camera mainCam;
    public float earthScale;
    public bool scaleOutEarth;
    public bool scaleInEarth;

    // Start is called before the first frame update
    void Start()
    {
        um = FindObjectOfType<UIManager>();
        mainCam = Camera.main;
        desiredRot = transform.eulerAngles.y;
        scaleOutEarth = true;
        speed = rotSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (mainCam.orthographicSize > 2.96f)
        {
            mainCam.orthographicSize -= Time.deltaTime * zoomSpeed;

            return;
        }

        if (mainCam.orthographicSize != 2.96f)
        {
            mainCam.orthographicSize = 2.96f;
        }

        if (scaleOutEarth)
        {
            if(earthScale == 3.2f)
            {
                transform.position = Vector3.Lerp(transform.position, Vector3.left * 3.2f, Time.deltaTime * damping);
            }

            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * (earthScale + 0.16f), Time.deltaTime * 2);

            if(transform.localScale.x >= earthScale)
            {
                scaleOutEarth = false;
            }

            return;
        }

        if(scaleInEarth)
        {
            if (earthScale == 2.4f)
            {
                transform.position = Vector3.Lerp(transform.position, Vector3.zero, Time.deltaTime * damping);
            }

            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * (earthScale - 0.16f), Time.deltaTime * 2);

            if (transform.localScale.x <= earthScale)
            {
                scaleInEarth = false;
            }

            return;
        }

        if (speed == 0)
        {
            stopTime -= Time.deltaTime;
            if (stopTime <= 0)
            {
                stopTime = 0;
                speed += Time.deltaTime;
            }
        }
        else
        {
            if(isRoll)
            {
                desiredRot += direction.x * Time.deltaTime;

                desiredRotQ = Quaternion.Euler(transform.eulerAngles.x, desiredRot, transform.eulerAngles.z);
                transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotQ, Time.deltaTime * rotSpeed);
            }
            else
            {
                speed += Time.deltaTime;
                speed = Mathf.Min(speed, rotSpeed);

                desiredRot -= speed * Time.deltaTime;

                desiredRotQ = Quaternion.Euler(transform.eulerAngles.x, desiredRot, transform.eulerAngles.z);
                transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotQ, Time.deltaTime * damping);
            }
        }
    }

    private void OnMouseDown()
    {
        isRoll = false;
        speed = 0;
        stopTime = damping;
        touchFirst = Input.mousePosition;
        touchStart = Input.mousePosition;
    }

    private void OnMouseDrag()
    {
        dragTime += Time.deltaTime;

        touchEnd = Input.mousePosition;

        direction = touchStart - touchEnd;
        desiredRotQ = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + direction.x / rotSpeed, transform.eulerAngles.z);
        transform.rotation = desiredRotQ;

        touchStart = Input.mousePosition;
    }

    private void OnMouseUp()
    {
        if(touchFirst != touchEnd && earthScale >= 3.2f && !um.getMove("RegionRight"))
        {
            um.callChild("RegionRight");
            earthScale = 2.4f;
            scaleInEarth = true;
        }

        desiredRot = transform.eulerAngles.y;
        stopTime = damping;

        if (dragTime <= 0.32f)
        {
            speed = rotSpeed;
            direction = touchFirst - touchEnd;
            isRoll = true;
        }

        dragTime = 0;
    }
}
