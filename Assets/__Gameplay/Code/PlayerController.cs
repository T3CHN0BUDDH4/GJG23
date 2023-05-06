﻿using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class PlayerController : ShootingDriver
{
    [Header("Camera")]
    public Transform camTarget; // თრანსფორმი რომელსაც ცინემაშინი დაყვება

    [Header("Movement")]
    public float automaticSpeed = 20f; // ავტომატური წინსვლის სისწრაფე
    public float steeringSpeed = 10f; // მოხვევის სისწრაფე
    public float steeringAcceleration = 7.5f;  // რამდენად მალე რაზგონდება მოსახვევად
    public float drivingSpeed = 10f; // ხელით წინსვლის სისწრაფე
    public float drivingAcceleration = 5f;  // რამდენად მალე რაზგონდება საწინსვლოდ

    public Transform boundTransform; // ბაუნდების ფარენთი
    public Transform max, min; // ფლეერი კედლების იქით რომ არ გავიდეს ტრანსფორმი შევადაროთ. კოლაიდერზე უფრო ოპტიმალურია, ნაკლებ ფიზიკაზე ვინერვიულებთ

    [Header("Screen Shake")]
    public float bulletShakeStrength = 10f;   // გასროლაზე სქრინ შეიქის რაოდენობა
    public float bulletShakeLength = 0.25f;   // გასროლაზე სქრინ შეიქის სიხანგრძლივე

    [Header("Shift")]
    public bool canShift = true;    // შეუძლია თუ არა დროში გადახტეს
    public bool is2003 = false; // დროის შემოწმების ბულიანი
    public float cooldown = 0.25f;  // რამდენი დრო ჭირდება რომ დრო დაშიფტოს

    public GameObject t2003, t2043;

    float verticalInput, horizontalInput;    // ინპუტი რომ შევინახოთ ფლეერის
    public bool shiftInput = false; // ფლეერის ნახტომის ინპუტი
    public bool shootInput = false; // ფლეერის სროლის ინპუტი
    float activeSteerSpeed, activeDriveSpeed; // ამ კონკრეტულ მომენტში აქსელერაცია სადამდეა მისული

    public CinemachineVirtualCamera cinemachineVC;   // ცინემაშინის კამერა სქრინ შეიქისთვის
    float shakeTime = 0;

    private void Start()
    {
        // თავიდანვე 2043ში რომ დაიყოს თამაში

        t2043.SetActive(true);
        t2003.SetActive(false);

        is2003 = false;
    }

    private void Update()
    {
        GetPlayerInput();
        ShakeTimer();
    }

    void GetPlayerInput()
    {
        verticalInput = Input.GetAxisRaw("Vertical");
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // დროში ნახტომის ინპუტის აღება
        if (Input.GetMouseButtonDown(1))
        {
            if (canShift) shiftInput = true;
        }

        // სროლის ინპუტის აღება
        if (Input.GetMouseButton(0))
        {
            if (canShoot) if (shootInput == false) shootInput = true;
        }
    }

    private void FixedUpdate()
    {
        Move();
        Steer();
        Drive();

        if (shiftInput)
        {
            shiftInput = false;
            Shift();
        }

        if (shootInput)
        {
            shootInput = false;
            Shoot();
        }
    }

    void Move()
    {
        transform.position += transform.right * automaticSpeed * Time.deltaTime; // წინსვლა


        boundTransform.position += boundTransform.right * automaticSpeed * Time.deltaTime; // ბაუნდების წინსვლა

        camTarget.position += camTarget.right * automaticSpeed * Time.deltaTime; // კამერა თარგეთის დააფდეითება
    }

    void Steer()
    {
        // აქსელერაციის დათვლა. თუ ინპუტი არ შემოდის ვასწრაფებთ. საპირისპირო შემთხვევაში ძალიან სრიალებს
        if (verticalInput == 0) activeSteerSpeed = Mathf.Lerp(activeSteerSpeed, verticalInput, steeringAcceleration * 1.5f * Time.deltaTime);
        else activeSteerSpeed = Mathf.Lerp(activeSteerSpeed, verticalInput, steeringAcceleration * Time.deltaTime);

        // მოხვევა
        if (activeSteerSpeed > 0)
        {
            // თუ ზედა კედელს არ ეხება, ზემოთ შეუძლია მოხვევა
            if (transform.position.y <= max.position.y) transform.position += transform.up * activeSteerSpeed * steeringSpeed * Time.deltaTime;
        }
        else if (activeSteerSpeed < 0)
        {
            // თუ ქვედა კედელს არ ეხება, ქვემოთ შეუძლია მოხვევა
            if (transform.position.y >= min.position.y) transform.position += transform.up * activeSteerSpeed * steeringSpeed * Time.deltaTime;
        }
    }

    void Drive()
    {
        // წინსვლის დათვლა. თუ ინპუტი არ შემოდის ვასწრაფებთ. საპირისპირო შემთხვევაში ძალიან სრიალებს
        if (horizontalInput == 0) activeDriveSpeed = Mathf.Lerp(activeDriveSpeed, horizontalInput, drivingAcceleration * 1.5f * Time.deltaTime);
        else activeDriveSpeed = Mathf.Lerp(activeDriveSpeed, horizontalInput, drivingAcceleration * Time.deltaTime);

        // მოხვევა
        if (activeDriveSpeed > 0)
        {
            // თუ ზედა კედელს არ ეხება, ზემოთ შეუძლია მოხვევა
            if (transform.position.x <= max.position.x) transform.position += transform.right * activeDriveSpeed * drivingSpeed * Time.deltaTime;
        }
        else if (activeDriveSpeed < 0)
        {
            // თუ ქვედა კედელს არ ეხება, ქვემოთ შეუძლია მოხვევა
            if (transform.position.x >= min.position.x) transform.position += transform.right * activeDriveSpeed * drivingSpeed * Time.deltaTime;
        }

        //  ასევე, ეს ზუსტად იგივე კოდია რაც Steer()ში გამოიყენება, უბრალოდ გამოცვლილი ცვლადებით. ძალიან ნიჭიერი პროგრამისტი ვარ. - Z
    }

    void Shift()
    {
        canShift = false;

        if (is2003) { t2043.SetActive(true); t2003.SetActive(false);  is2003 = false; }
        else { t2043.SetActive(false); t2003.SetActive(true);   is2003 = true; }

        // ნახტომის ქულდაუნზე გაშვება
        StartCoroutine(ShiftCooldown());
    }

    IEnumerator ShiftCooldown() { yield return new WaitForSeconds(cooldown); canShift = true; }

    void Shoot()
    {
        canShoot = false;

        // ტყვიის შექმნა და ტრანსფორმის ამოღება
        Transform newBullet = Instantiate(bulletPrefab, firingPoint.position, firingPoint.rotation).transform;
        // ტყვიაზე ცოტა სპრედის დადება
        newBullet.eulerAngles = new Vector3(newBullet.rotation.x, newBullet.rotation.y, Random.Range(-bulletSpread, bulletSpread));

        CameraShake(bulletShakeStrength, bulletShakeLength);

        // სროლის ქულდაუნზე გაშვება
        StartCoroutine(ShootCooldown());
    }
    IEnumerator ShootCooldown() { yield return new WaitForSeconds(firerate); canShoot = true; }

    void CameraShake(float strength, float length)
    {
        CinemachineBasicMultiChannelPerlin cvcp = cinemachineVC.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        cvcp.m_FrequencyGain = strength;
        shakeTime += length;
    }
    void ShakeTimer()
    {
        if(shakeTime > 0)
        {
            shakeTime -= Time.deltaTime;

            if (shakeTime <= 0)
            {
                CinemachineBasicMultiChannelPerlin cvcp = cinemachineVC.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

                cvcp.m_FrequencyGain = 1f;
            }
        }
    }


    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Bullet")
        {
            hP--;
            if (hP == 0)
            {
                DestroyCombatant();
            }
        }
        else if (other.tag == "Obstacle")
        {
            DestroyCombatant();
        }
    }
    public void DestroyCombatant()
    {
        // აქ იქნება სიკვდილის და რესტარტის ფუნქცია. ამჯერად უბრალოდ სცენა დარესეტდება

        SceneManager.LoadScene(0);
    }
}