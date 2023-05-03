using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public Animator anim;

    [Header("Health Bar:")]
    public Slider healthBar;
    public float currentHealth;
    public float maxHealth = 100;

    [Header("Hearts")]
    public Image[] hearts;
    public int livesRemaining;
    public bool isFlickering = false;
    public float timeDelay;

    [Header("Enemy Dealing Damage")]
    //public EnemyDamage damage;
    public GameObject loserScreen;

    [Header("Damage & Death Audio:")]
    public AudioSource deathAudioSource;
    public AudioSource painAudioSource;
    public AudioClip[] painClips;

    void Start()
    {
        currentHealth = maxHealth; // Set current health to full 
        healthBar.value = currentHealth; // Set bar to player health which is max health 
    }

    private void Update()
    {
        if (currentHealth == 0)
        {
            // If player isn't alive reset current health to max health
            if (!IsPlayerAlive() && livesRemaining != 0)
            {
                if (isFlickering == false)
                {
                    StartCoroutine(FlickeringImage());
                    Debug.Log("I've flickered on and off");
                    StartCoroutine(LostLife());
                    Debug.Log("I lost a life" + " " + livesRemaining);
                }
                currentHealth = maxHealth;
                healthBar.value = currentHealth;
                Debug.Log("Health has reset to max health" + " " + currentHealth);
            }
        }
    }

    public bool IsPlayerAlive()
    {
        return healthBar.value > 0;
    }

    // Function is called anytime the player is taking damage
    public void TakingDamage(int amount) // amount = how much damage the player takes 
    {
        healthBar.value -= amount;
        currentHealth = healthBar.value;
        painAudioSource.PlayOneShot(painClips[Random.Range(0, painClips.Length - 1)]);
        //Debug.Log("Im taking damage");
    }


    public void LoseLife()
    {
        // Decrease lives remaining
        livesRemaining--;
        
        // Hide a life image 
        hearts[livesRemaining].enabled = false;

        // if run out of lives then its ggs
        if (livesRemaining == 0)
        {
            currentHealth = 0;
            healthBar.value = currentHealth;
            isFlickering = false;
            deathAudioSource.Play();
            anim.SetTrigger("dead");
            StartCoroutine(Death());
            Debug.Log("You lost and play death animation then display lost UI screen");
        }
        // If no lives remaining, do nothing
        if (livesRemaining == 0)
        {
            return;
        }
    }

    IEnumerator FlickeringImage()
    {
        isFlickering = true;
        hearts[livesRemaining -1].enabled = false;
        timeDelay = Random.Range(0.01f, 0.2f);
        yield return new WaitForSeconds(timeDelay);
        hearts[livesRemaining -1].enabled = true;
        timeDelay = Random.Range(0.01f, 0.2f);
        yield return new WaitForSeconds(timeDelay);
        isFlickering = false;
    }

    IEnumerator LostLife()
    {
        yield return new WaitForSeconds(1.5f);
        LoseLife();
    }

    public IEnumerator Death()
    {
        yield return new WaitForSeconds(3.5f);
        Loser();
        UnlockMouse();
        Debug.Log("Game over" + " " + loserScreen);
    }

    public void Loser()
    {
        loserScreen.SetActive(true);
        Time.timeScale = 0;
    }

    void UnlockMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}