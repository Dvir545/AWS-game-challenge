using System;
using System.Collections;
using System.Collections.Generic;
using Enemies;
using Enemies.Demon;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Pool;

public class EvilBallManager : MonoBehaviour
{
    [SerializeField] private int numberOfBalls = 4;
    [SerializeField] private float horizontalRadius = 2f;
    [SerializeField] private float verticalRadius = 1f;
    [SerializeField] private float rotationSpeed = 2f;
    
    [SerializeField] private float distanceToSendBall = 6f;
    [SerializeField] private float timeToSpawnNewBall = 3f;
    private float _timeToSpawnNewBall;
    private List<EvilBallBehaviour> _balls = new List<EvilBallBehaviour>();
    [CanBeNull] private EvilBallBehaviour _sentBall;
    [SerializeField] private EnemyMovementManager enemyMovementManager;
    [SerializeField] private EnemyHealthManager enemyHealthManager;
    private AudioSource _audioSource;
    private int _curHealth;  // used to get indication of enemy hits
    
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void Init()
    {
        _timeToSpawnNewBall = 0f;
        _curHealth = enemyHealthManager.MaxHealth;
        SpawnBalls();
    }

    private void SpawnBall(int index, bool updatePos = false)
    {
        GameObject ball = BallPool.Instance.GetBall();
        EvilBallBehaviour ballBehaviour = ball.GetComponent<EvilBallBehaviour>();
            
        if (ballBehaviour != null)
        {
            ballBehaviour.Init(rotationSpeed, horizontalRadius, verticalRadius, index, numberOfBalls, transform, _audioSource);
        }
        _balls.Add(ballBehaviour);
        if (updatePos)
            for (int i = 0; i < _balls.Count; i++)
                if (i != index)
                    _balls[i].UpdateBallPosition(i, _balls.Count);
    }

    private void SpawnBalls()
    {
        for (int i = 0; i < numberOfBalls; i++)
        {
            SpawnBall(i);
        }
    }
    
    private void Update()
    {
        if (enemyHealthManager.IsDead)
        {
            if (_balls.Count > 0)
            {
                foreach (var ball in _balls) {
                    ball.Release();
                }
                _balls.Clear();
            }

            return;
        }
        if (_sentBall != null && !_sentBall.gameObject.activeSelf)
        {
            _sentBall = null;
        }
        if (_sentBall == null && enemyMovementManager.Targeted && 
            Vector2.Distance(transform.position, enemyMovementManager.GetCurrentTargetPosition()) < distanceToSendBall)
        {
            SendBall();
        }

        if (_balls.Count < numberOfBalls)
        {
            if (_curHealth != enemyHealthManager.CurHealth)  // on hit, reset timer
            {
                _curHealth = enemyHealthManager.CurHealth;
                _timeToSpawnNewBall = 0f;
            }
            _timeToSpawnNewBall += Time.deltaTime;
            if (_timeToSpawnNewBall >= timeToSpawnNewBall)
            {
                SpawnBall(_balls.Count, true);
                _timeToSpawnNewBall = 0f;
            }
        }
    }

    private void SendBall()
    {
        if (_sentBall != null && _sentBall.gameObject.activeSelf)
            return;
        if (_balls.Count == 0)
            return;
        // get index of ball closest to the player
        float closestDistance = float.MaxValue;
        int closestIndex = 0;
        for (int i = 0; i < _balls.Count; i++)
        {
            var distance = Vector2.Distance(_balls[i].transform.position, enemyMovementManager.GetCurrentTargetPosition());
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }
        _sentBall = _balls[closestIndex];
        _balls.RemoveAt(closestIndex);
        _sentBall!.Send();
        // adjust other balls positions
        for (int i = 0; i < _balls.Count; i++)
        {
            EvilBallBehaviour ball = _balls[i];
            // Calculate new position based on the new total number of balls
            ball.UpdateBallPosition(i, _balls.Count);
        }
    }
}
