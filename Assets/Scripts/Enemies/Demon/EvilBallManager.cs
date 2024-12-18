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
    
    private List<EvilBallBehaviour> _balls = new List<EvilBallBehaviour>();
    [CanBeNull] private EvilBallBehaviour _sentBall;
    [SerializeField] private EnemyMovementManager enemyMovementManager;
    private AudioSource _audioSource;
    
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void Init()
    {
        SpawnBalls();
    }

    private void SpawnBalls()
    {
        for (int i = 0; i < numberOfBalls; i++)
        {
            GameObject ball = BallPool.Instance.GetBall();
            EvilBallBehaviour ballBehaviour = ball.GetComponent<EvilBallBehaviour>();
            
            if (ballBehaviour != null)
            {
                ballBehaviour.Init(rotationSpeed, horizontalRadius, verticalRadius, i, numberOfBalls, transform, _audioSource);
            }
            _balls.Add(ballBehaviour);
        }
    }
    
    private void Update()
    {
        if (_sentBall != null && !_sentBall.gameObject.activeSelf)
        {
            _sentBall = null;
        }
        if (_sentBall == null && enemyMovementManager.Targeted && 
            Vector2.Distance(transform.position, enemyMovementManager.GetCurrentTargetPosition()) < distanceToSendBall)
        {
            SendBall();
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
