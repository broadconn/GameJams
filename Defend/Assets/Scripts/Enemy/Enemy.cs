using System;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour {
    private NavMeshAgent _agent;

    private void Awake() {
        _agent = GetComponent<NavMeshAgent>();
    }

    public void Spawn(Vector3 spawnPos, Vector3 goalPosition) {
        _agent.Warp(spawnPos);
        var canFindPath = _agent.SetDestination(goalPosition);
        if (!canFindPath) {
            Debug.LogWarning("Enemy could not find a valid path! It might attack!!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("EnemyGoal")) return;
        GameController.Instance.OnEnemyReachedGoal();
        Die();
    }

    private void Die() {
        Destroy(gameObject);
    }
}