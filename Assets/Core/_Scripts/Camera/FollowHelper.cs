using UnityEngine;
using System.Collections;

public class FollowHelper : MonoBehaviour
{
    public GameObject FollowTarget;

    public GameObject Home;

    public bool Snap = true;
    public KeyCode HomeKey = KeyCode.Q;
    public bool returnHome = true;
    private RtsCamera _rtsCamera;
    private GameObject _prevFollowTarget;

    void Reset()
    {
        FollowTarget = null;
        Snap = true;
    }

    void Start()
    {
        _rtsCamera = Camera.main.GetComponent<RtsCamera>();
        SetTarget();
    }

    void Update()
    {
        if (FollowTarget != _prevFollowTarget)
        {
            SetTarget();
        }


        if (Input.GetKeyDown(HomeKey) || returnHome == true)
        {
            _rtsCamera.Follow(Home, true);
        }

    }

    private void SetTarget()
    {
        _rtsCamera.Follow(FollowTarget, Snap);
        _prevFollowTarget = FollowTarget;
    }

}
