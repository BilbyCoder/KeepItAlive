using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Transform LeftEnd;
    public Transform RightEnd;
    public Transform SpawnerSprite;
    public Nutrient NutrientPrefab;
    public Nutrient PoisonPrefab;
    public float MinWaitTime;
    public float MaxWaitTime;
    public int MaxEdibles;
    public float Speed;
    // Start is called before the first frame update

    private float _nextEdible;
    private float _moveDirection;
    private float _moveValue;
    private float _speed;

    private int _spawnerCount;
    
    void Start()
    {
        _moveValue = Random.Range(0, 1f);
        _moveDirection = Random.Range(0, 2) == 0 ? -1 : 1;
        _speed = 1 / Vector3.Distance(LeftEnd.position, RightEnd.position) * Speed;
        _nextEdible = Time.time + Random.Range(0.0f, MinWaitTime);
    }

    // Update is called once per frame
    void Update()
    {

        MoveSpawner();

        if (_spawnerCount < MaxEdibles && Time.time > _nextEdible)
        {
            SpawnEdible();
        }
    }

    void MoveSpawner()
    {
        _moveValue += Time.deltaTime * _moveDirection * _speed;

        if (_moveValue < 0)
        {
            _moveValue = 0;
            _moveDirection *= -1;
        }

        if (_moveValue > 1)
        {
            _moveValue = 1;
            _moveDirection *= -1;
        }

        SpawnerSprite.position = Vector3.Lerp(LeftEnd.position, RightEnd.position, _moveValue);
    }

    void SpawnEdible()
    {
        var spawn = Random.Range(0, 7) < 4 ? NutrientPrefab : PoisonPrefab;
        Instantiate<Nutrient>(spawn, new Vector3(SpawnerSprite.position.x, SpawnerSprite.position.y - 0.25f, SpawnerSprite.position.z), Quaternion.identity).RegisterSpawner(this);
        _spawnerCount += 1;
        if (_spawnerCount < MaxEdibles)
        {
            SetNextSpawnTimer();
        }
    }

    void SetNextSpawnTimer()
    {
        _nextEdible = Time.time + Random.Range(MinWaitTime, MaxWaitTime);
    }


    public void DeRegister()
    {
        _spawnerCount -= 1;
        if (_spawnerCount < MaxEdibles)
        {
            SetNextSpawnTimer();
        }
    }

}
