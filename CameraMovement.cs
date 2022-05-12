using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    [SerializeField] private GameObject player;
    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField] private float zOffset = -10f;
    [SerializeField] private bool canFollow = true;
    [SerializeField] private BoxCollider2D _cameraBounds;
    private Vector3 velocity = Vector3.zero;
    private Camera _camera;
    Vector2 _cameraDimention;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _cameraDimention.y = _camera.orthographicSize;
        _cameraDimention.x = _camera.orthographicSize * _camera.aspect;
        if (_cameraBounds == null)
        {
            _cameraBounds = GameObject.FindGameObjectWithTag("CameraBounds").GetComponent<BoxCollider2D>();
        }
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }


    }
    private void FixedUpdate()
    {
        if (!canFollow)
        {
            return;
        }


        Vector3 targetPosition = new Vector3(player.transform.position.x, player.transform.position.y, zOffset);

        float minX = _cameraBounds.transform.position.x - _cameraBounds.size.x / 2 + _cameraDimention.x;
        float maxX = _cameraBounds.transform.position.x + _cameraBounds.size.x / 2 - _cameraDimention.x;
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);

        float minY = _cameraBounds.transform.position.y - _cameraBounds.size.y / 2 + _cameraDimention.y;
        float maxY = _cameraBounds.transform.position.y + _cameraBounds.size.y / 2 - _cameraDimention.y;
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime * Time.deltaTime);


    }

    

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_cameraBounds.transform.position, _cameraBounds.size);
    }
}