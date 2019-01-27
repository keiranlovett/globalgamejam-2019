using UnityEngine;
using System.Collections;

/// <summary>
/// Encapsulates mouse movement for RtsCamera.
/// </summary>
[AddComponentMenu("Camera-Control/RtsCamera-Mouse")]
public class RtsCameraMouse : MonoBehaviour
{
    public KeyCode MouseOrbitButton;

    public bool AllowScreenEdgeMove;
    public bool ScreenEdgeMoveBreaksFollow;
    public int ScreenEdgeBorderWidth;
    public float MoveSpeedCamera;

    public bool AllowPan;
    public bool PanBreaksFollow;
    public float PanSpeed;

    public bool AllowRotateCamera;
    public float RotateSpeedCamera;

    public bool AllowTiltCamera;
    public float TiltSpeedCamera;

    public bool AllowZoomCamera;
    public float ZoomSpeedCamera;

    public string RotateInputAxisCamera = "Mouse X";
    public string TiltInputAxisCamera = "Mouse Y";
    public string ZoomInputAxisCamera = "Mouse ScrollWheel";
    public KeyCode PanKey1 = KeyCode.LeftShift;
    public KeyCode PanKey2 = KeyCode.RightShift;

    //

    private RtsCamera _rtsCamera;

    //

    protected void Reset()
    {
        MouseOrbitButton = KeyCode.Mouse2;    // middle mouse by default (probably should not use right mouse since it doesn't work well in browsers)

        AllowScreenEdgeMove = true;
        ScreenEdgeMoveBreaksFollow = true;
        ScreenEdgeBorderWidth = 4;
        MoveSpeedCamera = 30f;

        AllowPan = true;
        PanBreaksFollow = true;
        PanSpeed = 50f;
        PanKey1 = KeyCode.LeftShift;
        PanKey2 = KeyCode.RightShift;

        AllowRotateCamera = true;
        RotateSpeedCamera = 360f;

        AllowTiltCamera = true;
        TiltSpeedCamera = 200f;

        AllowZoomCamera = true;
        ZoomSpeedCamera = 500f;

        RotateInputAxisCamera = "Mouse X";
        TiltInputAxisCamera = "Mouse Y";
        ZoomInputAxisCamera = "Mouse ScrollWheel";
    }

    protected void Start()
    {
        _rtsCamera = gameObject.GetComponent<RtsCamera>();
    }

    protected void Update()
    {
        if (_rtsCamera == null)
            return; // no camera, bail!

        if (AllowZoomCamera)
        {
            var scroll = Input.GetAxisRaw(ZoomInputAxisCamera);
            _rtsCamera.Distance -= scroll * ZoomSpeedCamera * Time.deltaTime;
        }

        if (Input.GetKey(MouseOrbitButton))
        {
            if (AllowPan && (Input.GetKey(PanKey1) || Input.GetKey(PanKey2)))
            {
                // pan
                var panX = -1 * Input.GetAxisRaw("Mouse X") * PanSpeed * Time.deltaTime;
                var panZ = -1 * Input.GetAxisRaw("Mouse Y") * PanSpeed * Time.deltaTime;

                _rtsCamera.AddToPosition(panX, 0, panZ);

                if (PanBreaksFollow && (Mathf.Abs(panX) > 0.001f || Mathf.Abs(panZ) > 0.001f))
                {
                    _rtsCamera.EndFollow();
                }
            }
            else
            {
                // orbit

                if (AllowTiltCamera)
                {
                    var tilt = Input.GetAxisRaw(TiltInputAxisCamera);
                    _rtsCamera.Tilt -= tilt * TiltSpeedCamera * Time.deltaTime;
                }

                if (AllowRotateCamera)
                {
                    var rot = Input.GetAxisRaw(RotateInputAxisCamera);
                    _rtsCamera.Rotation += rot * RotateSpeedCamera * Time.deltaTime;
                }
            }
        }

        if (AllowScreenEdgeMove && (!_rtsCamera.IsFollowing || ScreenEdgeMoveBreaksFollow))
        {
            var hasMovement = false;

            if (Input.mousePosition.y > (Screen.height - ScreenEdgeBorderWidth))
            {
                hasMovement = true;
                _rtsCamera.AddToPosition(0, 0, MoveSpeedCamera * Time.deltaTime);
            }
            else if (Input.mousePosition.y < ScreenEdgeBorderWidth)
            {
                hasMovement = true;
                _rtsCamera.AddToPosition(0, 0, -1 * MoveSpeedCamera * Time.deltaTime);
            }

            if (Input.mousePosition.x > (Screen.width - ScreenEdgeBorderWidth))
            {
                hasMovement = true;
                _rtsCamera.AddToPosition(MoveSpeedCamera * Time.deltaTime, 0, 0);
            }
            else if (Input.mousePosition.x < ScreenEdgeBorderWidth)
            {
                hasMovement = true;
                _rtsCamera.AddToPosition(-1 * MoveSpeedCamera * Time.deltaTime, 0, 0);
            }

            if (hasMovement && _rtsCamera.IsFollowing && ScreenEdgeMoveBreaksFollow)
            {
                _rtsCamera.EndFollow();
            }
        }
    }
}
