using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;


public class PlayerBehaviour : MonoBehaviour
{
    [Header("Touch Input")] 
    public Joystick joystick;
    [Range(0.01f, 1.0f)]
    public float sensitivity;
    
    [Header("Movement")] 
    public float horizontalForce;
    public float verticalForce;
    public bool isGrounded;
    public Transform groundOrigin;
    public float groundRadius;
    public LayerMask groundLayerMask;
    [Range(0.1f, 0.9f)]
    public float airControlFactor;

    [Header("Animation")] 
    public PlayerAnimationState state;

    [Header("Dust")]
    public ParticleSystem dusrTrail;
    public Color dustTrailColor;
    [Header("Sound FX")] 
    public AudioSource jumpSound;

    //[Header("Shaking")]
    //public CinemachineVirtualCamera virtualCamera;
    //public CinemachineBasicMultiChannelPerlin perlin;
    //public float shakeIntensity, maxTimeShake, shakeTimer;
    //public bool isCameraShaking;




    private Rigidbody2D rigidbody;
    private Animator animatorController;

    // Start is called before the first frame update
    void Start()
    {
        //isCameraShaking = false;
        //shakeTimer = maxTimeShake;
        rigidbody = GetComponent<Rigidbody2D>();
        animatorController = GetComponent<Animator>();
        jumpSound = GetComponent<AudioSource>();
        dusrTrail = GetComponentInChildren<ParticleSystem>();
       // perlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //if(isCameraShaking)
        //{
        //    shakeTimer -= Time.deltaTime;
        //    if(shakeTimer<=0.0f)
        //    {
        //        perlin.m_AmplitudeGain = 0.0f;
        //        shakeTimer = maxTimeShake;
        //        isCameraShaking = false;
        //    }
        //}
        Move();
        CheckIfGrounded();
    }
    //private void ShakerCamera()
    //{
    //    //perlin.m_AmplitudeGain = shakeIntensity;
    //    isCameraShaking = true;

    //}

    private void Move()
    {
        float x = (Input.GetAxisRaw("Horizontal") + joystick.Horizontal) * sensitivity ;

        if (isGrounded)
        {
            // Keyboard Input
            float y = (Input.GetAxisRaw("Vertical") + joystick.Vertical) * sensitivity;
            float jump = Input.GetAxisRaw("Jump") + ((UIController.jumpButtonDown) ? 1.0f : 0.0f);

            // jump activated
            if (jump > 0)
            {
                jumpSound.Play();
                //ShakerCamera();
            }

            // Check for Flip

            if (x != 0)
            {
                x = FlipAnimation(x);
                animatorController.SetInteger("AnimationState", (int) PlayerAnimationState.RUN); // RUN State
                state = PlayerAnimationState.RUN;
                CreateDustTrail();
            }
            else
            {
                animatorController.SetInteger("AnimationState", (int)PlayerAnimationState.IDLE); // IDLE State
                state = PlayerAnimationState.IDLE;
            }

            float horizontalMoveForce = x * horizontalForce;
            float jumpMoveForce = jump * verticalForce; 

            float mass = rigidbody.mass * rigidbody.gravityScale;


            rigidbody.AddForce(new Vector2(horizontalMoveForce, jumpMoveForce) * mass);
            rigidbody.velocity *= 0.99f; // scaling / stopping hack
        }
        else // Air Control
        {
            animatorController.SetInteger("AnimationState", (int)PlayerAnimationState.JUMP); // JUMP State
            state = PlayerAnimationState.JUMP;
            CreateDustTrail();
            if (x != 0)
            {
                x = FlipAnimation(x);

                float horizontalMoveForce = x * horizontalForce * airControlFactor;
                float mass = rigidbody.mass * rigidbody.gravityScale;

                rigidbody.AddForce(new Vector2(horizontalMoveForce, 0.0f) * mass);
            }
        }

    }

    private void CreateDustTrail()
    {
        dusrTrail.GetComponent<Renderer>().material.SetColor("_Color", dustTrailColor);
        dusrTrail.Play();
    }
    private void CheckIfGrounded()
    {
        RaycastHit2D hit = Physics2D.CircleCast(groundOrigin.position, groundRadius, Vector2.down, groundRadius, groundLayerMask);

        isGrounded = (hit) ? true : false;
    }

    private float FlipAnimation(float x)
    {
        // depending on direction scale across the x-axis either 1 or -1
        x = (x > 0) ? 1 : -1;

        transform.localScale = new Vector3(x, 1.0f);
        return x;
    }

    // EVENTS

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Platform"))
        {
            transform.SetParent(other.transform);
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Platform"))
        {
            transform.SetParent(null);
        }
    }

    // UTILITIES

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundOrigin.position, groundRadius);
    }

}
