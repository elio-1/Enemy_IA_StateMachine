using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState{
    IDLE,
    WALK,
    ATTACK,
    ATTACK2,
    BACKJUMP,
    TAKINGDAMAGE,
    DEAD
}
public class EnemyStateMachine : MonoBehaviour
{
    [SerializeField] protected EnemyState currentState;
    [HideInInspector] public int enemyHP;
    [SerializeField] IntVariable _enemyMaxHp;
    [Header ("Movement")]
    [SerializeField] float _speed = 10f;

    [Header ("Detections")]
    [SerializeField] float _detectionRadius = 2f;
    [SerializeField] float _attackRange = 0.75f;
    [SerializeField] protected LayerMask _playerLayer;
    [SerializeField] IntVariable playerHP;
     Transform _player;

    [Header ("Attack")]
    [SerializeField] float _attackDuration;
    [SerializeField] float _moveWhileAttacking = 2;
    [SerializeField] float _dashWhileAttackingTimer = 0.15f;
    [SerializeField] protected GameObject _groundAtackCollider;
    [SerializeField] protected Vector2 _groundAtackSize;
    [SerializeField] float _attackSpeed = 1f;
    [SerializeField] IntVariable _enemyDamage;
    private int _enemyAttackPower;

    [Header("Jump")]
    [SerializeField] float _backjumpDuration = .5f;
    [SerializeField] float _backjumpForce = 14f;

    [Header("Death & Damage")]
    [SerializeField] float _damageKnockBack = 1.5f;
    [SerializeField] float _deathDuration = 2f;
    [SerializeField] float _blinkRate = 0.3f;
    [SerializeField] GameObject _collectible;
    [SerializeField] AudioClip _deathSound;

    private Rigidbody2D enemyRb;
    private Animator enemyAnimator;
    private Vector2 _direction;
    private bool _playerInWalkRange = false;
    private bool _playerInAttackRange = false;
    private bool _canDoublePunch = false;
    private int _canDealDamage = 2;
    private AudioSource audioSource;

    private float _attackDurationTimer =0;
    private float _jumpDurationTimer = 0;
    private float _deathTimer = 0;
    private float _blinkTimer = 0;
    private float _attackSpeedTimer = 0;
    private int _enemyIsLosingHp;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        currentState = EnemyState.IDLE;
        enemyRb = GetComponent<Rigidbody2D>();
        enemyAnimator = GetComponentInChildren<Animator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _deathTimer = 0;

