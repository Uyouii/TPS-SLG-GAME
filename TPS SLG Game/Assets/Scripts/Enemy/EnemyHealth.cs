using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int startingHealth = 100;
    public int currentHealth;
    public float sinkSpeed = 2.5f;
    public int scoreValue = 10;
    public AudioClip deathClip;
    public GameObject healthImage;
    //public GameObject blankImage;

    [HideInInspector]
    public int monsterID;

    [HideInInspector]
    public int monsterStyle;

    [HideInInspector]
    public bool isDead;

    [HideInInspector]
    public AudioSource enemyAudio;


    Animator anim;

    ParticleSystem hitParticles;
    CapsuleCollider capsuleCollider;
    int[] enemyInitHealth;

    bool isSinking;


    void Awake ()
    {
        anim = GetComponent <Animator> ();
        enemyAudio = GetComponent <AudioSource> ();
        hitParticles = GetComponentInChildren <ParticleSystem> ();
        capsuleCollider = GetComponent <CapsuleCollider> ();

        currentHealth = startingHealth;
        isDead = false;
        enemyInitHealth = new int[] { 100, 150, 300 };
    }


    void Update ()
    {
        if(isSinking)
        {
            transform.Translate (-Vector3.up * sinkSpeed * Time.deltaTime);
        }
        healthImage.transform.LookAt(Camera.main.transform);
        //blankImage.transform.LookAt(Camera.main.transform);
    }

    public void SetHealthImage()
    {
        float health_value = GameSettings.HEALTH_INIT_WIDTH * currentHealth / enemyInitHealth[monsterStyle];
        healthImage.GetComponent<RectTransform>().sizeDelta = new Vector2(
            health_value,
            GameSettings.HEALTH_INIT_HEIGHT
        );
        healthImage.GetComponent<RectTransform>().anchoredPosition3D = new Vector2(
            (GameSettings.HEALTH_INIT_WIDTH - health_value) / 2,
            healthImage.GetComponent<RectTransform>().anchoredPosition3D.y);
    }


    public void TakeDamage (int amount, Vector3 hitPoint)
    {
        if(isDead)
            return;

        //enemyAudio.Play ();

        //currentHealth -= amount;
            
        hitParticles.transform.position = hitPoint;
        hitParticles.Play();

        //if(currentHealth <= 0)
        //{
        //    Death ();
        //}
    }


    public void Death ()
    {
        isDead = true;

        capsuleCollider.isTrigger = true;

        anim.SetTrigger ("Dead");

        enemyAudio.clip = deathClip;
        enemyAudio.Play ();
    }


    public void StartSinking ()
    {
        isSinking = true;
        Destroy (gameObject, 2f);
    }
}
