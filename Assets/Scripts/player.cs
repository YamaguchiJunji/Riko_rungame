using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
    private int score = 0;
    public TextMeshProUGUI score_text;
    public GameObject cake;
    public AudioSource audioSource_birthdaysong;
    public AudioSource audioSource_bgm;
    private bool is_played_birthdaysong = false;
    public Image overlay;
    public GameObject birthday_card;
    public AudioSource audioSource_jump;
    public AudioSource audioSource_item;
    public ParticleSystem shine_effect;

    private bool isGameStarted = false;
    public GameObject game_start_text;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;

        headphone.SetActive(false);
        cap.SetActive(false);
        score_text.text = score.ToString();
        birthday_card.SetActive(false);
    }

    void Update()
    {
        if (!isGameStarted)
        {
            if (detect_start_tap())
            {
                isGameStarted = true;
                game_start_text.SetActive(false);

                if (audioSource_bgm != null && !audioSource_bgm.isPlaying)
                {
                    audioSource_bgm.Play();
                }
            }

            return;
        }

        float distance = Vector3.Distance(transform.position, cake.transform.position);

        if (distance < 2)
        {
            if (!is_played_birthdaysong)
            {
                audioSource_bgm.Pause();
                audioSource_birthdaysong.Play();
                is_played_birthdaysong = true;

                overlay.gameObject.SetActive(true);
                StartCoroutine(fade_in_overlay());
            }

            return;
        }

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

        if (detect_jump_tap())
        {
            jump();
        }

        if (transform.position.y < -10)
        {
            respawn();
        }
    }

    private bool detect_start_tap()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    return false;
                }

                return true;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return false;
            }

            return true;
        }

        return false;
    }

    private bool detect_jump_tap()
    {
        if (fixedJoystick == null) return false;

        RectTransform joystickRect = fixedJoystick.GetComponent<RectTransform>();

        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                if (touch.phase != TouchPhase.Began) continue;

                if (RectTransformUtility.RectangleContainsScreenPoint(joystickRect, touch.position))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(joystickRect, Input.mousePosition))
            {
                return false;
            }

            return true;
        }

        return false;
    }

    public void jump()
    {
        if (!isGameStarted) return;
        if (jumpCount >= 2) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpCount++;
        isInAir = true;

        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }

        scaleCoroutine = StartCoroutine(jump_scale_animation());
        audioSource_jump.Play();
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
            audioSource_item.Play();
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.CompareTag("cap"))
        {
            cap.SetActive(true);
            audioSource_item.Play();
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.CompareTag("spike"))
        {
            respawn();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("strawberry"))
        {
            score++;
            score_text.text = score.ToString();
            shine_effect.Play();
        }
    }

    private void respawn()
    {
        float x = transform.position.x;
        transform.position = new Vector3(transform.position.x - 10, -1, transform.position.z);
    }

    private IEnumerator fade_in_overlay()
    {
        float duration = 3f;
        float time = 0f;

        Color color = overlay.color;
        color.a = 0f;
        overlay.color = color;

        while (time < duration)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 0.8f, time / duration);
            overlay.color = color;
            yield return null;
        }

        color.a = 0.8f;
        overlay.color = color;
        birthday_card.SetActive(true);
    }
}