using System;
using UnityEngine;

public class CarHandler : MonoBehaviour
{
    [SerializeField]
    Rigidbody rb;
    [SerializeField]
    Transform gameModel;
    [Header("SFX")]
    [SerializeField]
    AudioSource carEngineAS;
    [SerializeField]
    AnimationCurve carPitchAnimationCurve;

    bool isPlayer = false;
    bool isCrashed = false;

    public static event Action OnPlayerCrashed;

    // Max values
    float maxSteerVelocity = 5;
    float maxForwardVelocity = 30;

    // Multiplier
    float accelerationMultiplier = 3;
    float breaksMultiplier = 15;
    float steeringMultiplier = 8;

    Vector2 input = Vector2.zero;

    void Start()
    {
        isPlayer = CompareTag("Player");

        if (isPlayer)
        {
            carEngineAS.Play();
        }
    }

    void Update()
    {
        gameModel.transform.rotation = Quaternion.Euler(0, rb.linearVelocity.x * 5, 0);

        if (isCrashed)
            FadeOutCarAudio();
        else
            UpdateCarAudio();
    }

    private void FixedUpdate()
    {
        if (isCrashed)
            return;

        if (input.y > 0)
            Accelerate();
        else
            rb.linearDamping = 0.2f;

        if (input.y < 0)
            Brake();

        Steer();

        if (rb.linearVelocity.z < 0)
            rb.linearVelocity = Vector3.zero;
    }

    void Accelerate()
    {
        rb.linearDamping = 0;

        if (rb.linearVelocity.z >= maxForwardVelocity)
            return;

        rb.AddForce(rb.transform.forward * accelerationMultiplier * input.y);
    }

    void Brake()
    {
        if (rb.linearVelocity.z <= 0)
            return;

        rb.AddForce(rb.transform.forward * breaksMultiplier * input.y);
    }

    void Steer()
    {
        if (Mathf.Abs(input.x) > 0)
        {
            float speedBaseSteerLimit = rb.linearVelocity.z / 5.0f;
            speedBaseSteerLimit = Mathf.Clamp01(speedBaseSteerLimit);

            rb.AddForce(rb.transform.right * steeringMultiplier * input.x * speedBaseSteerLimit);

            float normalizedX = rb.linearVelocity.x / maxSteerVelocity;
            normalizedX = Mathf.Clamp(normalizedX, -1.0f, 1.0f);

            rb.linearVelocity = new Vector3(normalizedX * maxSteerVelocity, 0, rb.linearVelocity.z);
        }
        else
        {
            rb.linearVelocity = Vector3.Lerp(
                rb.linearVelocity,
                new Vector3(0, 0, rb.linearVelocity.z),
                Time.fixedDeltaTime * 3);
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (!isPlayer || isCrashed)
            return;

        Transform other = collision.collider.transform;

        
        bool isObstacle = false;
        for (Transform t = other; t != null; t = t.parent)
        {
            if (t.CompareTag("Obstacle"))
            {
                isObstacle = true;
                break;
            }
        }

        if (!isObstacle)
            return;

        isCrashed = true;
        input = Vector2.zero;
        rb.linearDamping = 2f;

        OnPlayerCrashed?.Invoke();
    }


    void UpdateCarAudio()
    {
        if(!isPlayer)
            return;
        
        float carMaxSpeedPercentage = rb.linearVelocity.z / maxForwardVelocity;

        carEngineAS.pitch = carPitchAnimationCurve.Evaluate(carMaxSpeedPercentage);
    }

    void FadeOutCarAudio()
    {
        if (!isPlayer)
            return;

        carEngineAS.volume = Mathf.Lerp(carEngineAS.volume, 0, Time.deltaTime * 10);

        if (carEngineAS.volume < 0.01f)
            carEngineAS.Stop();
    }

    public void SetInput(Vector2 inputVector)
    {
        if (isCrashed)
            return;

        inputVector.Normalize();
        input = inputVector;
    }
}