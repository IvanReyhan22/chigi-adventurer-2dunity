using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player_Controller : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private Collider2D coll;
    private enum State { idle, running, jumping, falling, hurt }
    private State state = State.idle;


    [SerializeField] private int fruit_orange = 0;
    [SerializeField] private LayerMask ground;
    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float knockForce = 5f;
    [SerializeField] private Text orange_count;
    [SerializeField] private int health;
    [SerializeField] private Text Health;
    [SerializeField] private AudioSource fruit_audio;
    [SerializeField] private AudioSource footstep;
    [SerializeField] private AudioSource enemy_hit;
    [SerializeField] private AudioSource hurt_audio;
    [SerializeField] private AudioSource restart_game;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
        Health.text = health.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (state != State.hurt)
        {
            Movement();
        }

        AnimationState();
        anim.SetInteger("State", (int)state);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Collectible")
        {
            Destroy(collision.gameObject);

            fruit_audio.Play();

            fruit_orange += 1;
            orange_count.text = fruit_orange.ToString();
        }

        if (collision.tag == "PowerUp")
        {
            Destroy(collision.gameObject);

            jumpForce = jumpForce * 2;
            GetComponent<SpriteRenderer>().color = Color.yellow;
            StartCoroutine(ResetPower());
        }

        if (collision.tag == "NextLevel")
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    private void OnCollisionEnter2D(Collision2D enterCollision)
    {

        if (enterCollision.gameObject.tag == "Enemy")
        {
            if (state == State.falling)
            {
                enemy_hit.Play();
                Destroy(enterCollision.gameObject);
            }
            else
            {
                hurt_audio.Play();
                state = State.hurt;
                HandleHealth();

                if (enterCollision.gameObject.transform.position.x > transform.position.x)
                {
                    rb.velocity = new Vector2(-knockForce, rb.velocity.y);
                }
                else
                {
                    rb.velocity = new Vector2(knockForce, rb.velocity.y);
                }
            }
        }

        if (enterCollision.gameObject.tag == "InstantDeath")
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void HandleHealth()
    {
        health -= 1;
        Health.text = health.ToString();

        if (health <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            restart_game.Play();
        }
    }

    private void Movement()
    {
        float HDirection = Input.GetAxis("Horizontal");

        if (HDirection > 0)
        {
            rb.velocity = new Vector2(speed, rb.velocity.y);
            transform.localScale = new Vector2(1, 1);
        }
        else if (HDirection < 0)
        {
            rb.velocity = new Vector2(-speed, rb.velocity.y);
            transform.localScale = new Vector2(-1, 1);
        }
        else
        {

        }

        if (Input.GetButtonDown("Jump") && coll.IsTouchingLayers(ground))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            state = State.jumping;
        }

        if (!coll.IsTouchingLayers(ground))
        {
            state = State.falling;
        }
    }

    private void AnimationState()
    {
        if (state == State.jumping)
        {
            if (rb.velocity.y < .1f)
            {
                state = State.falling;
            }
        }
        else if (state == State.falling)
        {
            if (coll.IsTouchingLayers(ground))
            {
                state = State.idle;
            }
        }
        else if (Mathf.Abs(rb.velocity.x) > .2f)
        {
            state = State.running;
        }
        else
        {
            state = State.idle;
        }
    }

    private IEnumerator ResetPower()
    {
        yield return new WaitForSeconds(10);

        jumpForce = jumpForce / 2;

        GetComponent<SpriteRenderer>().color = Color.white;
    }

    private void FootStep()
    {
        footstep.Play();
    }
}
