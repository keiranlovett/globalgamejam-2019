using UnityEngine;
using System.Collections;

/// <summary>
/// Encapsulates keyboard movement for RtsCamera
/// </summary>
public class RtsCameraTouch : MonoBehaviour
{
    public bool AllowMove;
    public float MoveSpeed;

    public bool AllowFastMove;
    public float FastMoveSpeed;
    public KeyCode FastMoveKeyCode1;
    public KeyCode FastMoveKeyCode2;

    public bool AllowRotate;
    public float RotateSpeed;

    public bool AllowZoom;
    public float ZoomSpeed;

    public bool AllowTilt;
    public float TiltSpeed;

    public KeyCode ResetKey;
    public bool IncludePositionOnReset;

    public bool MovementBreaksFollow;

    public bool RotateUsesInputAxis = false;
    public string RotateInputAxis = "KbCameraRotate";

    public bool ZoomUsesInputAxis = false;
    public string ZoomInputAxis = "KbCameraZoom";

    private bool moveRight;
    private bool moveLeft;
    private bool moveUp;
    private bool moveDown;


    float rotationY = 0;
    float rotationX = 0;


    Vector2?[] oldTouchPositions = {
        null,
        null
    };
    Vector2 oldTouchVector;
    float oldTouchDistance;



    private RtsCamera _rtsCamera;


    //start moving the camera based on the direction
    public void moveCameraButtonDown(string direction)
    {
        if (direction == "right")
        {
            moveRight = true;
        }
        if (direction == "left")
        {
            moveLeft = true;
        }
        if (direction == "up")
        {
            moveUp = true;
        }
        if (direction == "down")
        {
            moveDown = true;
        }
    }

    //stop moving the camera based on direction
    public void moveCameraButtonUp(string direction)
    {
        if (direction == "right")
        {
            moveRight = false;
        }
        if (direction == "left")
        {
            moveLeft = false;
        }
        if (direction == "up")
        {
            moveUp = false;
        }
        if (direction == "down")
        {
            moveDown = false;
        }
    }

    public void resetCamera()
    {
        _rtsCamera.ResetToInitialValues(IncludePositionOnReset, false);

    }

    protected void Reset()
    {
        AllowMove = true;
        MoveSpeed = 20f;

        AllowFastMove = true;
        FastMoveSpeed = 40f;

        AllowRotate = true;
        RotateSpeed = 180f;

        AllowZoom = true;
        ZoomSpeed = 20f;

        AllowTilt = true;
        TiltSpeed = 90f;

        IncludePositionOnReset = false;

        MovementBreaksFollow = true;
    }

    protected void Start()
    {
        _rtsCamera = gameObject.GetComponent<RtsCamera>();
    }

