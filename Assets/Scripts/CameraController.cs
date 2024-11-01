using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class CameraController : Singleton<CameraController>
{
    #region Fields

    private float _zoom;
    private float _zoomSpeed;

    private Camera _camera;
    private Vector2 _movementSpeed;

    // Events
    public event Action CameraUpdated;

    //Logic
    private float _previousOrthographicSize;
    private Vector3 _previousPosition;

    // Inspector
    [SerializeField, Range(50, 150)] private float _startingZoom = 20;
    [SerializeField, Range(1, 10)] private float _scrollMultiplier = 50;
    [SerializeField, Range(1f, 3f)] private float _zoomInDivider;
    [SerializeField, Range(0.1f, 0.5f)] private float _scrollSmoothing = 0.25f;
    [SerializeField, Range(0.5f, 5f)] private float _minZoom = 10;
    [SerializeField, Range(30, 200)] private float _maxZoom = 150;

    [Space]
    [SerializeField, Range(0.25f, 1f)] private float _movementMultiplier = 1;
    [SerializeField, Range(0.1f, 0.5f)] private float _movementSmoothing = 0.25f;
    [SerializeField] private Transform _cameraFollow;

    #endregion


    #region Unity Methods

    private void Awake() => _camera = GetComponent<Camera>();

    private void Start() => _zoom = _startingZoom;

    private void Update()
    {
        UpdateCameraFollow();
        UpdatePosition();
        UpdateZoom();

        // Trigger CameraUpdated event
        Vector3 position = transform.position;
        float orthographicSize = _camera.orthographicSize;

        if (_previousPosition != position || _previousOrthographicSize != orthographicSize)
            CameraUpdated?.Invoke();

        _previousPosition = position;
        _previousOrthographicSize = orthographicSize;

        // Reset camera
        if (Input.GetKeyDown(KeyCode.C))
            ResetCamera();
    }

    #endregion


    #region Custom Methods

    /// <summary>
    /// Resets the camera to the specified position and zoom.
    /// </summary>
    /// <param name="position">The new camera position</param>
    /// <param name="zoom">The new camera zoom</param>
    private void ResetCamera(Vector2 position, float zoom)
    {
        transform.position = new Vector3(position.x, position.y, transform.position.z);
        _cameraFollow.transform.localPosition = new Vector3(0, 0, _cameraFollow.transform.localPosition.z);

        _zoom = zoom;
        _camera.orthographicSize = zoom;
    }

    /// <summary>
    /// Resets the camera to the world center.
    /// </summary>
    private void ResetCamera() => ResetCamera(Vector2.zero, _zoom);


    /// <summary>
    /// Moves the camera follow GameObject according to user input.
    /// </summary>
    private void UpdateCameraFollow()
    {
        Vector2 inputVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        Vector3 screenPosition = new Vector3(inputVector.x, inputVector.y, _camera.nearClipPlane) * _movementMultiplier;
        Vector3 correctedPosition = _camera.ViewportToWorldPoint(screenPosition) - _camera.ViewportToWorldPoint(new Vector3(0, 0, _camera.nearClipPlane) * _movementMultiplier);
        correctedPosition = new Vector3(correctedPosition.x, correctedPosition.y * _camera.aspect, correctedPosition.z);

        _cameraFollow.localPosition = new Vector3(correctedPosition.x, correctedPosition.y, 0 - _camera.transform.position.z);
    }

    /// <summary>
    /// Updates the camera's position according to the camera follow GameObject.
    /// </summary>
    private void UpdatePosition()
    {
        Vector2 position = Vector2.SmoothDamp(transform.position, _cameraFollow.position, ref _movementSpeed, _movementSmoothing);
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }

    /// <summary>
    /// Smoothly updates the camera's zoom according to the zoom variable.
    /// </summary>
    private void UpdateZoom()
    {
        // Get the user input
        float input = Input.GetAxis("Mouse ScrollWheel");
        float scroll = input * _camera.orthographicSize;

        if (input > 0)
            scroll /= _zoomInDivider;

        // Apply the zoom
        _zoom -= scroll * _scrollMultiplier;
        _zoom = Mathf.Clamp(_zoom, _minZoom, _maxZoom);

        _camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, _zoom, ref _zoomSpeed, _scrollSmoothing);
    }

    #endregion
}
