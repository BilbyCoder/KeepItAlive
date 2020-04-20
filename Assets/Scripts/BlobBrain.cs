using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public enum BlobState
{
    Eating,
    Searching,
    Falling,
    GoingToFood,
    BeingCarried,
    Flipped,
    Dormant
}

public class BlobBrain : MonoBehaviour
{
    // Start is called before the first frame update
    public int StartingHealth;
    public int ReadyHealth;
    public int DangerHealth;
    public int BiteGainedHealth;
    public float HealthFade;
    public TextMeshPro Text;

    public AudioClip JumpSFX;
    public AudioClip EatSFX;
    public AudioClip PoisonSFX;
    public AudioClip HelpMeSFX;

    float _health;
    float _platformCount;
    float _startFalling;
    
    BlobState state;
    Nutrient _targetDestination;
    Rigidbody2D _rigid;
    Nutrient _meal;
    Nutrient _bad;
    GameState _game;
    AudioSource _audioPlayer;
    bool _reportedOutcome;

    private void Awake()
    {
        state = BlobState.Searching;
        _platformCount = 0;
        _health = StartingHealth;
    }

    void Start()
    {
        _audioPlayer = GetComponent<AudioSource>();
        _rigid = GetComponent<Rigidbody2D>();
        _game = GameObject.FindObjectOfType<GameState>();
        Text.text = "Health\n" + _health + "\\" + ReadyHealth;
        Text.transform.parent = null;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHealth();
        if (state == BlobState.Dormant)
        {
            
            return;
        }
        
        var rot = Mathf.Abs(transform.rotation.eulerAngles.z);

        if (rot > 90f && rot < 270 && state != BlobState.Falling)
        {
            state = BlobState.Flipped;
        }
        
        switch (state)
        {
            case BlobState.Searching:
                LookForNutrient();
                break;
            case BlobState.GoingToFood:
                MoveBlob();
                break;
            case BlobState.Flipped:

                if (_rigid.angularVelocity == 0)
                {
                    _rigid.AddTorque(Random.Range(-35, 35));
                    _audioPlayer.clip = HelpMeSFX;
                    _audioPlayer.Play();
                }
                break;
        }

    }

    private void FixedUpdate()
    {
        if (_health > ReadyHealth)
        {
            GetComponentInChildren<SpriteRenderer>().color = new Color(135 / 255f, 253 / 255f, 101 / 255f);

        }
    }

    public void BeingHeld()
    {
        state = BlobState.BeingCarried;
        _rigid.isKinematic = true;
    }

    public void Dropped()
    {
        state = BlobState.Searching;
        _rigid.isKinematic = false;
        transform.parent = null;
    }

    void UpdateHealth()
    {

        Text.transform.position = transform.position + new Vector3(0, 1.25f, 0);

        if (IsDormant)
        {
            if (!_reportedOutcome) { 
                if (_health == 0)
                {
                    Text.text = "I Ded!";
                    _game.RegisterDead();
                    transform.rotation = Quaternion.Euler(0, 0, 180);
                }
                else
                {
                    Text.text = "Hibernating!";
                    _game.RegisterSurvived();
                }
                _reportedOutcome = true;
            }
            return;
        }

        _health -= HealthFade * Time.deltaTime;
        if (_health <= 0)
        {
            state = BlobState.Dormant;
            _health = 0;
            return;
            
        }

        Text.text = "Health:\n" + Mathf.CeilToInt(_health) + "\\" + ReadyHealth;
    }

    void LookForNutrient()
    {
        var nutrients = GameObject.FindObjectsOfType<Nutrient>();
        var bestDistance = Mathf.Infinity;
        foreach (var nutrient in nutrients)
        {
            var distance = Vector3.Distance(transform.position, nutrient.transform.position);
            
            if (distance < bestDistance)
            {
                _targetDestination = nutrient;
                bestDistance = distance;
                state = BlobState.GoingToFood;
            }
        }
    }

    void MoveBlob()
    {
        if (_targetDestination == null)
        {
            state = BlobState.Searching;
            return;
        }

        var td = _targetDestination.transform.position;
        var moveDirection = td - transform.position;
        var facing = moveDirection.x / Mathf.Abs(moveDirection.x);
        moveDirection.y = 0;

        if (Mathf.Abs(td.x - transform.position.x) < 1 && Mathf.Abs(td.y - transform.position.y) > 3 && !_targetDestination.Falling && _rigid.angularVelocity < 1f && _rigid.angularVelocity > -1f)
        {
            _audioPlayer.clip = JumpSFX;
            _audioPlayer.Play();

            
            _rigid.AddTorque(100 * facing);
        }
        else
        {
            if ((moveDirection.x < 0 && transform.localScale.x > 0) || (moveDirection.x > 0 && transform.localScale.x < 0))
            {
                var _scale = transform.localScale;
                _scale.x *= -1;
                transform.localScale = _scale;
            }
            transform.Translate(moveDirection.normalized * Time.deltaTime);
        }

        LookForNutrient();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (state == BlobState.Eating)
        {
            return;
        }
        
        if (collision.tag == "nutrient" && state == BlobState.GoingToFood)
        {
            state = BlobState.Eating;
            _meal = collision.GetComponent<Nutrient>();
            _meal.RegisterEating(this);
            StartCoroutine(Eat());
        }
    }

    private IEnumerator Eat()
    {
       while (state == BlobState.Eating)
       {
            var healthGain = _meal.Eat();
            if (healthGain != 0)
            {
                if (healthGain > 0)
                {
                    _audioPlayer.clip = EatSFX;
                }
                else
                {
                    _audioPlayer.clip = PoisonSFX;
                }

                _audioPlayer.Play();

                _health += healthGain;
                if (_health >= ReadyHealth)
                {
                    state = BlobState.Dormant;
                    _health = ReadyHealth;

                }

                if (_health <= 0)
                {
                    state = BlobState.Dormant;
                    _health = 0;
                }


            }
            else
            {
                _meal.FinishedEating(this);
                _meal = null;

                if (!IsDormant) state = BlobState.Searching;
            }
            yield return new WaitForSeconds(0.5f);
        }
        
        if (_meal != null)
        {
            _meal.FinishedEating(this);
            _meal = null;
        }
        
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        
        if (collision.transform.CompareTag("platform"))
        {
            _platformCount -= 1;
            if (_platformCount == 0)
            {
                state = BlobState.Falling;
                _startFalling = Time.time;
            }

        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("platform"))
        {
            _platformCount += 1;
            //transform.localRotation = Quaternion.identity;
            if (state == BlobState.Falling)
            {
                _health -= Time.time - _startFalling;
                state = BlobState.Searching;
            }
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public bool IsDormant { get { return state == BlobState.Dormant; } }
}
