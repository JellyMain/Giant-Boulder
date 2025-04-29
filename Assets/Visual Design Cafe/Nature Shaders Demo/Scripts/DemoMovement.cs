using UnityEngine;

public class DemoMovement : MonoBehaviour
{
    [SerializeField]
    private float _radius = 2.5f;

    [SerializeField]
    private float _speed = 2f;

    void Update()
    {
        transform.position =
            new Vector3(
                Mathf.Sin( Time.time * _speed ) * _radius,
                transform.position.y,
                Mathf.Cos( Time.time * _speed ) * _radius );
    }
}
