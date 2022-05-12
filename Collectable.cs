using System.Collections;
using System.Collections.Generic;
using UnityEngine;

  public enum BonusCollectable{
    GREENCAN,
    REDCAN,
    TAPE
}
public class Collectable : MonoBehaviour
{
    private BonusCollectable collectableState;
    [SerializeField] IntVariable _playerScore;
    [SerializeField] IntVariable playerHP;
    [SerializeField] IntVariable _playerMaxHP;
    [SerializeField] AnimationCurve _PopCurve;
    public int _bonusGreenCanHP = 5;
    public int _bonusRedCanHP = 10;
    public int _bonusTapeScore = 200;
    public bool _isPoping = true;

    private Animator animator;
    private float _curveTimer;
    private int _bonus_value;
    private float _randomDirection;
    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        SetUpCollectible();
        oneTime = true;
        _randomDirection = Random.Range(-1, 1);
        
    }

    private void FixedUpdate()
    {
        if (_isPoping)
        {
            
            if (_curveTimer < 1f)
            {
                _curveTimer += Time.deltaTime;
                
                float y = _PopCurve.Evaluate(_curveTimer);
                transform.position = new Vector3(transform.position.x + 0.01f * _randomDirection, y, transform.position.z);
            }
        }
        else
        {
            _curveTimer = 0;
            _randomDirection = Random.Range(-1, 1);

        }
    }


    void SetUpCollectible() // set up collectble with the animation and bonus value chose in the inspector
    {
        switch (collectableState)
        {
            case BonusCollectable.GREENCAN:
                _bonus_value = _bonusGreenCanHP;
                animator.SetBool("GreenCan", true);
                break;
            case BonusCollectable.REDCAN:
                animator.SetBool("RedCan", true);
                _bonus_value = _bonusRedCanHP;

                break;
            case BonusCollectable.TAPE:
                animator.SetBool("Tape", true);
                _bonus_value = _bonusTapeScore;
                break;
            default:
                break;
        }

    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            if (oneTime)
            {
                    switch (collectableState)
                    {
                        case BonusCollectable.GREENCAN:
                        if (playerHP.value < _playerMaxHP.value)
                        {
                                playerHP.value += _bonus_value ;

                        }
                        else
                        {
                            playerHP.value = _playerMaxHP.value;
                        }
                            break;
                        case BonusCollectable.REDCAN:
                        if (playerHP.value < _playerMaxHP.value)
                        {
                            playerHP.value += _bonus_value;

                        }
                        else
                        {
                            playerHP.value = _playerMaxHP.value;
                        }
                        break;
                        case BonusCollectable.TAPE:
                                _playerScore.value += _bonus_value ;

                            break;
                        default:
                            break;
                    }
                oneTime = false;
                Destroy(gameObject);
                Debug.Log(_bonus_value);

            }
        }
    }

    private bool oneTime = true;
}

