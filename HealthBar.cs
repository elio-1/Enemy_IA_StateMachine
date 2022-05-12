using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Slider _healthBar;

    // for testing
    [SerializeField] IntVariable _maxHealth;

    [SerializeField] IntVariable _currentHealth;
    // for testing

    void Start()
    {
        _healthBar = GetComponentInChildren<Slider>();
        _healthBar.maxValue = _maxHealth.value;
    }

    void Update()
    {
        _healthBar.value = _currentHealth.value;
    }
}