        _player = GameObject.FindWithTag("Player").transform.Find("pos");
        _enemyAttackPower = _enemyDamage.value;
        enemyHP = _enemyMaxHp.value;
        _enemyIsLosingHp = enemyHP;
    }
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void Update()
    {
        DetectPlayerWalkRange();
        DetectPlayerAttackRange();
        Flip();
        OnStateUpdate();
        _direction = _player.transform.position - transform.position;
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
                _attackSpeedTimer = 0;

                break;
            case EnemyState.WALK:
                _attackSpeedTimer = 0;

                break;
            case EnemyState.ATTACK:
                enemyAnimator.SetTrigger("Attacking");
                _canDealDamage = 2;
                break;
            case EnemyState.ATTACK2:
                _canDealDamage = 1;
                enemyAnimator.SetTrigger("Attacking2");

                break;
            case EnemyState.BACKJUMP:
                enemyAnimator.SetTrigger("BackJump");
                break;
            case EnemyState.TAKINGDAMAGE:
                audioSource.Play();
                enemyAnimator.SetTrigger("TakingDamage");
                break;
            case EnemyState.DEAD:
                AudioSource.PlayClipAtPoint(_deathSound, transform.position);
                GameObject prefab = Instantiate(_collectible, this.transform);
                prefab.transform.parent = null;
                enemyRb.bodyType = RigidbodyType2D.Static;
                enemyAnimator.SetBool("IsDead", true);
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
                EnemyAttack(_enemyAttackPower);
                _canDoublePunch = false;
                break;
            case EnemyState.ATTACK2:
                enemyAnimator.ResetTrigger("Attacking2");
                _attackDurationTimer = 0;
                EnemyAttack(_enemyAttackPower);
                _canDealDamage = 0;
                _canDoublePunch = false;
                _attackSpeedTimer = 0;
                break;
            case EnemyState.BACKJUMP:
                enemyAnimator.ResetTrigger("BackJump");
                _jumpDurationTimer = 0;
                break;
            case EnemyState.TAKINGDAMAGE:
                enemyAnimator.ResetTrigger("TakingDamage");
                _jumpDurationTimer = 0;
                _attackSpeedTimer = 0;

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
                    _attackSpeedTimer += Time.deltaTime;

                    if (_playerInAttackRange && _attackSpeedTimer > _attackSpeed)
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
                EnemyDead();
                EnemyLosingHp();
                break;
            case EnemyState.WALK:
                
                enemyAnimator.SetFloat("MoveSpeed", _direction.magnitude);

                _attackSpeedTimer += Time.deltaTime;

                if (_playerInAttackRange && _attackSpeedTimer > _attackSpeed)
                {
                    TransitionToState(EnemyState.ATTACK);
                }
                if (!_playerInWalkRange)
                {
                    TransitionToState(EnemyState.IDLE);
                }
                EnemyLosingHp();
                EnemyDead();
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

                EnemyLosingHp();
                EnemyDead();
                break;

            case EnemyState.ATTACK2:
                _attackDurationTimer += Time.deltaTime;

                if (_attackDurationTimer > _attackDuration)
                {
                    TransitionToState(EnemyState.BACKJUMP);
                }

                EnemyLosingHp();
                EnemyDead();
                break;
            case EnemyState.BACKJUMP:
                _jumpDurationTimer += Time.deltaTime;
                if (_jumpDurationTimer > _backjumpDuration)
                {
                    TransitionToState(EnemyState.IDLE);
                }

                EnemyLosingHp();
                EnemyDead();
                break;
            case EnemyState.TAKINGDAMAGE:
                _jumpDurationTimer += Time.deltaTime;
                if (_jumpDurationTimer > _backjumpDuration)
                {
                    TransitionToState(EnemyState.IDLE);
                }
                EnemyDead();
                break;
            case EnemyState.DEAD:
                _deathTimer += Time.deltaTime;
                _blinkTimer += Time.deltaTime;
                if (_blinkTimer > _blinkRate)
                {
                    _spriteRenderer.enabled = !_spriteRenderer.enabled;
                    _blinkTimer = 0;
                }
                if (_deathTimer > _deathDuration)
                {
                    gameObject.SetActive(false);
                }
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
            case EnemyState.TAKINGDAMAGE:
                enemyRb.velocity = _direction.normalized * _damageKnockBack* -_backjumpForce * Time.deltaTime;
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

    private void EnemyAttack(int damage)
    {
        Collider2D puchCol = Physics2D.OverlapBox(_groundAtackCollider.transform.position, _groundAtackSize, 0, _playerLayer);
        
        if (puchCol != null && _canDealDamage > 0 )
        {
            Debug.Log(puchCol);
            //playerHP.value -= damage;
            puchCol.transform.gameObject.GetComponentInParent<PlayerManagement>().Damage(damage);
        }
    }
    private void Flip()
    {
        if (_direction.x < 0)
        {
            //transform.Rotate(new Vector3(0, 180, 0));
            transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);


        }
    }

    private void EnemyDead()
    {
        if (enemyHP <= 0)
        {
            TransitionToState(EnemyState.DEAD);
        }
    }

    private void EnemyLosingHp()
    {
        if (enemyHP<_enemyIsLosingHp)
        {
            TransitionToState(EnemyState.TAKINGDAMAGE);
            _enemyIsLosingHp = enemyHP;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);
        Gizmos.DrawWireSphere(transform.position, _attackRange);
        Gizmos.DrawWireCube(_groundAtackCollider.transform.position, _groundAtackSize);
    }
}
