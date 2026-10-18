using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class player : MonoBehaviour
{
    private float speed = 10f;
    private float jumpForce = 8f;
    private int jumpCount = 0;
    private Rigidbody2D rb;

    private float currentSpeed = 0f;
    private float accelerationTime = 0.5f;

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;
    private bool isInAir = false;

    private float squashScale = 0.5f;
    private float stretchScale = 1.25f;
    private float squashTime = 0.06f;
    private float stretchTime = 0.1f;
    private float returnTime = 0.12f;

    private float landingSquashTime = 0.03f;
    private float landingReturnTime = 0.05f;

    public GameObject headphone;
    public GameObject cap;
    public FixedJoystick fixedJoystick;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;

        headphone.SetActive(false);
        cap.SetActive(false);
    }

    void Update()
    {
        bool right = Input.GetKey(KeyCode.RightArrow);
        bool left = Input.GetKey(KeyCode.LeftArrow);

        float joystickHorizontal = fixedJoystick != null ? fixedJoystick.Horizontal : 0f;

        if (joystickHorizontal > 0.01f)
        {
            right = true;
        }
        else if (joystickHorizontal < -0.01f)
        {
            left = true;
        }

        float acceleration = speed / accelerationTime;

        if (right && !left)
        {
            if (transform.localScale.x <= 0)
            {
                flip_character();
                currentSpeed = 0f;
            }

            currentSpeed = Mathf.MoveTowards(currentSpeed, speed, acceleration * Time.deltaTime);
            transform.position += Vector3.right * currentSpeed * Time.deltaTime;
        }
        else if (left && !right)
        {
            if (transform.localScale.x > 0)
            {
                flip_character();
                currentSpeed = 0f;
            }

            currentSpeed = Mathf.MoveTowards(currentSpeed, speed, acceleration * Time.deltaTime);
            transform.position += Vector3.left * currentSpeed * Time.deltaTime;
        }
        else
        {
            currentSpeed = 0f;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jump();
        }

        if (transform.position.y < -10)
        {
            respawn();
        }
    }

    public void jump()
    {
        if (jumpCount >= 2) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpCount++;
        isInAir = true;

        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }

        scaleCoroutine = StartCoroutine(jump_scale_animation());
    }

    private IEnumerator jump_scale_animation()
    {
        yield return scale_y(squashScale, squashTime);
        yield return scale_y(stretchScale, stretchTime);
        yield return scale_y(1f, returnTime);
        scaleCoroutine = null;
    }

    private IEnumerator landing_scale_animation()
    {
        yield return scale_y(squashScale, landingSquashTime);
        yield return scale_y(1f, landingReturnTime);
        scaleCoroutine = null;
    }

    private IEnumerator scale_y(float targetScaleRate, float duration)
    {
        float startY = transform.localScale.y;
        float targetY = originalScale.y * targetScaleRate;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float y = Mathf.Lerp(startY, targetY, time / duration);
            transform.localScale = new Vector3(transform.localScale.x, y, transform.localScale.z);
            yield return null;
        }

        transform.localScale = new Vector3(transform.localScale.x, targetY, transform.localScale.z);
    }

    private void flip_character()
    {
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            jumpCount = 0;

            if (isInAir)
            {
                isInAir = false;

                if (scaleCoroutine != null)
                {
                    StopCoroutine(scaleCoroutine);
                }

                scaleCoroutine = StartCoroutine(landing_scale_animation());
            }
        }

        if (collision.gameObject.CompareTag("headphone"))
        {
            headphone.SetActive(true);
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.CompareTag("cap"))
        {
            cap.SetActive(true);
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.CompareTag("spike"))
        {
            respawn();
        }
    }

    private void respawn()
    {
        float x = transform.position.x;
        transform.position = new Vector3(transform.position.x - 10, -1, transform.position.z);
    }
}