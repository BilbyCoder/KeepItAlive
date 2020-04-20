using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCharacter : MonoBehaviour
{
    public float Speed;
    public Transform blobHolder;
    public Transform blobPlacer;

    Rigidbody2D _body;
    private bool _holdingBlob;
    private BlobBrain _blob;

    // Start is called before the first frame update
    void Start()
    {
        _body = GetComponent<Rigidbody2D>();
        _holdingBlob = false;
    }

    // Update is called once per frame
    void Update()
    {


        if (Input.GetKey(KeyCode.LeftArrow))
        {
            var vel = _body.velocity;
            vel.x = -Speed * Time.deltaTime;
            _body.velocity = vel;

            var scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            var vel = _body.velocity;
            vel.x = Speed * Time.deltaTime;
            _body.velocity = vel;

            var scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            //_body.AddForce(new Vector3(0, 600, 0));
            var vel = _body.velocity;
            vel.y = Speed * Time.deltaTime * 1.5f;
            _body.velocity = vel;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!_holdingBlob)
            {
                PickupBlob();
            }
            else
            {
                _blob.transform.parent = blobPlacer;
                _blob.transform.localPosition = Vector3.zero;
                _blob.transform.localRotation = Quaternion.identity;
                _holdingBlob = false;
                _blob.Dropped();
                _blob = null;
            }
        }
    }

    private void PickupBlob()
    {
        var blobs = GameObject.FindObjectsOfType<BlobBrain>();
        foreach (var blob in blobs)
        {
            if (Vector2.Distance(blob.transform.position, transform.position) < 1.5f  && !blob.IsDormant && !_holdingBlob)
            {
                blob.transform.parent = blobHolder;
                blob.transform.localPosition = Vector3.zero;
                blob.transform.localRotation = Quaternion.identity;
                blob.BeingHeld();
                _blob = blob;
                _holdingBlob = true;
            }
        }
    }
}
