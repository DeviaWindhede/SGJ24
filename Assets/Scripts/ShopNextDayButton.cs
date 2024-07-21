using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopNextDayButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    private Animator _animator;
    private bool _isVisible;
    private bool _hasFinishedAnimation = false;

    public bool HasFinishedAnimation => _hasFinishedAnimation;
    public Button Button => _button;

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        Button.onClick.AddListener(OnNewDayButton);
        Button.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        Button.onClick.RemoveListener(OnNewDayButton);
    }

    private void OnNewDayButton()
    {
        StartCoroutine(NewDay());
    }

    private IEnumerator NewDay()
    {
        _button.enabled = false;

        StartCoroutine(Disable());
        yield return null;

        while (!_hasFinishedAnimation)
        {
            yield return null;
        }

        _button.enabled = true;
        PersistentShopData.Instance.shopTime.ResetDay();
        FindObjectOfType<PlayerBehaviour>().ResetPosition();
        ResetGooberActiveStates();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ResetGooberActiveStates()
    {
        var goobers = PersistentShopData.Instance.shopResources.goobers;
        for (int i = 0; i < goobers.Count; i++)
        {
            goobers[i].petPercentage = 0.0f;
            foreach (var state in goobers[i].activeStates)
            {
                state.dirtiness = 0.0f;
            }
        }
    }

    public void SetVisibility(bool aIsVisible)
    {
        if (_isVisible == aIsVisible) { return; }

        _isVisible = aIsVisible;
        if (aIsVisible)
        {
            StartCoroutine(Enable());
            return;
        }

        StartCoroutine(Disable());
    }

    private IEnumerator Enable()
    {
        _button.gameObject.SetActive(true);
        _animator.SetTrigger("Enable");
        yield return null;

        _hasFinishedAnimation = false;

        while (true)
        {
            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("CloseShopActiveIdle")) { break; }

            yield return null;
        }

        _hasFinishedAnimation = true;
    }

    private IEnumerator Disable()
    {
        _animator.SetTrigger("Disable");

        yield return null;
        _hasFinishedAnimation = false;

        while (true)
        {
            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("CloseShopInactiveIdle")) { break; }

            yield return null;
        }

        _button.gameObject.SetActive(false);
        _hasFinishedAnimation = true;
    }
}
