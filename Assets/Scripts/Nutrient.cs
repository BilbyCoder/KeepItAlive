using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nutrient : MonoBehaviour
{
    // Start is called before the first frame update
    public int Bites;
    public int Health;

    private List<BlobBrain> _munchers;
    private Spawner _spawner;

    private void Start()
    {
        _munchers = new List<BlobBrain>();
    }

    private void Update()
    {
        if (Bites == 0 && _munchers.Count == 0)
        {
            Destroy(gameObject);
        }
    }

    public void RegisterEating(BlobBrain blob)
    {
        if (! _munchers.Contains(blob))
        {
            _munchers.Add(blob);
        }
    }

    public void FinishedEating(BlobBrain blob)
    {
        _munchers.Remove(blob);

        if (_munchers.Count == 0)
        {
            Destroy(gameObject);
        }

        if (_spawner != null) _spawner.DeRegister();
    }

    public int Eat()
    {
        if (Bites == 0) return 0;

        Bites -= 1;
        transform.localScale = transform.localScale * 0.5f;

        return Health;
    }

    public void RegisterSpawner(Spawner spawner)
    {
        _spawner = spawner;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("platform"))
        {
            GetComponent<Rigidbody2D>().isKinematic = true;
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            GetComponent<Collider2D>().isTrigger = true;
            gameObject.layer = 0;
        }


        if (collision.transform.CompareTag("Player"))
        {
            Bites = 0;
            if (_munchers.Count == 0)
            {
                _spawner.DeRegister();
                Destroy(gameObject);
            }
            else
            {
                Destroy(GetComponent<SpriteRenderer>());
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.CompareTag("Player"))
        {
            Bites = 0;
            if (_munchers.Count == 0)
            {
                _spawner.DeRegister();
                Destroy(gameObject);
            }
            else
            {
                Destroy(GetComponent<SpriteRenderer>());
            }
        }
    }

    public bool Falling
    {
        get
        {
            return !GetComponent<Rigidbody2D>().isKinematic;
        }
    }
}