    protected void Update()
    {
        if (_rtsCamera == null)
            return; // no camera, bail!






        if (Input.touchCount == 0)
        {
            oldTouchPositions[0] = null;
            oldTouchPositions[1] = null;
        }
        else if (Input.touchCount == 1)
        {
            if (oldTouchPositions[0] == null || oldTouchPositions[1] != null)
            {
                oldTouchPositions[0] = Input.GetTouch(0).position;
                oldTouchPositions[1] = null;
            }
            else
            {
                Vector2 newTouchPosition = Input.GetTouch(0).position;

                transform.position += transform.TransformDirection((Vector3)((oldTouchPositions[0] - newTouchPosition) * GetComponent<Camera>().orthographicSize / GetComponent<Camera>().pixelHeight * 2f));

                oldTouchPositions[0] = newTouchPosition;
            }
        }
        else
        {
            if (oldTouchPositions[1] == null)
            {
                oldTouchPositions[0] = Input.GetTouch(0).position;
                oldTouchPositions[1] = Input.GetTouch(1).position;
                oldTouchVector = (Vector2)(oldTouchPositions[0] - oldTouchPositions[1]);
                oldTouchDistance = oldTouchVector.magnitude;
            }
            else
            {
                Vector2 screen = new Vector2(GetComponent<Camera>().pixelWidth, GetComponent<Camera>().pixelHeight);

                Vector2[] newTouchPositions = {
                    Input.GetTouch(0).position,
                    Input.GetTouch(1).position
                };
                Vector2 newTouchVector = newTouchPositions[0] - newTouchPositions[1];
                float newTouchDistance = newTouchVector.magnitude;

                transform.position += transform.TransformDirection((Vector3)((oldTouchPositions[0] + oldTouchPositions[1] - screen) * GetComponent<Camera>().orthographicSize / screen.y));
                transform.localRotation *= Quaternion.Euler(new Vector3(0, 0, Mathf.Asin(Mathf.Clamp((oldTouchVector.y * newTouchVector.x - oldTouchVector.x * newTouchVector.y) / oldTouchDistance / newTouchDistance, -1f, 1f)) / 0.0174532924f));
                GetComponent<Camera>().orthographicSize *= oldTouchDistance / newTouchDistance;
                transform.position -= transform.TransformDirection((newTouchPositions[0] + newTouchPositions[1] - screen) * GetComponent<Camera>().orthographicSize / screen.y);

                oldTouchPositions[0] = newTouchPositions[0];
                oldTouchPositions[1] = newTouchPositions[1];
                oldTouchVector = newTouchVector;
                oldTouchDistance = newTouchDistance;
            }
        }





























        if (AllowMove && (!_rtsCamera.IsFollowing || MovementBreaksFollow))
        {
            var hasMovement = false;

            var speed = MoveSpeed;
         
            if (moveRight)
            {
                hasMovement = true;
                _rtsCamera.AddToPosition(speed * Time.deltaTime, 0, 0);
            }

            if (moveLeft)
            {
                hasMovement = true;
                _rtsCamera.AddToPosition(-speed * Time.deltaTime, 0, 0);
            }

            if (moveUp)
            {
                hasMovement = true;
                _rtsCamera.AddToPosition(0, 0, speed * Time.deltaTime);
            }

            if (moveDown)
            {
                hasMovement = true;
                _rtsCamera.AddToPosition(0, 0, -speed * Time.deltaTime);
            }


            if (hasMovement && _rtsCamera.IsFollowing && MovementBreaksFollow)
                _rtsCamera.EndFollow();
        }

        //

        if (AllowRotate)
        {
            if (RotateUsesInputAxis)
            {

                if (Input.touchCount == 1)
                {
                    //rotate camera based on the touch position
                    Touch touch = Input.GetTouch(0);
                    float mouseX = touch.deltaPosition.x;
                    float mouseY = -touch.deltaPosition.y;


                    rotationY += mouseX * RotateSpeed * Time.deltaTime * 0.2f;
                    rotationX += mouseY * RotateSpeed * Time.deltaTime * 0.2f;

                    //clamp x rotation to limit it
                    //  rotationX = Mathf.Clamp(rotationX, -clampAngle, clampAngle);

                    //apply rotation
                    Quaternion rot = Quaternion.Euler(rotationX, rotationY, 0.0f);

/*                    if (Mathf.Abs(rot) > 0.001f)
                    {
                      //  _rtsCamera.Rotation += rot; // * RotateSpeed * Time.deltaTime;
                    }*/

                }
               
            }

        }

        if (AllowZoom && Input.touchCount == 2)
        {
        
            //store two touches
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            //find the position in the previous frame of each touch
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            //find the magnitude of the vector (the distance) between the touches in each frame
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            //find the difference in the distances between each frame
            var zoom = (prevTouchDeltaMag - touchDeltaMag) * 0.005f;

            if (Mathf.Abs(zoom) > 0.001f)
            {
                _rtsCamera.Distance += zoom * ZoomSpeed * Time.deltaTime;
            }
        
        }

        if (AllowTilt && Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {

                var tilt = Input.touches[0].deltaPosition.y;
                if (Mathf.Abs(tilt) > 0.001f)
                {
                    _rtsCamera.Tilt += tilt * TiltSpeed * Time.deltaTime;
                }
 
        }
    }
}