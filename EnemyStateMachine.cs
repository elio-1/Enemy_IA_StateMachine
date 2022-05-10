using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState{
    IDLE,
    WALK,
    ATTACK,
    ATTACK2,
    BACKJUMP,
    DEAD
}
public class EnemyStateMachine : MonoBehaviour
{
    [SerializeField] protected EnemyState currentState;
    public int enemyPV;
    [Header ("Movement")]
    [SerializeField] float _speed = 10f;

    [Header ("Detections")]
    [SerializeField] float _detectionRadius = 2f;
    [SerializeField] float _attackRange = 0.75f;
    [SerializeField] protected LayerMask _playerLayer;
    [SerializeField] GameObject _player;

    [Header ("Attack")]
    [SerializeField] float _attackDuration;
    public int _enemyAttackPower;
    [SerializeField] float _moveWhileAttacking = 2;
    [SerializeField] float _dashWhileAttackingTimer = 0.15f;
    [SerializeField] protected GameObject _groundAtackCollider;
    [SerializeField] protected float _groundAtackSize = 0.1f;
    [SerializeField] float _punchTimerCooldown = 1f;

    [Header("Jump")]
    [SerializeField] float _backjumpDuration = .5f;
    [SerializeField] float _backjumpForce = 14f;


    private Rigidbody2D enemyRb;
    private Animator enemyAnimator;
    private Vector2 _direction;
    private bool _playerInWalkRange = false;
    private bool _playerInAttackRange = false;
    private bool _canDoublePunch = false;
    private float _attackDurationTimer =0;
    private float _jumpDurationTimer = 0;

    private void Start()
    {
        currentState = EnemyState.IDLE;
        enemyRb = GetComponent<Rigidbody2D>();
        enemyAnimator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        DetectPlayerWalkRange();
        DetectPlayerAttackRange();
        Flip();
        OnStateUpdate();
    }
    private void FixedUpdate()
    {
        OnStateFixedUpdate();
    }

    void OnStateEnter()
    {
        switch (currentState)
        {
            case EnemyState.IDLE:

                break;
            case EnemyState.WALK:
                break;
            case EnemyState.ATTACK:
                enemyAnimator.SetTrigger("Attacking");
                EnemyAttack();
                break;
            case EnemyState.ATTACK2:
                enemyAnimator.SetTrigger("Attacking2");
                EnemyAttack();

                break;
            case EnemyState.BACKJUMP:
                enemyAnimator.SetTrigger("BackJump");
                break;
            case EnemyState.DEAD:
                break;
            
            default:
                break;
        }

    }
    void OnStateExit()
    {
        switch (currentState)
        {
            case EnemyState.IDLE:
                
                break;
            case EnemyState.WALK:

                break;
            case EnemyState.ATTACK:
                enemyAnimator.ResetTrigger("Attacking");
                _attackDurationTimer = 0;
                _canDoublePunch = false;
                break;
            case EnemyState.ATTACK2:
                enemyAnimator.ResetTrigger("Attacking2");
                _attackDurationTimer = 0;
                _canDoublePunch = false;
                break;
            case EnemyState.BACKJUMP:
                enemyAnimator.ResetTrigger("BackJump");
                _jumpDurationTimer = 0;
                break;
            case EnemyState.DEAD:
                break;
            default:
                break;
        }
    }

    void OnStateUpdate()
    {
        switch (currentState)
        {
            case EnemyState.IDLE:
                enemyAnimator.SetFloat("MoveSpeed", 0f);
                if (_playerInWalkRange)
                {
                    if (_playerInAttackRange)
                    {
                        TransitionToState(EnemyState.ATTACK);
                    }
                    else
                    {
                        TransitionToState(EnemyState.WALK);
                    }
                }
                else
                {
                    enemyRb.velocity = Vector2.zero;
                }

                break;
            case EnemyState.WALK:
                _direction = _player.transform.position - transform.position;
                enemyAnimator.SetFloat("MoveSpeed", _direction.magnitude);
                if (_playerInAttackRange)
                {
                    TransitionToState(EnemyState.ATTACK);
                }
                if (!_playerInWalkRange)
                {
                    TransitionToState(EnemyState.IDLE);
                }
                break;
            case EnemyState.ATTACK:
                _attackDurationTimer += Time.deltaTime;
                if (_attackDurationTimer > _attackDuration)
                {
                    TransitionToState(EnemyState.IDLE);
                    _canDoublePunch = true;
                }
                if (_canDoublePunch)
                {
                    TransitionToState(EnemyState.ATTACK2);
                }
                    break;

            case EnemyState.ATTACK2:
                _attackDurationTimer += Time.deltaTime;

                if (_attackDurationTimer > _attackDuration)
                {
                    TransitionToState(EnemyState.BACKJUMP);
                }
                break;
            case EnemyState.BACKJUMP:
                _jumpDurationTimer += Time.deltaTime;
                if (_jumpDurationTimer > _backjumpDuration)
                {
                    TransitionToState(EnemyState.IDLE);
                }
                break;
            case EnemyState.DEAD:
                break;
            default:
                break;
        }
    }
    void OnStateFixedUpdate(){
        switch (currentState)
        {
            case EnemyState.IDLE:
                break;
            case EnemyState.WALK:
                enemyRb.velocity = _direction.normalized * _speed * Time.deltaTime;
                break;
            case EnemyState.ATTACK:

                if (_attackDurationTimer < _dashWhileAttackingTimer)
                {
                    enemyRb.velocity = _direction.normalized * _moveWhileAttacking * Time.deltaTime;
                }
                else
                {
                    enemyRb.velocity = Vector2.zero;
                }
                break;
            case EnemyState.ATTACK2:

                if (_attackDurationTimer < _dashWhileAttackingTimer)
                {
                    enemyRb.velocity = _direction.normalized * _moveWhileAttacking * Time.deltaTime;
                }
                else
                {
                    enemyRb.velocity = Vector2.zero;
                }
                break;
            case EnemyState.BACKJUMP:
                enemyRb.velocity = _direction.normalized * -_backjumpForce * Time.deltaTime;
                break;
            case EnemyState.DEAD:
                break;
            default:
                break;
        }
    }
    public void TransitionToState(EnemyState newState)
    {
       OnStateExit();
       currentState = newState;
       OnStateEnter();
    }

    private void DetectPlayerWalkRange()
    {
        Collider2D detectionRange = Physics2D.OverlapCircle(transform.position, _detectionRadius, _playerLayer);
        if (detectionRange != null)
        {
            _playerInWalkRange = true;
        }else
        {
            _playerInWalkRange = false;
        }
    }
    private void DetectPlayerAttackRange()
    {
        Collider2D detectionRange = Physics2D.OverlapCircle(transform.position, _attackRange, _playerLayer);
        if (detectionRange != null)
        {
            _playerInAttackRange = true;
        }
        else
        {
            _playerInAttackRange = false;
        }
    }

    private void EnemyAttack()
    {
        Collider2D puchCol = Physics2D.OverlapCircle(_groundAtackCollider.transform.position, _groundAtackSize, _playerLayer);

        if (puchCol != null)
        {
            Debug.Log("hit");
            //playerhp -= damage
        }
    }
    private void Flip()
    {
        if (_direction.x < 0)
        {
            //transform.Rotate(new Vector3(0, 180, 0));
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);


        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);
        Gizmos.DrawWireSphere(transform.position, _attackRange);
        Gizmos.DrawWireSphere(_groundAtackCollider.transform.position, _groundAtackSize);
    }
}
